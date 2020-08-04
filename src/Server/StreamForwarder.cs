using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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


        private readonly BufferBlock<INetworkClient> _activeLocalSockets =
            new BufferBlock<INetworkClient>();

        private readonly ILogger _logger = LogFactory.Create<StreamForwarder>();

        private StreamForwarder(
            INetworkServer networkServer,
            WebSocket remoteSocket)
        {
            _networkServer = networkServer;
            _remoteSocket = new WebSocketStreamer(
                remoteSocket, _cancellationTokenSource);
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

            _backgroundTasks.Add(StartReceivingLocalClients());
            for (var i = 0; i < Environment.ProcessorCount; i++)
            {
                _backgroundTasks.Add(StartPortForwarding());
            }
            return this;
        }

        private async Task StartReceivingLocalClients()
        {
            while (_cancellationTokenSource.IsCancellationRequested ==
                   false)
            {
                try
                {
                    var client = await _networkServer
                        .WaitForConnectedClientAsync(CancellationToken)
                        .ConfigureAwait(false);

                    _logger.Trace("Local socket connected");
                    await _activeLocalSockets.SendAsync(
                            client, CancellationToken)
                        .ConfigureAwait(false);
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
                    //Cancel and exit fast
                    //This will most likely change when we need to report
                    //back that the forwarding terminated or that we
                    //should retry
                    _cancellationTokenSource.Cancel(false);
#pragma warning restore 4014
                    return;
                }
            }
        }

        private async Task StartPortForwarding()
        {
            while (_cancellationTokenSource.IsCancellationRequested ==
                   false)
            {
                var client = await _activeLocalSockets
                    .ReceiveAsync(CancellationToken)
                    .ConfigureAwait(false);
                using var _ = _logger.LogicalThread.With(
                    "local-socket-id", Guid.NewGuid());
                try
                {
                    var forward = CancellationTokenSource
                        .CreateLinkedTokenSource(CancellationToken);
                    try
                    {
                        await StartTransferDataToRemote(client, forward.Token)
                            .ConfigureAwait(false);

                        // Client still alive, add it back at the end of the queue
                        await _activeLocalSockets.SendAsync(
                                client, CancellationToken)
                            .ConfigureAwait(false);
                    }
                    catch (SocketException socketException)
                    {
                        _logger.Debug(
                            socketException,
                            "Local socket caught an exception");
                        try
                        {
                            await client.DisposeAsync()
                                .ConfigureAwait(false);
                        }
                        catch
                        {
                            // Try to dispose gracefully
                        }

                        forward.Cancel(false);
                    }
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
                    //Cancel and exit fast
                    //This will most likely change when we need to report
                    //back that the forwarding terminated or that we
                    //should retry
                    _cancellationTokenSource.Cancel(false);
#pragma warning restore 4014
                    return;
                }
            }
        }

        private async Task StartTransferDataToRemote(
            INetworkClient localSocket,
            CancellationToken aToken)
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(65536);
            var memory = memoryOwner.Memory;
            _logger.Trace("Receiving from local socket");
            var bytesReceived = await localSocket
                .ReceiveAsync(
                    memory,
                    aToken)
                .ConfigureAwait(false);

            // End of the stream! 
            // https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.sockettaskextensions.receiveasync?view=netcore-3.1
            if (bytesReceived == 0)
            {
                throw new SocketException((int) SocketError.Success);
            }

            _logger.Trace(
                "Sending {bytes} bytes to remote socket",
                bytesReceived + 1);

            using (await _webSocketGate
                .WaitAsync(CancellationToken)
                .ConfigureAwait(false))
            {
                var sendResult = await _remoteSocket
                    .SendAsync(memory.Slice(0, bytesReceived))
                    .ConfigureAwait(false);

                if (sendResult.IsCanceled ||
                    sendResult.IsCompleted)
                {
                    _cancellationTokenSource.Cancel();
                }

                await StartTransferDataToLocal(localSocket, aToken)
                    .ConfigureAwait(false);
            }
        }

        private async Task StartTransferDataToLocal(
            INetworkClient localSocket,
            CancellationToken aToken)
        {
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
                    _logger.Trace("No response from kubernetes");
                    break;
                }

                foreach (var sequence in content.Buffer)
                {
                    if (httpResponseContentLength == 0)
                    {
                        sequence
                            .TryGetHttpResponseLength(
                                out httpResponseHeaderLength,
                                out httpResponseContentLength);
                    }

                    await localSocket
                        .SendAsync(
                            sequence,
                            aToken)
                        .ConfigureAwait(false);

                    totalReceivedBytes += sequence.Length;
                }

                if (content.IsCompleted)
                {
                    _cancellationTokenSource.Cancel();
                    return;
                }

                if (totalReceivedBytes == httpResponseHeaderLength +
                    httpResponseContentLength)
                {
                    _logger.Trace(
                        "Received {total} bytes in total, exiting",
                        totalReceivedBytes);
                    break;
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cancellationTokenSource.Cancel(false);
            _cancellationTokenSource.Dispose();

            try
            {
                await _networkServer.DisposeAsync()
                    .ConfigureAwait(false);
            }
            catch
            {
                // Ignore unhandled exceptions during shutdown 
            }

            try
            {
                await _remoteSocket.DisposeAsync()
                    .ConfigureAwait(false);
            }
            catch
            {
                // Ignore unhandled exceptions during shutdown 
            }

            _webSocketGate.Dispose();
            await Task.WhenAll(_backgroundTasks)
                .ConfigureAwait(false);
        }
    }
}