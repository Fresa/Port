using System;
using System.Threading;

namespace Spdy.Helpers
{
    internal sealed class ExclusiveLock
    {
        private int _acquired;
        internal IDisposable TryAcquire(out bool acquired)
        {
            if (Interlocked.CompareExchange(ref _acquired, 1, 0) == 1)
            {
                acquired = false;
                return new DisposableAction(() => {});
            }

            acquired = true;
            return new DisposableAction(
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