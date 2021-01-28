using System;
using System.Threading;
using System.Threading.Tasks;

namespace Spdy.Endpoint
{
    public sealed class SpdyEndpoint : IDisposable, IEndpoint
    {
        private readonly IEndpointStateIterator _stateIterator =
            EndpointStateBuilder.StartWith(EndpointState.Closed)
                                .Then(EndpointState.Closed)
                                .Or(
                                    EndpointState.Opened, builder =>
                                        builder.Then(EndpointState.Closed))
                                .Build();

        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();

        internal CancellationToken Cancellation { get; }

        private event Action<EndpointState> StateChanged = source => { };

        internal SpdyEndpoint()
        {
            Cancellation = _cancellationSource.Token;
        }

        internal bool Open()
        {
            var opened = _stateIterator.TransitionTo(EndpointState.Opened);
            if (opened)
            {
                StateChanged.Invoke(EndpointState.Opened);
            }

            return opened;
        }

        public bool IsOpen => _stateIterator.Current == EndpointState.Opened;
        public Task WaitForOpenedAsync(CancellationToken cancellationToken = default)
        {
            return WaitForStateAsync(EndpointState.Opened, cancellationToken);
        }

        internal bool Close()
        {
            var closed = _stateIterator.TransitionTo(EndpointState.Closed);
            if (closed)
            {
                _cancellationSource.Cancel(false);
                StateChanged.Invoke(EndpointState.Closed);
            }

            return closed;
        }

        public bool IsClosed => _stateIterator.Current == EndpointState.Closed;
        public Task WaitForClosedAsync(CancellationToken cancellationToken = default)
        {
            return WaitForStateAsync(EndpointState.Closed, cancellationToken);
        }

        private async Task WaitForStateAsync(EndpointState state, CancellationToken cancellationToken = default)
        {
            using var signal = new SemaphoreSlim(0);
            StateChanged += OnStateChanged;
            try
            {
                if (_stateIterator.Current == state)
                {
                    return;
                }
                await signal.WaitAsync(cancellationToken)
                            .ConfigureAwait(false);
            }
            finally
            {
                StateChanged -= OnStateChanged;
            }

            void OnStateChanged(
                EndpointState changedState)
            {
                if (changedState == state)
                {
                    signal.Release();
                }
            }
        }

        public void Dispose()
        {
            _cancellationSource.Dispose();
        }
    }
}