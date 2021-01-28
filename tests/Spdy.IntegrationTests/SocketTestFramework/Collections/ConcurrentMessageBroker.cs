using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Spdy.IntegrationTests.SocketTestFramework.Collections
{
    internal sealed class ConcurrentMessageBroker<T> : ISubscription<T>
    {
        private readonly ConcurrentQueue<Task<T>> _queue = new ConcurrentQueue<Task<T>>();
        private readonly SemaphoreSlim _signaler = new SemaphoreSlim(0);
        internal void Send(
            T message)
        {
            _queue.Enqueue(Task.FromResult(message));
            _signaler.Release();
        }

        public async Task<T> ReceiveAsync(
            CancellationToken cancellation = default)
        {
            await _signaler.WaitAsync(cancellation)
                           .ConfigureAwait(false);
            if (_queue.TryDequeue(out var item))
            {
                return await item
                    .ConfigureAwait(false);
            }

            throw new InvalidOperationException("Signaler out of sync");
        }

        internal void Complete(
            Exception? exception = default)
        {
            if (exception != default)
            {
                _queue.Enqueue(Task.FromException<T>(exception));
                _signaler.Release();
            }

            _queue.Enqueue(Task.FromException<T>(new InvalidOperationException("The sender has completed")));
            _signaler.Release();
        }
    }
}