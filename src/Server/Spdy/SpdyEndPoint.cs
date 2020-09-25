using System;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy
{
    public sealed class SpdyEndPoint : IDisposable
    {
        private int _state = Closed;
        private const int Closed = 0;
        private const int Opened = 1;

        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();

        internal CancellationToken Cancellation
            => _cancellationSource.Token;

        private event Action<int> StateChanged = source => { };

        internal SpdyEndPoint()
        {
        }

        internal void Open()
        {
            if (Interlocked.CompareExchange(ref _state, Opened, Closed) == Closed)
            {
                StateChanged.Invoke(Opened);
            }
        }

        public bool IsOpen => !IsClosed;
        public Task WaitForOpenedAsync(CancellationToken cancellationToken = default)
        {
            return WaitForStateAsync(Opened, cancellationToken);
        }

        internal void Close()
        {
            if (Interlocked.CompareExchange(ref _state, Closed, Opened) == Opened)
            {
                _cancellationSource.Cancel(false);
                StateChanged.Invoke(Closed);
            }
        }

        public bool IsClosed => _state == Closed;
        public Task WaitForClosedAsync(CancellationToken cancellationToken = default)
        {
            return WaitForStateAsync(Closed, cancellationToken);
        }

        public async Task WaitForStateAsync(int state, CancellationToken cancellationToken = default)
        {
            using var signal = new SemaphoreSlim(0);
            StateChanged += OnStateChanged;
            try
            {
                if (_state == state)
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
                int changedState)
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