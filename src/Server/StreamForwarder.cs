using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using k8s;

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

        public StreamForwarder(
            INetworkServer networkServer,
            WebSocket webSocket)
        {
            _networkServer = networkServer;
            _webSocket = webSocket;
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
                            var read = await _webSocket
                                .ReceiveAsync(
                                    memory, _cancellationTokenSource.Token)
                                .ConfigureAwait(false);

                            // The port forward stream looks like this when receiving:
                            // [Stream index][High port byte][Low port byte][Data 1]..[Data n]
                            if (read.Count <= 3)
                            {
                                continue;
                            }

                            await networkClient.SendAsync(memory.Slice(1))
                                .ConfigureAwait(false);
                        }
                        catch when (_cancellationTokenSource
                            .IsCancellationRequested)
                        {
                            return;
                        }
                    }
                });

            _backgroundTasks.Add(receiveTask);

            var sendTask = Task.Run(
                async () =>
                {
                    using var memoryOwner = MemoryPool<byte>.Shared.Rent();
                    var memory = memoryOwner.Memory;
                    // The port forward stream looks like this when sending:
                    // [Stream index][Data 1]..[Data n]
                    memory.Span[0] = (byte)ChannelIndex.StdIn;
                    while (_cancellationTokenSource.IsCancellationRequested ==
                           false)
                    {
                        try
                        {
                            var bytesRec = await networkClient
                                .ReceiveAsync(memory.Slice(1),
                                    _cancellationTokenSource.Token)
                                .ConfigureAwait(false);
                            if (bytesRec == 0)
                            {
                                continue;
                            }

                            await _webSocket
                                .SendAsync(
                                    memory, WebSocketMessageType.Binary, false,
                                    _cancellationTokenSource.Token)
                                .ConfigureAwait(false);
                        }
                        catch when (_cancellationTokenSource
                            .IsCancellationRequested)
                        {
                            return;
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