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
        private readonly WebSocket _webSocket;
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
            WebSocket webSocket)
        {
            _networkServer = networkServer;
            _webSocket = webSocket;
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
            INetworkClient networkClient)
        {
            // ReSharper disable once MethodSupportsCancellation
            // Background task
            var receiveTask = Task.Run(
                async () =>
                {
                    using var memoryOwner = MemoryPool<byte>.Shared.Rent();
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
                                    received = await _webSocket
                                        .ReceiveAsync(
                                            memory.Slice(receivedBytes),
                                            CancellationToken)
                                        .ConfigureAwait(false);
                                    receivedBytes += received.Count;
                                } while (received.EndOfMessage == false);
                            }

                            // The port forward stream first sends port number:
                            // [Stream index][High port byte][Low port byte]
                            if (receivedBytes <= 3)
                            {
                                continue;
                            }

                            // When port number has been sent, data is sent:
                            // [Stream index][Data 1]..[Data n]
                            await networkClient
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
                    using var memoryOwner = MemoryPool<byte>.Shared.Rent();
                    var memory = memoryOwner.Memory;
                    // The port forward stream looks like this when sending:
                    // [Stream index][Data 1]..[Data n]
                    memory.Span[0] = (byte) ChannelIndex.StdIn;
                    while (_cancellationTokenSource.IsCancellationRequested ==
                           false)
                    {
                        try
                        {
                            var bytesRec = await networkClient
                                .ReceiveAsync(
                                    memory.Slice(1),
                                    CancellationToken)
                                .ConfigureAwait(false);
                            if (bytesRec == 0)
                            {
                                continue;
                            }

                            using (await _webSocketSendGate
                                .WaitAsync(CancellationToken)
                                .ConfigureAwait(false))
                            {
                                await _webSocket
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