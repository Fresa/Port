using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
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
                using var _ = _logger.LogicalThread.With(
                    "local-socket-id", Guid.NewGuid());
                try
                {
                    await using var client = await _networkServer
                        .WaitForConnectedClientAsync(CancellationToken)
                        .ConfigureAwait(false);
                    
                    _logger.Trace("Local socket connected");

                    await StartTransferDataFromLocalToRemoteSocket(client)
                        .ConfigureAwait(false);
                }
                catch when (_cancellationTokenSource
                    .IsCancellationRequested)
                {
                    return;
                }
                catch (SocketException socketException)
                {
                    _logger.Debug(
                        socketException,
                        "Local socket caught an exception");
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
                finally
                {
                    _logger.Trace("Local socket disconnected");
                }
            }
        }
        

        private async Task StartTransferDataFromLocalToRemoteSocket(
            INetworkClient localSocket)
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(65536);
            var memory = memoryOwner.Memory;
            try
            {
                _logger.Trace("Receiving from local socket");
                var bytesReceived = await localSocket
                    .ReceiveAsync(
                        memory,
                        CancellationToken)
                    .ConfigureAwait(false);

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
                                    CancellationToken)
                                .ConfigureAwait(false);

                            totalReceivedBytes += sequence.Length;
                        }

                        if (totalReceivedBytes == httpResponseHeaderLength +
                            httpResponseContentLength)
                        {
                            _logger.Trace(
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
}