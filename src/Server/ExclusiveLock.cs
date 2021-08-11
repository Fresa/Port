using System;
using System.Threading;

namespace Port.Server
{
    internal sealed class ExclusiveLock
    {
        private int _acquired;
        internal IDisposable TryAcquire(out bool acquired)
        {
            if (Interlocked.CompareExchange(ref _acquired, 1, 0) == 1)
            {
                acquired = false;
                return new DisposableActions(() => {});
            }

            acquired = true;
            return new DisposableActions(
                () =>
                {
                    Interlocked.Exchange(ref _acquired, 0);
                });
        }

        internal bool TryAcquire()
        {
            TryAcquire(out var acquired);
            return acquired;
        }
    }
}