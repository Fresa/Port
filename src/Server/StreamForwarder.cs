using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using Log.It;

namespace Port.Server
{
    internal sealed class StreamForwarder : IAsyncDisposable
    {
        private readonly INetworkServer _networkServer;
        private readonly WebSocketStreamer _remoteSocket;
        private const int Stopped = 0;
        private const int Started = 1;
        private int _status = Stopped;

        private readonly SemaphoreSlimGate _webSocketGate =
            SemaphoreSlimGate.OneAtATime;

        private readonly CancellationTokenSource _cancellationTokenSource =
            new CancellationTokenSource();

        private CancellationToken CancellationToken
            => _cancellationTokenSource.Token;

        private readonly List<Task> _backgroundTasks = new List<Task>();

        private readonly ILogger _logger = LogFactory.Create<StreamForwarder>();

        private StreamForwarder(
            INetworkServer networkServer,
            WebSocket remoteSocket)
        {
            _networkServer = networkServer;
            _remoteSocket = new WebSocketStreamer(
                remoteSocket, _cancellationTokenSource);
        }

        private CancellationToken TimeOutCancellationToken =>
            CancellationTokenSource.CreateLinkedTokenSource(
                    new CancellationTokenSource(TimeSpan.FromSeconds(5))
                        .Token,
                    CancellationToken)
                .Token;

        internal static IAsyncDisposable Start(
            INetworkServer networkServer,
            WebSocket webSocket)
        {
            return new StreamForwarder(networkServer, webSocket)
                .Start();
        }

        public IAsyncDisposable Start()
        {
            var previousStatus = Interlocked.Exchange(ref _status, Started);
            if (previousStatus == Started)
            {
                return this;
            }

            _backgroundTasks.Add(StartForwarding());
            return this;
        }

        private async Task StartForwarding()
        {
            while (_cancellationTokenSource.IsCancellationRequested ==
                   false)
            {
                try
                {
                    var client = await _networkServer
                        .WaitForConnectedClientAsync(CancellationToken)
                        .ConfigureAwait(false);

                    await StartCrossWiring(client);
                }
                catch when (_cancellationTokenSource
                    .IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.Fatal(ex, "Unknown error, closing down");
#pragma warning disable 4014
                    //Dispose and exit fast
                    //This will most likely change when we need to report
                    //back that the forwarding terminated or that we
                    //should retry
                    DisposeAsync();
#pragma warning restore 4014
                    return;
                }
            }
        }

        private async Task StartCrossWiring(
            INetworkClient localSocket)
        {
            try
            {
                await StartTransferDataFromLocalToRemoteSocket(localSocket)
                    .ConfigureAwait(false);
            }
            catch (TaskCanceledException ex)
            {
                _logger.Info(ex, "Request was cancelled");
            }
            finally
            {
                await localSocket.DisposeAsync()
                    .ConfigureAwait(false);
                _logger.Debug("Local socket disconnected");
            }
        }

        private async Task StartTransferDataFromLocalToRemoteSocket(
            INetworkClient localSocket)
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(65536);
            var memory = memoryOwner.Memory;
            try
            {
                _logger.Info("Receiving from local socket");
                var bytesReceived = await localSocket
                    .ReceiveAsync(
                        memory,
                        TimeOutCancellationToken)
                    .ConfigureAwait(false);

                _logger.Info(
                    "Sending {bytes} bytes to remote socket",
                    bytesReceived + 1);

                using (await _webSocketGate
                    .WaitAsync(CancellationToken)
                    .ConfigureAwait(false))
                {
                    var sendResult = await _remoteSocket
                        .SendAsync(memory.Slice(0, bytesReceived))
                        .ConfigureAwait(false);
                    if (sendResult.IsCanceled)
                    {
                        _cancellationTokenSource.Cancel();
                        return;
                    }

                    var httpResponseContentLength = 0;
                    var httpResponseHeaderLength = 0;
                    var totalReceivedBytes = 0;
                    while (true)
                    {
                        var content =
                            await _remoteSocket.ReceiveAsync(
                                    TimeSpan.FromSeconds(5))
                                .ConfigureAwait(false);

                        if (content.IsCanceled)
                        {
                            break;
                        }
                        
                        foreach (var sequence in content.Buffer)
                        {
                            if (httpResponseContentLength == 0)
                            {
                                (httpResponseHeaderLength,
                                        httpResponseContentLength) =
                                    sequence
                                        .GetHttpResponseLength();
                            }

                            await localSocket
                                .SendAsync(
                                    sequence,
                                    CancellationToken)
                                .ConfigureAwait(false);

                            totalReceivedBytes += sequence.Length;
                        }
                        
                        if (totalReceivedBytes == httpResponseHeaderLength +
                            httpResponseContentLength)
                        {
                            _logger.Info(
                                "Received {total} bytes in total, exiting",
                                totalReceivedBytes);
                            break;
                        }

                        if (content.IsCompleted)
                        {
                            _cancellationTokenSource.Cancel();
                            return;
                        }
                    }

                    if (sendResult.IsCompleted)
                    {
                        _cancellationTokenSource.Cancel();
                    }
                }
            }
            catch when (_cancellationTokenSource
                .IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Failed transfer data from local machine to kubernetes");
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cancellationTokenSource.Cancel(false);
            _cancellationTokenSource.Dispose();

            try
            {
                await _networkServer.DisposeAsync();
            }
            catch
            {
                // Ignore unhandled exceptions during shutdown 
            }

            try
            {
                await _remoteSocket.DisposeAsync();
            }
            catch
            {
                // Ignore unhandled exceptions during shutdown 
            }

            _webSocketGate.Dispose();
            await Task.WhenAll(_backgroundTasks);
        }
    }

    internal class WebSocketStreamer : IAsyncDisposable
    {
        private readonly WebSocket _webSocket;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private CancellationToken CancellationToken
            => _cancellationTokenSource.Token;

        private readonly ILogger _logger =
            LogFactory.Create<WebSocketStreamer>();

        private readonly Pipe _receivingPipe = new Pipe();
        private readonly Pipe _sendingPipe = new Pipe();

        private readonly List<Task> _backgroundJobs = new List<Task>();

        public WebSocketStreamer(
            WebSocket webSocket,
            CancellationTokenSource cancellationTokenSource)
        {
            _webSocket = webSocket;
            _cancellationTokenSource = cancellationTokenSource;

            _backgroundJobs.Add(StartReceivingJobAsync());
            _backgroundJobs.Add(StartSendingJobAsync());
        }

        private readonly SemaphoreSlimGate _writeLock =
            SemaphoreSlimGate.OneAtATime;

        public async Task<FlushResult> SendAsync(
            ReadOnlyMemory<byte> buffer)
        {
            using (await _writeLock.WaitAsync(CancellationToken)
                .ConfigureAwait(false))
            {
                return await _sendingPipe.Writer.WriteAsync(
                        buffer, CancellationToken)
                    .ConfigureAwait(false);
            }
        }

        private readonly SemaphoreSlimGate _readLock =
            SemaphoreSlimGate.OneAtATime;

        public async Task<ReadResult> ReceiveAsync(
            TimeSpan timeout)
        {
            using (await _readLock.WaitAsync(CancellationToken)
                .ConfigureAwait(false))
            {
                var timeoutLock = new SemaphoreSlim(0);
                var task = _receivingPipe.Reader
                    .ReadAsync(CancellationToken)
                    .AsTask();

                _ = task.ContinueWith(
                    _ =>
                        timeoutLock.Release(), CancellationToken);

                var released = await timeoutLock
                    .WaitAsync(timeout, CancellationToken)
                    .ConfigureAwait(false);
                if (!released)
                {
                    _receivingPipe.Reader.CancelPendingRead();
                }

                var result = await task.ConfigureAwait(false);
                var bytes = result.Buffer.ToArray();
                _receivingPipe.Reader.AdvanceTo(
                    result.Buffer.GetPosition(bytes.Length));
                _logger.Trace("Received {bytes} from remote", bytes.Length);
                return new ReadResult(
                    new ReadOnlySequence<byte>(bytes), result.IsCanceled,
                    result.IsCompleted);
            }
        }

        private async Task StartReceivingJobAsync()
        {
            Exception? exception = null;
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(65536);
            var memory = memoryOwner.Memory;
            try
            {
                while (_cancellationTokenSource.IsCancellationRequested ==
                       false)
                {
                    try
                    {
                        var receivedBytes = 0;
                        ValueWebSocketReceiveResult received;
                        do
                        {
                            _logger.Info("Receiving from remote socket");
                            received = await _webSocket
                                .ReceiveAsync(
                                    memory[receivedBytes..],
                                    CancellationToken)
                                .ConfigureAwait(false);
                            receivedBytes += received.Count;

                            _logger.Info(
                                "Received {@received} from remote socket",
                                received);

                            if (received.MessageType ==
                                WebSocketMessageType.Close)
                            {
                                _logger.Info(
                                    "Received close message from remote, closing...");
                                await _webSocket.CloseOutputAsync(
                                        WebSocketCloseStatus.NormalClosure,
                                        "Close received",
                                        CancellationToken)
                                    .ConfigureAwait(false);

                                return;
                            }

                            if (received.Count == 0 &&
                                received.EndOfMessage == false)
                            {
                                throw new InvalidOperationException(
                                    "Received 0 bytes from socket, but socket indicates there is more data. Is there enough room in the memory buffer?");
                            }
                        } while (received.EndOfMessage == false);

                        if (receivedBytes == 0)
                        {
                            _logger.Info(
                                "Received 0 bytes, aborting remote socket");
                            _cancellationTokenSource.Cancel(false);
                            return;
                        }

                        // The port forward stream first sends port number:
                        // [Stream index][High port byte][Low port byte]
                        if (receivedBytes <= 3)
                        {
                            continue;
                        }

                        var content = memory[1..receivedBytes];
                        _logger.Info(
                            "Sending {bytes} bytes to local socket",
                            content.Length);

                        var result = await _receivingPipe.Writer.WriteAsync(
                                content, CancellationToken)
                            .ConfigureAwait(false);
                        if (result.IsCompleted || result.IsCanceled)
                        {
                            _cancellationTokenSource.Cancel();
                            return;
                        }
                    }
                    catch when (_cancellationTokenSource
                        .IsCancellationRequested)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        _logger.Fatal(ex, "Unknown error, closing down");
                        _cancellationTokenSource.Cancel(false);
                        return;
                    }
                }
            }
            finally
            {
                await _receivingPipe.Writer.CompleteAsync(exception)
                    .ConfigureAwait(false);
            }
        }

        private async Task StartSendingJobAsync()
        {
            Exception? exception = null;
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(65536);
            var memory = memoryOwner.Memory;
            // The port forward stream looks like this when sending:
            // [Stream index][Data 1]..[Data n]
            memory.Span[0] = (byte) ChannelIndex.StdIn;
            try
            {
                while (_cancellationTokenSource.IsCancellationRequested ==
                       false)
                {
                    try
                    {
                        var result = await _sendingPipe.Reader
                            .ReadAsync(CancellationToken)
                            .ConfigureAwait(false);

                        _sendingPipe.Reader.AdvanceTo(
                            result.Buffer.Start, result.Buffer.End);

                        if (result.IsCompleted || result.IsCanceled)
                        {
                            _cancellationTokenSource.Cancel();
                            return;
                        }

                        _logger.Info(
                            "Sending {bytes} bytes to remote socket",
                            result.Buffer.Length + 1);

                        result.Buffer.CopyTo(
                            memory.Slice(1)
                                .Span);

                        await _webSocket
                            .SendAsync(
                                memory.Slice(0, (int) result.Buffer.Length + 1),
                                WebSocketMessageType.Binary, false,
                                CancellationToken)
                            .ConfigureAwait(false);
                    }
                    catch when (_cancellationTokenSource
                        .IsCancellationRequested)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        _logger.Fatal(ex, "Unknown error, closing down");
                        _cancellationTokenSource.Cancel(false);
                        return;
                    }
                }
            }
            finally
            {
                await _sendingPipe.Reader.CompleteAsync(exception)
                    .ConfigureAwait(false);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _sendingPipe.Writer.CompleteAsync()
                .ConfigureAwait(false);
            await _receivingPipe.Reader.CompleteAsync()
                .ConfigureAwait(false);
            try
            {
                await _webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure, "Closing",
                        CancellationToken.None)
                    .ConfigureAwait(false);
                _webSocket.Dispose();
            }
            catch
            {
                // Try closing the socket
            }

            await Task.WhenAll(_backgroundJobs.ToArray());
        }
    }
}