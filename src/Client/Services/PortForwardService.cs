using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace Port.Client.Services
{
    internal sealed class PortForwardService : IAsyncDisposable
    {
        private readonly AsyncDuplexStreamingCall<ForwardRequest, ForwardResponse> _stream;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _listeningTask;
        private CancellationToken CancellationToken => _cts.Token;

        public PortForwardService(
            AsyncDuplexStreamingCall<ForwardRequest, ForwardResponse> stream,
            global::Port.Shared.PortForward model)
        {
            _stream = stream;
            Model = model ?? throw new ArgumentNullException(nameof(model));
            _listeningTask = StartListenOnEventsAsync();
        }

        internal global::Port.Shared.PortForward Model { get; }
        internal event Action OnStateChanged = () => { };

        internal Task ForwardAsync()
        {
            return _stream.RequestStream.WriteAsync(
                new ForwardRequest
                {
                    Forward = new Forward
                    {
                        Namespace = Model.Namespace,
                        Pod = Model.Pod,
                        PodPort = (uint)Model.PodPort,
                        LocalPort = (uint)(Model.LocalPort ?? throw new ArgumentNullException(nameof(Model.LocalPort))),
                        ProtocolType = Model.ProtocolType switch
                        {
                            ProtocolType.Tcp => Forward.Types.ProtocolType.Tcp,
                            ProtocolType.Udp => Forward.Types.ProtocolType.Udp,
                            _ => throw new ArgumentOutOfRangeException($"{Model.ProtocolType} not supported")
                        }
                    }
                });
        }

        internal Task StopAsync()
        {
            return _stream.RequestStream.WriteAsync(
                new ForwardRequest
                {
                    Stop = new Stop()
                });
        }

        private async Task StartListenOnEventsAsync()
        {
            try
            {
                await foreach (var message in _stream.ResponseStream.ReadAllAsync(cancellationToken: CancellationToken))
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
                    OnStateChanged();
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
            }
            catch (Exception) when (CancellationToken.IsCancellationRequested)
            {
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            await Task.WhenAll(
                _stream.RequestStream.CompleteAsync(),
                _listeningTask);
            _stream.Dispose();
            _cts.Dispose();
        }
    }
}