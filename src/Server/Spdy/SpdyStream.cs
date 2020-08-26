using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Extensions;
using Port.Server.Spdy.Frames;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    public sealed class SpdyStream
    {
        private readonly ConcurrentPriorityQueue<byte[]> _sendingPriorityQueue;

        private readonly ConcurrentPriorityQueue<Frame> _receivingPriorityQueue
            = new ConcurrentPriorityQueue<Frame>();

        internal SpdyStream(
            UInt31 id,
            SynStream.PriorityLevel priority,
            ConcurrentPriorityQueue<byte[]> sendingPriorityQueue)
        {
            Id = id;
            _sendingPriorityQueue = sendingPriorityQueue;
            Priority = priority;
        }
        public UInt31 Id { get; }

        internal void Receive(
            Frame frame)
        {
            _receivingPriorityQueue.Enqueue(Priority, frame);
        }

        public SynStream.PriorityLevel Priority { get; }

        public async Task Open(
            CancellationToken cancellationToken = default)
        {
            var open = new SynStream(
                SynStream.Options.None, Id, UInt31.From(0), Priority, new Dictionary<string, string>());
            await open.WriteToAsync(
                          _sendingPriorityQueue, Priority, cancellationToken)
                      .ConfigureAwait(false);
        }

        public void Enqueue(
            byte[] data)
        {
            _sendingPriorityQueue.Enqueue(Priority, data);
        }

        public async Task<Frame> Dequeue(
            CancellationToken cancellationToken = default)
            => await _receivingPriorityQueue.DequeueAsync(cancellationToken)
                                            .ConfigureAwait(false);
    }
}