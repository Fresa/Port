using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Port.Server.Messages;
using Port.Server.Threading;

namespace Port.Server.Services
{
    internal sealed class PortForwardService : PortForwarder.PortForwarderBase,
        IDisposable
    {
        private readonly IKubernetesService _kubernetesService;
        private readonly CancellationTokenSource _cts = new();
        private CancellationToken CancellationToken => _cts.Token;

        private readonly TransitioningLock _portForwardHandlerLock = new();
        private IAsyncDisposable _portForwardHandler = new AsyncDisposables();

        public PortForwardService(
            IKubernetesService kubernetesService)
            => _kubernetesService = kubernetesService;


        public override async Task PortForward(
            IAsyncStreamReader<ForwardRequest> requestStream,
            IServerStreamWriter<ForwardResponse> responseStream,
            ServerCallContext context)
        {
            await foreach (var message in requestStream
                                          .ReadAllAsync(CancellationToken)
                                          .ConfigureAwait(false))
            {
                switch (message.CommandCase)
                {
                    case ForwardRequest.CommandOneofCase.Forward:
                        if (!_portForwardHandlerLock.TryStartLocking(
                            out var lockState))
                        {
                            return;
                        }

                        try
                        {
                            var portForward = message.Forward.ToPortForward();

                            _portForwardHandler =
                                await _kubernetesService
                                      .PortForwardAsync(
                                          message.Context,
                                          portForward,
                                          CancellationToken)
                                      .ConfigureAwait(false);
                        }
                        catch
                        {
                            lockState.Release();
                            throw;
                        }

                        lockState.Lock();

                        await responseStream
                              .WriteAsync(ForwardResponse.CreateForwarded())
                              .ConfigureAwait(false);
                        break;
                    case ForwardRequest.CommandOneofCase.Stop:
                        if (!_portForwardHandlerLock.TryStartReleasing(
                            out var releaseState))
                        {
                            return;
                        }

                        try
                        {
                            await _portForwardHandler.DisposeAsync()
                                .ConfigureAwait(false);
                        }
                        catch
                        {
                            releaseState.Lock();
                            throw;
                        }

                        releaseState.Release();

                        await responseStream
                              .WriteAsync(ForwardResponse.CreateStopped())
                              .ConfigureAwait(false);
                        break;
                    case ForwardRequest.CommandOneofCase.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
