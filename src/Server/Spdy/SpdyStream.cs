using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Frames;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    public sealed class SpdyStream
    {
        private readonly ConcurrentPriorityQueue<Frame> _sendingPriorityQueue;

        private readonly ConcurrentQueue<Frame> _receivingPriorityQueue
            = new ConcurrentQueue<Frame>();
        private readonly SemaphoreSlim _itemsAvailable = new SemaphoreSlim(0);

        internal SpdyStream(
            UInt31 id,
            SynStream.PriorityLevel priority,
            ConcurrentPriorityQueue<Frame> sendingPriorityQueue)
        {
            Id = id;
            _sendingPriorityQueue = sendingPriorityQueue;
            Priority = priority;
        }
        public UInt31 Id { get; }
        public bool IsClosed { get; private set; } = true;

        internal void Receive(
            Frame frame)
        {
            _receivingPriorityQueue.Enqueue(frame);
            _itemsAvailable.Release();
        }

        public SynStream.PriorityLevel Priority { get; }

        public void Open()
        {
            var open = new SynStream(
                SynStream.Options.None, Id, UInt31.From(0), Priority, new Dictionary<string, string>());
            _sendingPriorityQueue.Enqueue(Priority, open);
            IsClosed = false;
        }

        public void Send(
            Frame frame)
        {
            _sendingPriorityQueue.Enqueue(Priority, frame);
        }

        public async Task<Frame> ReadAsync(
            CancellationToken cancellationToken = default)
        {
            await _itemsAvailable.WaitAsync(cancellationToken)
                           .ConfigureAwait(false);
            if (_receivingPriorityQueue.TryDequeue(out var frame))
            {
                return frame;
            }
            
            throw new InvalidOperationException("Receiving queue got out of sync");
        }
    }
}