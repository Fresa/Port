using System;
using System.Threading;
using System.Threading.Tasks;

namespace Spdy.Helpers
{
    internal sealed class Signaler : IDisposable
    {
        private event Action Notified = () => { };

        internal void Signal()
        {
            Notified.Invoke();
        }

        internal IWaiter Subscribe()
        {
            var waiter = new ConcurrentWaiter();
            waiter.OnDispose += () => Notified -= OnNotified;
            Notified += OnNotified;
            return waiter;

            void OnNotified()
            {
                waiter.Release();
            }
        }

        private class ConcurrentWaiter : IWaiter
        {
            private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
            private bool _isDisposed;

            public Task WaitAsync(
                CancellationToken cancellation = default)
                => _semaphore.WaitAsync(cancellation);

            internal void Release()
            {
                lock (_semaphore)
                {
                    if (_isDisposed)
                    {
                        return;
                    }
                    
                    _semaphore.Release();
                }
            }

            internal event Action OnDispose = () => { };

            public void Dispose()
            {
                lock (_semaphore)
                {
                    OnDispose();
                    foreach (var @delegate in OnDispose.GetInvocationList())
                    {
                        OnDispose -= @delegate as Action;
                    }

                    _semaphore.Dispose();
                    _isDisposed = true;
                }
            }
        }

        public void Dispose()
        {
            foreach (var @delegate in Notified.GetInvocationList())
            {
                Notified -= @delegate as Action;
            }
        }
    }
}