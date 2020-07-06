using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
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
            }
        }

        private async Task StartCrossWiring(
            INetworkClient localSocket)
        {
            await Task.WhenAll(
                StartTransferDataFromRemoteToLocalSocket(localSocket),
                StartTransferDataFromLocalToRemoteSocket(localSocket));
            await localSocket.DisposeAsync();
            _logger.Debug("Local socket disconnected");
        }

        private async Task StartTransferDataFromLocalToRemoteSocket(
            INetworkClient localSocket)
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(65536);
            var memory = memoryOwner.Memory;
            // The port forward stream looks like this when sending:
            // [Stream index][Data 1]..[Data n]
            memory.Span[0] = (byte)ChannelIndex.StdIn;
            try
            {
                _logger.Info("Receiving from local socket");
                var bytesReceived = await localSocket
                    .ReceiveAsync(
                        memory[1..],
                        CancellationToken)
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
                            CancellationToken)
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
                _logger.Error(ex, "Failed receiving from host");
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
                                    CancellationToken)
                                .ConfigureAwait(false);
                            receivedBytes += received.Count;

                            _logger.Info(
                                "Received {@received} from remote socket",
                                received);

                            if (received.MessageType ==
                                WebSocketMessageType.Close)
                            {
                                await _remoteSocket.CloseOutputAsync(
                                        WebSocketCloseStatus.NormalClosure,
                                        "Close received", CancellationToken)
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
                            CancellationToken)
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
                _logger.Error(ex, "Failed receiving from k8s websocket");
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cancellationTokenSource.Cancel();

            await Task.WhenAll(_backgroundTasks);

            _webSocketReceiveGate.Dispose();
            _webSocketSendGate.Dispose();
        }
    }
}