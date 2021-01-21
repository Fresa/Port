using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Log.It;
using Port.Server.Spdy;
using Port.Server.Spdy.Collections;
using Port.Server.Spdy.Frames;
using Port.Shared;
using ReadResult = System.IO.Pipelines.ReadResult;

namespace Port.Server
{
    internal sealed class SpdyStreamForwarder : IAsyncDisposable
    {
        private readonly INetworkServer _networkServer;
        private readonly SpdySession _spdySession;
        private readonly PortForward _portForward;
        private const int Stopped = 0;
        private const int Started = 1;
        private int _status = Stopped;

        private readonly CancellationTokenSource _cancellationTokenSource =
            new CancellationTokenSource();

        private CancellationToken CancellationToken
            => _cancellationTokenSource.Token;

        private readonly List<Task> _backgroundTasks = new List<Task>();

        private readonly ILogger _logger = LogFactory.Create<SpdyStreamForwarder>();

        private SpdyStreamForwarder(
            INetworkServer networkServer,
            SpdySession spdySession,
            PortForward portForward)
        {
            _networkServer = networkServer;
            _spdySession = spdySession;
            _portForward = portForward;
        }

        internal static IAsyncDisposable Start(
            INetworkServer networkServer,
            SpdySession spdySession,
            PortForward portForward)
        {
            return new SpdyStreamForwarder(networkServer, spdySession, portForward)
                .Start();
        }

        private IAsyncDisposable Start()
        {
            var previousStatus = Interlocked.Exchange(ref _status, Started);
            if (previousStatus == Started)
            {
                return this;
            }

            _backgroundTasks.Add(StartReceivingLocalClientsAsync());
            return this;
        }

        private async Task StartReceivingLocalClientsAsync()
        {
            while (CancellationToken.IsCancellationRequested ==
                   false)
            {
                try
                {
                    var client = await _networkServer
                                       .WaitForConnectedClientAsync(CancellationToken)
                                       .ConfigureAwait(false);

                    _logger.Trace("Local socket connected");
                    _backgroundTasks.Add(StartPortForwardingAsync(client));
                }
                catch when (CancellationToken
                    .IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unknown error while waiting for clients, closing down");
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

        private static int _requestId;
        private async Task StartPortForwardingAsync(INetworkClient client)
        {
            await using (client.ConfigureAwait(false))
            {
                using var cancellationTokenSource =
                    CancellationTokenSource.CreateLinkedTokenSource(
                        CancellationToken);
                var cancellationToken = cancellationTokenSource.Token;

                var requestId = Interlocked.Increment(ref _requestId).ToString();
                using var stream = _spdySession.Open(
                    headers: new NameValueHeaderBlock(
                        (Kubernetes.Headers.PortForward.StreamType.Key, new[]
                        {
                            Kubernetes.Headers.PortForward.StreamType.Data
                        }),
                        (Kubernetes.Headers.PortForward.Port, new[]
                        {
                           _portForward.PodPort.ToString()
                        }),
                        (Kubernetes.Headers.PortForward.RequestId, new[]
                        {
                            requestId
                        })));

                using var errorStream = _spdySession.Open(
                    options: SynStream.Options.Fin,
                    headers: new NameValueHeaderBlock(
                        (Kubernetes.Headers.PortForward.StreamType.Key, new[]
                        {
                            Kubernetes.Headers.PortForward.StreamType.Error
                        }),
                        (Kubernetes.Headers.PortForward.Port, new[]
                        {
                            _portForward.PodPort.ToString()
                        }),
                        (Kubernetes.Headers.PortForward.RequestId, new[]
                        {
                            requestId
                        })));

                var sendingTask = Task.CompletedTask;
                var receivingTask = Task.CompletedTask;
                var receivingErrorsTask = Task.CompletedTask;

                try
                {
                    sendingTask = StartSendingAsync(
                        client,
                        stream,
                        cancellationToken);
                    receivingTask = StartReceivingAsync(
                        client,
                        stream,
                        cancellationToken);
                    receivingErrorsTask = StartReceivingAsync(
                        new LogErrorStreamNetworkClient(),
                        errorStream,
                        cancellationToken);

                    await Task.WhenAny(
                            stream.Local.WaitForClosedAsync(cancellationToken),
                            stream.Remote.WaitForClosedAsync(cancellationToken))
                        .ConfigureAwait(false);

                    // Remote has closed. Let the receiving local socket
                    // receive any in-flight data before cancelling
                    if (stream.Local.IsOpen)
                    {
                        await receivingTask.ConfigureAwait(false);
                    }
                }
                catch when (cancellationToken
                    .IsCancellationRequested)
                {
                }
                catch (Exception ex)
                {
                    _logger.Fatal(
                        ex,
                        "[{SessionId},{StreamId}]: Unknown error while sending and receiving data, " +
                        "closing down",
                        _spdySession.Id,
                        stream.Id);
                }
                finally
                {
                    cancellationTokenSource.Cancel();

                    await Task.WhenAll(sendingTask, receivingTask, receivingErrorsTask)
                              .ConfigureAwait(false);

                    _logger.Info(
                        "[{SessionId},{StreamId}]: Port forward stream has completed, disconnecting local socket",
                        _spdySession.Id,
                        stream.Id);
                }
            }
        }

        private async Task StartSendingAsync(
            INetworkClient localSocket,
            SpdyStream spdyStream,
            CancellationToken cancellationToken)
        {
            try
            {
                using var memoryOwner = MemoryPool<byte>.Shared.Rent(65536);
                var memory = memoryOwner.Memory;
                FlushResult sendResult;
                do
                {
                    _logger.Trace(
                        "[{SessionId}:{StreamId}]: Waiting for data from local socket...",
                        spdyStream.SessionId, spdyStream.Id);
                    var bytesReceived = await localSocket
                                              .ReceiveAsync(
                                                  memory,
                                                  cancellationToken)
                                              .ConfigureAwait(false);
                    _logger.Trace(
                        "[{SessionId}:{StreamId}]: " +
                        $"Received {bytesReceived} bytes from local socket",
                        spdyStream.SessionId, spdyStream.Id);
                    // End of the stream! 
                    // https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.sockettaskextensions.receiveasync?view=netcore-3.1
                    if (bytesReceived == 0)
                    {
                        await spdyStream.SendLastAsync(
                                            ReadOnlyMemory<byte>.Empty,
                                            cancellationToken: cancellationToken)
                                        .ConfigureAwait(false);
                        _logger.Trace(
                            "[{SessionId}:{StreamId}]: Local -> remote data transferring has " +
                            "stopped (Local socket closed the connection)",
                            spdyStream.SessionId, spdyStream.Id);
                        return;
                    }

                    _logger.Trace(
                        "[{SessionId}:{StreamId}]: " +
                        $"Sending {bytesReceived} bytes to remote socket",
                        spdyStream.SessionId, spdyStream.Id);

                    sendResult = await spdyStream
                                       .SendAsync(
                                           memory.Slice(0, bytesReceived),
                                           cancellationToken: cancellationToken)
                                       .ConfigureAwait(false);
                    _logger.Trace(
                        "[{SessionId}:{StreamId}]: Sending to remote socket complete",
                        spdyStream.SessionId, spdyStream.Id);

                } while (sendResult.HasMore());

                _logger.Trace(
                    "[{SessionId}:{StreamId}]: " +
                    $"Local -> remote data transferring has stopped ({sendResult.GetStatusAsString()})",
                    spdyStream.SessionId, spdyStream.Id);
            }
            catch when (cancellationToken
                .IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.Fatal(
                    ex,
                    "[{SessionId}:{StreamId}]: Unknown error while sending and receiving data, closing down",
                    spdyStream.SessionId, spdyStream.Id);
                _cancellationTokenSource.Cancel(false);
            }
        }

        private async Task StartReceivingAsync(
            INetworkClient localSocket,
            SpdyStream spdyStream,
            CancellationToken cancellationToken)
        {
            try
            {
                ReadResult content;
                do
                {
                    _logger.Trace(
                        "[{SessionId}:{StreamId}]: Waiting for data from remote socket...",
                        spdyStream.SessionId, spdyStream.Id);
                    content = await spdyStream
                                    .ReceiveAsync(
                                        cancellationToken: cancellationToken)
                                    .ConfigureAwait(false);

                    _logger.Trace(
                        "[{SessionId}:{StreamId}]: " +
                        $"Received {content.Buffer.Length} bytes from remote socket",
                        spdyStream.SessionId, spdyStream.Id);

                    if (content.Buffer.IsEmpty)
                    {
                        continue;
                    }

                    foreach (var sequence in content.Buffer)
                    {
                        _logger.Trace(
                            "[{SessionId}:{StreamId}]: " +
                            $"Sending {sequence.Length} bytes to local socket",
                            spdyStream.SessionId, spdyStream.Id);
                        _logger.Trace(
                            Encoding.ASCII.GetString(sequence.ToArray()));
                        await localSocket
                              .SendAsync(
                                  sequence, cancellationToken)
                              .ConfigureAwait(false);
                        _logger.Trace(
                            "[{SessionId}:{StreamId}]: Sending to local socket complete",
                            spdyStream.SessionId, spdyStream.Id);
                    }
                } while (content.HasMoreData());

                _logger.Trace(
                    "[{SessionId}:{StreamId}]: " +
                    $"Remote -> local data transferring has stopped ({content.GetStatusAsString()})",
                    spdyStream.SessionId, spdyStream.Id);
            }
            catch when (cancellationToken
                .IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.Fatal(
                    ex,
                    "[{SessionId}:{StreamId}]: Unknown error while sending and receiving data, closing down",
                    spdyStream.SessionId, spdyStream.Id);
                _cancellationTokenSource.Cancel(false);
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cancellationTokenSource.Cancel(false);

            try
            {
                await Task.WhenAll(_backgroundTasks)
                          .ConfigureAwait(false);

            }
            catch (OperationCanceledException)
            {
            }

            _cancellationTokenSource.Dispose();
        }
    }
}