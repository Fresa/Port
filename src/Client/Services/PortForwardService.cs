using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace Port.Client.Services
{
    internal sealed class PortForwardService : IAsyncDisposable
    {
        private readonly PortForwarder.PortForwarderClient _portForwarder;
        private readonly string _context;
        private readonly CancellationTokenSource _cts = new();
        private CancellationToken CancellationToken => _cts.Token;
        private readonly ConcurrentBag<Task> _tasks = new();

        public PortForwardService(
            PortForwarder.PortForwarderClient portForwarder,
            string context,
            Port.Shared.PortForward model)
        {
            _portForwarder = portForwarder;
            _context = context;
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        internal Port.Shared.PortForward Model { get; }
        internal event Func<Task> OnStateChangedAsync = () => Task.CompletedTask;
        internal event Func<Exception, Task> OnErrorAsync = _ => Task.CompletedTask;

        internal Task ForwardAsync()
        {
            var forwardRequest = new Forward
            {
                Context = _context,
                Namespace = Model.Namespace,
                Pod = Model.Pod,
                PodPort = (uint)Model.PodPort,
                LocalPort = (uint)(Model.LocalPort ??
                                    throw new ArgumentNullException(
                                        nameof(Model.LocalPort))),
                ProtocolType = Model.ProtocolType switch
                {
                    System.Net.Sockets.ProtocolType.Tcp => ProtocolType.Tcp,
                    System.Net.Sockets.ProtocolType.Udp => ProtocolType.Udp,
                    _ => throw new ArgumentOutOfRangeException(
                        $"{Model.ProtocolType} not supported")
                }
            };

            var stream = _portForwarder.PortForward(forwardRequest);
            _tasks.Add(StartListenOnForwardResponseAsync(stream));
            return Task.CompletedTask;
        }

        internal async Task StopAsync()
        {
            var stopRequest = new Stop
            {
                Context = _context,
                Namespace = Model.Namespace,
                Pod = Model.Pod,
                PodPort = (uint)Model.PodPort,
                LocalPort = (uint)(Model.LocalPort ??
                                    throw new ArgumentNullException(
                                        nameof(Model.LocalPort))),
                ProtocolType = Model.ProtocolType switch
                {
                    System.Net.Sockets.ProtocolType.Tcp => ProtocolType.Tcp,
                    System.Net.Sockets.ProtocolType.Udp => ProtocolType.Udp,
                    _ => throw new ArgumentOutOfRangeException(
                        $"{Model.ProtocolType} not supported")
                }
            };

            using var stream = _portForwarder.StopForwardingAsync(stopRequest);
            try
            {
                await stream.ResponseAsync.ConfigureAwait(false);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
            }
        }

        private async Task StartListenOnForwardResponseAsync(
            AsyncServerStreamingCall<ForwardResponse> stream)
        {
            try
            {
                await foreach (var message in stream.ResponseStream
                                                    .ReadAllAsync(CancellationToken)
                                                    .ConfigureAwait(false))
                {
                    switch (message.EventCase)
                    {
                        case ForwardResponse.EventOneofCase.Forwarded:
                            if (Model.Forwarding)
                            {
                                continue;
                            }

                            Model.Forwarding = true;
                            break;
                        case ForwardResponse.EventOneofCase.Stopped:
                            if (!Model.Forwarding)
                            {
                                continue;
                            }

                            Model.Forwarding = false;
                            break;
                        case ForwardResponse.EventOneofCase.None:
                            continue;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    await OnStateChangedAsync()
                        .ConfigureAwait(false);
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
            }
            catch (Exception) when (CancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                Model.Forwarding = false;
                await OnErrorAsync(ex)
                    .ConfigureAwait(false);
            }
            finally
            {
                stream.Dispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();

            await Task.WhenAll(_tasks)
                      .ConfigureAwait(false);

            _cts.Dispose();
        }
    }
}