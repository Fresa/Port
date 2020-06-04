using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using Log.It;

namespace Kubernetes.PortForward.Manager.Server
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

            // ReSharper disable once MethodSupportsCancellation
            // Background task
            var task = Task.Run(
                async () =>
                {
                    while (_cancellationTokenSource.IsCancellationRequested ==
                           false)
                    {
                        try
                        {
                            var client = await _networkServer
                                .WaitForConnectedClientAsync(
                                    CancellationToken)
                                .ConfigureAwait(false);

                            StartCrossWiring(client);
                        }
                        catch when (_cancellationTokenSource
                            .IsCancellationRequested)
                        {
                            return;
                        }
                    }
                });
            _backgroundTasks.Add(task);
            return this;
        }

        private void StartCrossWiring(
            INetworkClient localSocket)
        {
            // ReSharper disable once MethodSupportsCancellation
            // Background task
            var receiveTask = Task.Run(
                async () =>
                {
                    using var memoryOwner = MemoryPool<byte>.Shared.Rent(65536);
                    var memory = memoryOwner.Memory;
                    while (_cancellationTokenSource.IsCancellationRequested ==
                           false)
                    {
                        try
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
                                            memory.Slice(receivedBytes),
                                            CancellationToken)
                                        .ConfigureAwait(false);
                                    receivedBytes += received.Count;
                                    _logger.Info("Received {bytes}/{total} from remote socket, {next}", received.Count, receivedBytes, received.EndOfMessage ? "done" : "continuing");
                                    
                                    if (received.Count == 0 && received.EndOfMessage == false)
                                    {
                                        throw new InvalidOperationException("Received 0 bytes from socket, but socket indicates there is more data. Is there enough room in the memory buffer?");
                                    }
                                } while (received.EndOfMessage == false);
                            }

                            if (receivedBytes == 0)
                            {
                                _logger.Info("Received 0 bytes, aborting remote socket");
                                return;
                            }

                            // The port forward stream first sends port number:
                            // [Stream index][High port byte][Low port byte]
                            if (receivedBytes <= 3)
                            {
                                continue;
                            }

                            _logger.Info("Sending {bytes} bytes to local socket", receivedBytes - 1);
                            // When port number has been sent, data is sent:
                            // [Stream index][Data 1]..[Data n]
                            await localSocket
                                .SendAsync(memory.Slice(1, receivedBytes - 1), CancellationToken)
                                .ConfigureAwait(false);
                        }
                        catch when (_cancellationTokenSource
                            .IsCancellationRequested)
                        {
                            return;
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(
                                ex, "Failed receiving from k8s websocket");
                            throw;
                        }
                    }
                });

            _backgroundTasks.Add(receiveTask);

            // ReSharper disable once MethodSupportsCancellation
            // Background task
            var sendTask = Task.Run(
                async () =>
                {
                    using var memoryOwner = MemoryPool<byte>.Shared.Rent(65536);
                    var memory = memoryOwner.Memory;
                    // The port forward stream looks like this when sending:
                    // [Stream index][Data 1]..[Data n]
                    memory.Span[0] = (byte) ChannelIndex.StdIn;
                    while (_cancellationTokenSource.IsCancellationRequested ==
                           false)
                    {
                        try
                        {
                            _logger.Info("Receiving from local socket");
                            var bytesRec = await localSocket
                                .ReceiveAsync(
                                    memory.Slice(1),
                                    CancellationToken)
                                .ConfigureAwait(false);
                            if (bytesRec == 0)
                            {
                                _logger.Info("Received 0 bytes, aborting local socket");
                                return;
                            }

                            using (await _webSocketSendGate
                                .WaitAsync(CancellationToken)
                                .ConfigureAwait(false))
                            {
                                _logger.Info("Sending {bytes} bytes to remote socket", bytesRec + 1);
                                await _remoteSocket
                                    .SendAsync(
                                        memory.Slice(0, bytesRec + 1),
                                        WebSocketMessageType.Binary, true,
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
                });

            _backgroundTasks.Add(sendTask);
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