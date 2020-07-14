using System;
using System.Buffers;
using System.Collections.Generic;
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
        private readonly WebSocket _remoteSocket;
        private const int Stopped = 0;
        private const int Started = 1;
        private int _status = Stopped;

        private readonly SemaphoreSlimGate _webSocketReceiveGate =
            SemaphoreSlimGate.OneAtATime;

        private readonly SemaphoreSlimGate _webSocketSendGate =
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
            _remoteSocket = remoteSocket;
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
                await StartTransferDataFromRemoteToLocalSocket(localSocket)
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
            // The port forward stream looks like this when sending:
            // [Stream index][Data 1]..[Data n]
            memory.Span[0] = (byte) ChannelIndex.StdIn;
            try
            {
                _logger.Info("Receiving from local socket");
                var bytesReceived = await localSocket
                    .ReceiveAsync(
                        memory[1..],
                        TimeOutCancellationToken)
                    .ConfigureAwait(false);

                using (await _webSocketSendGate
                    .WaitAsync(CancellationToken)
                    .ConfigureAwait(false))
                {
                    _logger.Info(
                        "Sending {bytes} bytes to remote socket",
                        bytesReceived + 1);
                    await _remoteSocket
                        .SendAsync(
                            memory.Slice(0, bytesReceived + 1),
                            WebSocketMessageType.Binary, false,
                            TimeOutCancellationToken)
                        .ConfigureAwait(false);
                }
            }
            catch when (_cancellationTokenSource
                .IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed transfer data from local machine to kubernetes");
                throw;
            }
        }

        private async Task StartTransferDataFromRemoteToLocalSocket(
            INetworkClient localSocket)
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(65536);
            var memory = memoryOwner.Memory;
            try
            {
                var httpResponseContentLength = 0;
                var httpResponseHeaderLength = 0;
                var totalReceivedBytes = 0;
                while (true)
                {
                    var receivedBytes = 0;
                    using (await _webSocketReceiveGate
                        .WaitAsync(CancellationToken)
                        .ConfigureAwait(false))
                    {
                        ValueWebSocketReceiveResult received;
                        do
                        {
                            _logger.Info("Receiving from remote socket");
                            received = await _remoteSocket
                                .ReceiveAsync(
                                    memory[receivedBytes..],
                                    TimeOutCancellationToken)
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
                                await _remoteSocket.CloseOutputAsync(
                                        WebSocketCloseStatus.NormalClosure,
                                        "Close received",
                                        CancellationToken)
                                    .ConfigureAwait(false);
                                _cancellationTokenSource.Cancel(false);
                                return;
                            }

                            if (received.Count == 0 &&
                                received.EndOfMessage == false)
                            {
                                throw new InvalidOperationException(
                                    "Received 0 bytes from socket, but socket indicates there is more data. Is there enough room in the memory buffer?");
                            }
                        } while (received.EndOfMessage == false);
                    }

                    if (receivedBytes == 0)
                    {
                        _logger.Info(
                            "Received 0 bytes, aborting remote socket");
                        return;
                    }

                    // The port forward stream first sends port number:
                    // [Stream index][High port byte][Low port byte]
                    if (receivedBytes <= 3)
                    {
                        continue;
                    }

                    // When port number has been sent, data is sent:
                    // [Stream index][Data 1]..[Data n]
                    var content = memory[1..receivedBytes];
                    _logger.Info(
                        "Sending {bytes} bytes to local socket",
                        content.Length);

                    if (httpResponseContentLength == 0)
                    {
                        (httpResponseHeaderLength, httpResponseContentLength) =
                            content
                                .GetHttpResponseLength();
                    }

                    await localSocket
                        .SendAsync(
                            content,
                            TimeOutCancellationToken)
                        .ConfigureAwait(false);

                    totalReceivedBytes += content.Length;
                    if (totalReceivedBytes == httpResponseHeaderLength +
                        httpResponseContentLength)
                    {
                        _logger.Info(
                            "Received {total} bytes in total, exiting",
                            totalReceivedBytes);
                        break;
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
                _logger.Error(ex, "Failed transfer data from kubernetes to local machine");
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            _cancellationTokenSource.Cancel(false);

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
                await _remoteSocket.CloseOutputAsync(
                    WebSocketCloseStatus.NormalClosure, "Closing",
                    CancellationToken.None);
            }
            catch
            {
                // Ignore unhandled exceptions during shutdown 
            }
            finally
            {
                _remoteSocket.Dispose();
            }

            _webSocketReceiveGate.Dispose();
            _webSocketSendGate.Dispose();

            await Task.WhenAll(_backgroundTasks);
        }
    }
}