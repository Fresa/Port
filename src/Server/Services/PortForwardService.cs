using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Port.Server.Messages;

namespace Port.Server.Services
{
    internal sealed class PortForwardService : PortForwarder.PortForwarderBase,
        IAsyncDisposable
    {
        private readonly IKubernetesService _kubernetesService;
        private readonly CancellationTokenSource _cts = new();
        private CancellationToken CancellationToken => _cts.Token;
        private IAsyncDisposable _portForwardHandler = new AsyncDisposables();

        private readonly ConcurrentDictionary<string, IAsyncDisposable>
            _portForwardHandlers =
                new();

        public PortForwardService(
            IKubernetesService kubernetesService)
            => _kubernetesService = kubernetesService;

        public override async Task PortForward(
            Forward command,
            IServerStreamWriter<ForwardResponse> responseStream,
            ServerCallContext context)
        {
            var portForward = command.ToPortForward();

            _portForwardHandler =
                await _kubernetesService
                      .PortForwardAsync(
                          command.Context,
                          portForward,
                          CancellationToken)
                      .ConfigureAwait(false);

            _portForwardHandlers.TryAdd(command.GetId(), _portForwardHandler);

            await responseStream
                  .WriteAsync(ForwardResponse.WasForwarded())
                  .ConfigureAwait(false);
        }

        public override async Task<Stopped> StopForwarding(
            Stop command,
            ServerCallContext context)
        {
            if (_portForwardHandlers.TryRemove(
                command.GetId(), out var handler))
            {
                await handler.DisposeAsync()
                             .ConfigureAwait(false);
            }

            return new Stopped();
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();

            while (!_portForwardHandlers.IsEmpty)
            {
                await Task.WhenAll(
                    _portForwardHandlers
                        .Keys
                        .Select(
                            key => _portForwardHandlers
                                .TryRemove(key, out var handler)
                                ? handler.DisposeAsync()
                                : ValueTask.CompletedTask)
                        .Where(
                            valueTask => !valueTask
                                .IsCompletedSuccessfully)
                        .Select(valueTask => valueTask.AsTask()));
            }

            _cts.Dispose();
        }
    }
}
