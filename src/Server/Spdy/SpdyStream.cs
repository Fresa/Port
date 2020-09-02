using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Frames;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    public sealed class SpdyStream
    {
        private readonly ConcurrentPriorityQueue<Frame> _sendingPriorityQueue;

        private readonly ConcurrentQueue<Data> _receivingQueue
            = new ConcurrentQueue<Data>();
        private readonly SemaphoreSlim _frameAvailable = new SemaphoreSlim(0);

        private readonly RstStream _invalidStream;
        private readonly RstStream _streamInUse;
        private readonly RstStream _protocolError;

        private readonly ConcurrentDictionary<Type, Control> _controlFramesReceived = new ConcurrentDictionary<Type, Control>();

        private int _remote;
        private int _local;
        private const int Closed = 1;

        internal SpdyStream(
            UInt31 id,
            SynStream.PriorityLevel priority,
            ConcurrentPriorityQueue<Frame> sendingPriorityQueue)
        {
            Id = id;
            _sendingPriorityQueue = sendingPriorityQueue;
            Priority = priority;

            _invalidStream = RstStream.InvalidStream(Id);
            _streamInUse = RstStream.StreamInUse(Id);
            _protocolError = RstStream.ProtocolError(Id);
        }

        public UInt31 Id { get; }

        private void CloseRemote()
        {
            Interlocked.Exchange(ref _remote, Closed);
        }
        private void CloseLocal()
        {
            Interlocked.Exchange(ref _local, Closed);
        }

        internal void Receive(
            Frame frame)
        {
            if (_remote == Closed)
            {
                if (frame is RstStream)
                {
                    return;
                }
                Send(_protocolError);
                return;
            }

            switch (frame)
            {
                case SynReply synReply:
                    if (_controlFramesReceived.TryAdd(typeof(SynReply), synReply) == false)
                    {
                        Send(_streamInUse);
                        return;
                    }

                    if (synReply.IsFin)
                    {
                        CloseRemote();
                        return;
                    }

                    break;
                case RstStream rstStream:
                    _controlFramesReceived.TryAdd(typeof(RstStream), rstStream);
                    CloseRemote();
                    CloseLocal();
                    return;
                case Headers headers:
                    if (headers.IsLastFrame)
                    {
                        CloseRemote();
                    }

                    if (headers.Values.Any(pair => pair.Key.Length == 0))
                    {
                        Send(_protocolError);
                        return;
                    }

                    break;
                case Data data:
                    // If the endpoint which created the stream receives a data frame before receiving a SYN_REPLY on that stream, it is a protocol error, and the recipient MUST issue a stream error (Section 2.4.2) with the status code PROTOCOL_ERROR for the stream-id.
                    if (_controlFramesReceived.ContainsKey(typeof(SynReply)) == false)
                    {
                        Send(_protocolError);
                        return;
                    }

                    if (data.IsLastFrame)
                    {
                        CloseRemote();
                    }

                    _receivingQueue.Enqueue(data);
                    _frameAvailable.Release();
                    return;
            }

            throw new InvalidOperationException($"{frame.GetType()} was not handled");
        }

        public SynStream.PriorityLevel Priority { get; }

        public void Open()
        {
            var open = new SynStream(
                SynStream.Options.None, Id, UInt31.From(0), Priority,
                new Dictionary<string, string[]>());
            _sendingPriorityQueue.Enqueue(Priority, open);
        }

        private void Send(
            Frame frame)
        {
            if (frame is RstStream)
            {
                CloseRemote();
                CloseLocal();
            }

            _sendingPriorityQueue.Enqueue(Priority, frame);
        }

        public void Send(
            Data frame)
        {
            if (_local == Closed)
            {
                throw new InvalidOperationException("The stream is closed");
            }
            
            Send((Frame)frame);
        }

        public async Task<Data> ReadAsync(
            TimeSpan timeout = default,
            CancellationToken cancellationToken = default)
        {
            if (timeout == default)
            {
                await _frameAvailable.WaitAsync(cancellationToken)
                                     .ConfigureAwait(false);
            }
            else
            {
                await _frameAvailable.WaitAsync(timeout, cancellationToken)
                                     .ConfigureAwait(false);
            }
            if (_receivingQueue.TryDequeue(out var frame))
            {
                return frame;
            }

            throw new InvalidOperationException(
                "Receiving queue got out of sync");
        }
    }
}