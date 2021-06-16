using System;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Client.Threading
{
    internal sealed class SemaphoreSlimGate : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        internal static SemaphoreSlimGate OneAtATime =>
            new(new SemaphoreSlim(1, 1));

        private SemaphoreSlimGate(
            SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        internal async Task<IDisposable> WaitAsync(
            CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken)
                            .ConfigureAwait(false);
            return new DisposableAction(
                () =>
                    _semaphore.Release());
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }
    }
}