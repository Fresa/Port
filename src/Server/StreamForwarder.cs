using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using Log.It;
using Microsoft.AspNetCore.Components.RenderTree;

namespace Kubernetes.PortForward.Manager.Server
{
    internal sealed class StreamForwarder : IAsyncDisposable
    {
        private readonly INetworkServer _networkServer;
        private readonly WebSocket _webSocket;
        private const int Stopped = 0;
        private const int Started = 1;
        private int _status = Stopped;

        private readonly CancellationTokenSource _cancellationTokenSource =
            new CancellationTokenSource();

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
                                    _cancellationTokenSource.Token)
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
                            ValueWebSocketReceiveResult received;
                            var receivedBytes = 0;
                            do
                            {
                                received = await _webSocket
                                    .ReceiveAsync(
                                        memory.Slice(receivedBytes), _cancellationTokenSource.Token)
                                    .ConfigureAwait(false);
                                receivedBytes += received.Count;
                            } while (received.EndOfMessage == false);

                            // The port forward stream first sends port number:
                            // [Stream index][High port byte][Low port byte]
                            if (receivedBytes <= 3)
                            {
                                continue;
                            }

                            // When port number has been sent, data is sent:
                            // [Stream index][Data 1]..[Data n]
                            await networkClient
                                .SendAsync(memory.Slice(1, receivedBytes - 1))
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

            var sendTask = Task.Run(
                async () =>
                {
                    using var memoryOwner = MemoryPool<byte>.Shared.Rent();
                    var memory = memoryOwner.Memory;
                    while (_cancellationTokenSource.IsCancellationRequested ==
                           false)
                    {
                        // The port forward stream looks like this when sending:
                        // [Stream index][Data 1]..[Data n]
                        memory.Span[0] = (byte) ChannelIndex.StdIn;
                        try
                        {
                            var bytesRec = await networkClient
                                .ReceiveAsync(
                                    memory.Slice(1),
                                    _cancellationTokenSource.Token)
                                .ConfigureAwait(false);
                            if (bytesRec == 0)
                            {
                                continue;
                            }

                            await _webSocket
                                .SendAsync(
                                    memory.Slice(0, bytesRec + 1),
                                    WebSocketMessageType.Binary, true,
                                    _cancellationTokenSource.Token)
                                .ConfigureAwait(false);
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
        }
    }
}