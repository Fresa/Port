using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Spdy.Frames;

namespace Spdy.Collections
{
    internal sealed class ConcurrentPriorityQueue<T>
    {
        private readonly Dictionary<SynStream.PriorityLevel,
            ConcurrentQueue<T>> _priorityQueues =
            new Dictionary<SynStream.PriorityLevel,
                ConcurrentQueue<T>>(
                Enum.GetValues(typeof(SynStream.PriorityLevel))
                    .Cast<SynStream.PriorityLevel>()
                    .OrderBy(priority => priority)
                    .Select(
                        priority
                            => new KeyValuePair<SynStream.PriorityLevel,
                                ConcurrentQueue<T>>(
                                priority, new ConcurrentQueue<T>())));

        private readonly SemaphoreSlim _itemsAvailable = new SemaphoreSlim(0);

        public void Enqueue(
            SynStream.PriorityLevel priority,
            T item)
        {
            _priorityQueues[priority]
                .Enqueue(item);
            _itemsAvailable.Release();
        }

        public async Task<T> DequeueAsync(
            CancellationToken cancellationToken = default)
        {
            await _itemsAvailable
                  .WaitAsync(cancellationToken)
                  .ConfigureAwait(false);

            foreach (var queue in _priorityQueues.Values)
            {
                if (queue.TryDequeue(out var frame))
                {
                    return frame;
                }
            }

            throw new InvalidOperationException("Gate is out of sync");
        }
    }
}