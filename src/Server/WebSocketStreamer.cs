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