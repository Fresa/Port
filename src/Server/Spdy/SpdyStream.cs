using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Frames;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    public sealed class SpdyStream : IDisposable
    {
        private readonly ConcurrentPriorityQueue<Frame> _sendingPriorityQueue;

        private readonly ConcurrentQueue<Data> _receivingQueue
            = new ConcurrentQueue<Data>();
        private readonly SemaphoreSlim _frameAvailable = new SemaphoreSlim(0);

        private readonly RstStream _streamInUse;
        private readonly RstStream _protocolError;
        private readonly RstStream _flowControlError;
        private readonly RstStream _streamAlreadyClosedError;

        private readonly ConcurrentDictionary<Type, Control> _controlFramesReceived = new ConcurrentDictionary<Type, Control>();

        private readonly ConcurrentDictionary<string, string[]> _headers = new ConcurrentDictionary<string, string[]>();

        private bool IsRemoteClosed => _remoteStream.IsCancellationRequested;
        private bool IsLocalClosed => _localStream.IsCancellationRequested;

        private int _windowSize = 64000;

        internal SpdyStream(
            UInt31 id,
            SynStream.PriorityLevel priority,
            ConcurrentPriorityQueue<Frame> sendingPriorityQueue)
        {
            Id = id;
            _sendingPriorityQueue = sendingPriorityQueue;
            Priority = priority;

            _streamInUse = RstStream.StreamInUse(Id);
            _protocolError = RstStream.ProtocolError(Id);
            _flowControlError = RstStream.FlowControlError(Id);
            _streamAlreadyClosedError = RstStream.StreamAlreadyClosed(Id);
        }

        public UInt31 Id { get; }

        private void CloseRemote()
        {
            _remoteStream.Cancel(false);
        }
        private void CloseLocal()
        {
            _localStream.Cancel(false);
        }

        internal void Receive(
            Frame frame)
        {
            if (IsRemoteClosed)
            {
                if (IsLocalClosed)
                {
                    Send(_protocolError);
                    return;
                }

                switch (frame)
                {
                    case RstStream _:
                        return;
                    case WindowUpdate _:
                        break;
                    default:
                        Send(_streamAlreadyClosedError);
                        return;
                }
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

                    return;
                case RstStream _:
                    CloseRemote();
                    CloseLocal();
                    return;
                case Headers headers:
                    if (headers.IsLastFrame)
                    {
                        CloseRemote();
                    }

                    foreach (var (key, values) in headers.Values)
                    {
                        if (_headers.TryAdd(key, values))
                        {
                            continue;
                        }

                        Send(_protocolError);
                        break;
                    }
                    break;
                case WindowUpdate windowUpdate:
                    TryIncreaseWindowSize(windowUpdate.DeltaWindowSize);
                    return;
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

        internal void Open()
        {
            var open = new SynStream(
                SynStream.Options.None, Id, UInt31.From(0), Priority,
                new Dictionary<string, string[]>());
            Send(open);
        }

        private void Send(
            RstStream rstStream)
        {
            CloseRemote();
            CloseLocal();

            Send((Frame)rstStream);
        }

        private void Send(
            Frame frame)
        {
            _sendingPriorityQueue.Enqueue(Priority, frame);
        }

        private readonly CancellationTokenSource _localStream = new CancellationTokenSource();
        private readonly SemaphoreSlimGate _sendingGate = SemaphoreSlimGate.OneAtATime;
        public async Task SendAsync(
            Data data,
            TimeSpan timeout = default,
            CancellationToken cancellationToken = default)
        {
            var tokens = new List<CancellationToken>
            {
                _localStream.Token,
                cancellationToken
            };

            if (timeout != default)
            {
                tokens.Add(new CancellationTokenSource(timeout).Token);
            }

            var token = CancellationTokenSource
                        .CreateLinkedTokenSource(tokens.ToArray())
                        .Token;

            using var gate = await _sendingGate.WaitAsync(token)
                                               .ConfigureAwait(false);
            {
                while (Interlocked.Add(ref _windowSize, -data.Payload.Length) < 0)
                {
                    Interlocked.Add(ref _windowSize, data.Payload.Length);
                    await _windowSizeGate.WaitAsync(token)
                                         .ConfigureAwait(false);
                }
            }

            Send(data);

            if (data.IsLastFrame)
            {
                CloseLocal();
            }
        }

        private readonly SemaphoreSlim _windowSizeGate = new SemaphoreSlim(1, 1);

        private void TryIncreaseWindowSize(
            int delta)
        {
            var newWindowSize = Interlocked.Add(ref _windowSize, delta);
            try
            {
                // Check if we encountered overflow
                _ = checked(newWindowSize - delta);
            }
            catch (OverflowException)
            {
                Send(_flowControlError);
                return;
            }

            if (newWindowSize > 0)
            {
                _windowSizeGate.Release();
            }
        }

        private readonly CancellationTokenSource _remoteStream = new CancellationTokenSource();
        public async Task<Data> ReceiveAsync(
            TimeSpan timeout = default,
            CancellationToken cancellationToken = default)
        {
            var tokens = new List<CancellationToken>
            {
                _remoteStream.Token,
                cancellationToken
            };
            if (timeout != default)
            {
                tokens.Add(new CancellationTokenSource(timeout).Token);
            }
            var token = CancellationTokenSource
                        .CreateLinkedTokenSource(tokens.ToArray())
                        .Token;

            do
            {
                if (_receivingQueue.TryDequeue(out var frame))
                {
                    return frame;
                }

                await _frameAvailable.WaitAsync(token)
                                     .ConfigureAwait(false);
            } while (true);
        }

        public void Dispose()
        {
            _frameAvailable.Dispose();
            _localStream.Dispose();
            _windowSizeGate.Dispose();
            _remoteStream.Dispose();
            _sendingGate.Dispose();
        }
    }
}