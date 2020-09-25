using System;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy
{
    public sealed class SpdyEndPoint : IDisposable
    {
        private CancellationTokenSource _state;

        private readonly CancellationTokenSource _closed = new CancellationTokenSource(0);
        private readonly CancellationTokenSource _opened = new CancellationTokenSource();

        internal CancellationToken Cancellation
            => _opened.Token;

        private event Action<CancellationTokenSource> StateChanged = source => { };

        internal SpdyEndPoint()
        {
            _state = _closed;
        }

        internal void Open()
        {
            if (Interlocked.CompareExchange(ref _state, _opened, _closed) == _closed)
            {
                StateChanged.Invoke(_opened);
            }
        }

        public bool IsOpen => !IsClosed;
        public async Task WaitForOpenedAsync(CancellationToken cancellationToken = default)
        {
            using var signal = new SemaphoreSlim(0);
            StateChanged += OnStateChanged;
            try
            {
                if (IsOpen)
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
                CancellationTokenSource args)
            {
                if (args == _opened)
                {
                    signal.Release();
                }
            }
        }

        internal void Close()
        {
            if (Interlocked.CompareExchange(ref _state, _closed, _opened) == _opened)
            {
                _opened.Cancel(false);
                StateChanged.Invoke(_closed);
            }
        }

        public bool IsClosed => _state.IsCancellationRequested;
        public async Task WaitForClosedAsync(CancellationToken cancellationToken = default)
        {
            using var signal = new SemaphoreSlim(0);
            StateChanged += OnStateChanged;
            try
            {
                if (IsClosed)
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
                CancellationTokenSource args)
            {
                if (args == _closed)
                {
                    signal.Release();
                }
            }
        }

        public void Dispose()
        {
            _closed.Dispose();
            _opened.Dispose();
        }
    }
}