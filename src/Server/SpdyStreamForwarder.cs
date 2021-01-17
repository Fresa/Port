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
using Port.Server.Spdy.Extensions;
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
                    _logger.Fatal(ex, "Unknown error while waiting for clients, closing down");
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
            using var _ = _logger.LogicalThread.Capture(
                "local-socket-id", Guid.NewGuid());
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

                    await Task.WhenAll(
                            errorStream.Remote.WaitForClosedAsync(cancellationToken),
                            stream.Local.WaitForClosedAsync(cancellationToken)
                                  .ContinueWith(task => cancellationTokenSource.CancelAfter(1000), cancellationToken),
                            stream.Remote.WaitForClosedAsync(cancellationToken)
                                  .ContinueWith(task => cancellationTokenSource.CancelAfter(1000), cancellationToken))
                        .ConfigureAwait(false);
                }
                catch when (cancellationToken
                    .IsCancellationRequested)
                {
                }
                catch (Exception ex)
                {
                    _logger.Fatal(
                        ex,
                        "Unknown error while sending and receiving data, closing down");
                }
                finally
                {
                    // todo:
                    // Hack!
                    // There is a race condition when the spdy stream is 
                    // reported as fully closed but the last data might not
                    // have been fully sent to the client. By stalling for 
                    // a while we let the remote receiving background worker
                    // do it's job for a little while longer.
                    // Ideally we should let this process work until it's done
                    // sending all data, but we cannot let the process run
                    // potentially forever, the application might be in a stopping
                    // state, and at some point we have to let go.

                    //This will most likely change when we need to report
                    //back that the forwarding terminated or that we
                    //should retry
                    cancellationTokenSource.CancelAfter(1000);

                    // Wait for the tasks to complete otherwise
                    // we risk to dispose the stream and the local
                    // socket client while being in used
                    await Task.WhenAll(sendingTask, receivingTask, receivingErrorsTask)
                              .ConfigureAwait(false);
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
                using var _ = _logger.LogicalThread.Capture(
                    "stream-id", spdyStream.Id);

                using var memoryOwner = MemoryPool<byte>.Shared.Rent(65536);
                var memory = memoryOwner.Memory;
                FlushResult sendResult;
                do
                {
                    _logger.Trace("Receiving from local socket");
                    var bytesReceived = await localSocket
                                              .ReceiveAsync(
                                                  memory,
                                                  cancellationToken)
                                              .ConfigureAwait(false);
                    _logger.Trace("Received {bytes} bytes from local socket",
                        bytesReceived);
                    // End of the stream! 
                    // https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.sockettaskextensions.receiveasync?view=netcore-3.1
                    if (bytesReceived == 0)
                    {
                        await spdyStream.SendLastAsync(
                                            new ReadOnlyMemory<byte>(),
                                            cancellationToken: cancellationToken)
                                        .ConfigureAwait(false);
                        return;
                    }

                    _logger.Trace(
                        "Sending {bytes} bytes to remote socket",
                        bytesReceived);

                    sendResult = await spdyStream
                                       .SendAsync(
                                           memory.Slice(0, bytesReceived),
                                           cancellationToken: cancellationToken)
                                       .ConfigureAwait(false);
                    _logger.Trace("Sending to remote socket complete");

                } while (sendResult.HasMore());
            }
            catch when (cancellationToken
                .IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Unknown error while sending and receiving data, closing down");
#pragma warning disable 4014
                //Cancel and exit fast
                //This will most likely change when we need to report
                //back that the forwarding terminated or that we
                //should retry
                _cancellationTokenSource.Cancel(false);
#pragma warning restore 4014
            }
        }

        private async Task StartReceivingAsync(
            INetworkClient localSocket,
            SpdyStream spdyStream,
            CancellationToken cancellationToken)
        {
            try
            {
                using var _ = _logger.LogicalThread.Capture(
                    "stream-id", spdyStream.Id);

                ReadResult content;
                do
                {
                    _logger.Trace("Waiting for data from remote socket...");
                    content = await spdyStream
                                    .ReceiveAsync(
                                        cancellationToken: cancellationToken)
                                    .ConfigureAwait(false);

                    _logger.Trace("Received {bytes} bytes from remote socket",
                        content.Buffer.Length);

                    if (content.Buffer.IsEmpty)
                    {
                        continue;
                    }

                    foreach (var sequence in content.Buffer)
                    {
                        _logger.Trace("Sending {bytes} bytes to local socket",
                            sequence.Length);
                        _logger.Trace(
                            Encoding.ASCII.GetString(sequence.ToArray()));
                        await localSocket
                              .SendAsync(
                                  sequence,
                                  cancellationToken)
                              .ConfigureAwait(false);
                        _logger.Trace("Sending to local socket complete");
                    }
                } while (content.HasMoreData());
            }
            catch when (cancellationToken
                .IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Unknown error while sending and receiving data, closing down");
#pragma warning disable 4014
                //Cancel and exit fast
                //This will most likely change when we need to report
                //back that the forwarding terminated or that we
                //should retry
                _cancellationTokenSource.Cancel(false);
#pragma warning restore 4014
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