using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using k8s;

namespace Kubernetes.PortForward.Manager.Server
{
    internal sealed class StreamForwarder : IAsyncDisposable
    {
        private readonly StreamDemuxer _stream;
        private readonly INetworkServer _networkServer;
        private readonly WebSocket _webSocket;
        private const int Stopped = 0;
        private const int Started = 1;
        private int _status = Stopped;

        private readonly CancellationTokenSource _cancellationTokenSource =
            new CancellationTokenSource();

        private readonly List<Task> _backgroundTasks = new List<Task>();

        public StreamForwarder(
            StreamDemuxer stream,
            INetworkServer networkServer,
            WebSocket webSocket)
        {
            _stream = stream;
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
                    var start = true;
                    while (_cancellationTokenSource.IsCancellationRequested ==
                           false)
                    {
                        try
                        {
                            var client = await _networkServer
                                .WaitForConnectedClientAsync(
                                    _cancellationTokenSource.Token)
                                .ConfigureAwait(false);
                            //if (start)
                            StartCrossWiring(client);
                            start = false;
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
            //var stream = _stream.GetStream(
            //    ChannelIndex.StdIn, ChannelIndex.StdIn);
            var receiveTask = Task.Run(
                async () =>
                {
                    while (_cancellationTokenSource.IsCancellationRequested ==
                           false)
                    {
                        try
                        {
                            var buff = new byte[4096];
                            var memory = new Memory<byte>(buff);
                            var read = await _webSocket.ReceiveAsync(
                                    memory, _cancellationTokenSource.Token)
                                .ConfigureAwait(false);

                            // The port forward stream looks like this when receiving:
                            // [Stream index][High port byte][Low port byte][Data 1]..[Data n]
                            if (read.Count <= 3)
                            {
                                continue;
                            }

                            await networkClient.SendAsync(
                                    memory.Slice(1),
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

            _backgroundTasks.Add(receiveTask);

            var sendTask = Task.Run(
                async () =>
                {
                    var memory = new Memory<byte>(new byte[4096]);
                    // The port forward stream looks like this when sending:
                    // [Stream index][Data 1]..[Data n]
                    memory.Span[0] = (byte) ChannelIndex.StdOut;
                    while (_cancellationTokenSource.IsCancellationRequested ==
                           false)
                    {
                        try
                        {
                            var bytesRec = await networkClient
                                .ReceiveAsync(memory.Slice(1))
                                .ConfigureAwait(false);
                            if (bytesRec == 0)
                            {
                                continue;
                            }

                            await _webSocket.SendAsync(
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