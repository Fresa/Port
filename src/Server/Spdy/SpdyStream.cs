using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Log.It;
using Port.Server.Spdy.Collections;
using Port.Server.Spdy.Endpoint;
using Port.Server.Spdy.Frames;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    public sealed class SpdyStream : IDisposable
    {
        private ILogger _logger = LogFactory.Create<SpdyStream>();
        private readonly SynStream _synStream;
        private readonly ConcurrentPriorityQueue<Frame> _sendingPriorityQueue;

        private readonly ConcurrentQueue<Data> _receivingQueue
            = new ConcurrentQueue<Data>();
        private readonly SemaphoreSlim _frameAvailable = new SemaphoreSlim(0);

        private readonly RstStream _streamInUse;
        private readonly RstStream _protocolError;
        private readonly RstStream _flowControlError;
        private readonly RstStream _streamAlreadyClosedError;

        private readonly ConcurrentDictionary<Type, Control> _controlFramesReceived = new ConcurrentDictionary<Type, Control>();

        private readonly ObservableConcurrentDictionary<string, IReadOnlyList<string>> _headers = new ObservableConcurrentDictionary<string, IReadOnlyList<string>>();

        public IObservableReadOnlyDictionary<string, IReadOnlyList<string>> Headers => _headers;

        private int _windowSize = 64000;
        private int _initialWindowSize = 64000;

        private SpdyStream(
            SynStream synStream,
            ConcurrentPriorityQueue<Frame> sendingPriorityQueue)
        {
            _synStream = synStream;
            _sendingPriorityQueue = sendingPriorityQueue;

            _streamInUse = RstStream.StreamInUse(Id);
            _protocolError = RstStream.ProtocolError(Id);
            _flowControlError = RstStream.FlowControlError(Id);
            _streamAlreadyClosedError = RstStream.StreamAlreadyClosed(Id);
        }

        public UInt31 Id => _synStream.StreamId;

        private readonly SpdyEndpoint _local = new SpdyEndpoint();
        public IEndpoint Local => _local;
        private readonly SpdyEndpoint _remote = new SpdyEndpoint();
        public IEndpoint Remote => _remote;

        private void OpenRemote()
        {
            _remote.Open();
            _logger.Trace("Remote opened");
        }
        private void CloseRemote()
        {
            _remote.Close();
            _logger.Trace("Remote closed");
        }
        private void OpenLocal()
        {
            _local.Open();
            _logger.Trace("Local opened");
        }
        private void CloseLocal()
        {
            _local.Close();
            _logger.Trace("Local closed");
        }

        internal void Receive(
            Frame frame)
        {
            if (Remote.IsClosed)
            {
                switch (frame)
                {
                    case RstStream _:
                        return;
                    case WindowUpdate _:
                        break;
                    case SynReply _:
                        break;
                    default:
                        if (Local.IsClosed)
                        {
                            Send(_protocolError);
                            return;
                        }
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

                    SetHeaders(synReply.Headers);

                    if (synReply.IsLastFrame)
                    {
                        CloseRemote();
                        return;
                    }
                    else
                    {
                        OpenRemote();
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

                    SetHeaders(headers.Values);
                    break;
                case WindowUpdate windowUpdate:
                    IncreaseWindowSize(windowUpdate.DeltaWindowSize);
                    return;
                case Settings settings:
                    foreach (var setting in settings.Values)
                    {
                        switch (setting.Id)
                        {
                            case Settings.Id.InitialWindowSize:
                                IncreaseWindowSize(
                                    (int)setting.Value - _initialWindowSize);
                                Interlocked.Exchange(
                                    ref _initialWindowSize,
                                    (int)setting.Value);
                                break;
                        }
                    }

                    break;
                case Data data:
                    // If the endpoint which created the stream receives a data frame before receiving a SYN_REPLY on that stream, it is a protocol error, and the recipient MUST issue a stream error (Section 2.4.2) with the status code PROTOCOL_ERROR for the stream-id.
                    if (_controlFramesReceived.ContainsKey(typeof(SynReply)) == false)
                    {
                        Send(_protocolError);
                        return;
                    }

                    _receivingQueue.Enqueue(data);
                    _frameAvailable.Release();

                    _logger.Trace("{length} bytes data received", data.Payload.Length);
                    if (data.IsLastFrame)
                    {
                        CloseRemote();
                    }
                    return;
                default:
                    throw new InvalidOperationException($"{frame.GetType()} was not handled");
            }
        }

        private void SetHeaders(
            IReadOnlyDictionary<string, IReadOnlyList<string>> headers)
        {
            foreach (var (key, values) in headers)
            {
                if (_headers.TryAdd(key, values))
                {
                    continue;
                }

                Send(_protocolError);
                break;
            }
        }

        public SynStream.PriorityLevel Priority => _synStream.Priority;

        internal static SpdyStream Accept(
            SynStream synStream,
            ConcurrentPriorityQueue<Frame> sendingPriorityQueue,
            IReadOnlyDictionary<string, IReadOnlyList<string>>? headers =
                default)
        {
            var stream = new SpdyStream(synStream, sendingPriorityQueue);
            stream.Accept(headers);
            return stream;
        }

        private void Accept(IReadOnlyDictionary<string, IReadOnlyList<string>>? headers =
            default)
        {
            if (_synStream.IsUnidirectional)
            {
                CloseRemote();
            }
            else
            {
                OpenRemote();
            }

            var reply = SynReply.Accept(Id, headers);
            if (_synStream.IsFin || reply.IsLastFrame)
            {
                CloseLocal();
            }
            else
            {
                OpenLocal();
            }

            _controlFramesReceived.TryAdd(typeof(SynReply), reply);
            Send(reply);
        }

        internal static SpdyStream Open(
            SynStream synStream,
            ConcurrentPriorityQueue<Frame> sendingPriorityQueue)
        {
            var stream = new SpdyStream(synStream, sendingPriorityQueue);
            stream.Open();
            return stream;
        }

        private void Open()
        {
            if (_synStream.IsUnidirectional)
            {
                CloseRemote();
            }

            if (_synStream.IsFin)
            {
                CloseLocal();
            }
            else
            {
                OpenLocal();
            }

            Send(_synStream);
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

        private readonly ExclusiveLock _sendLock = new ExclusiveLock();

        public Task<FlushResult> SendAsync(
            ReadOnlyMemory<byte> data,
            TimeSpan timeout = default,
            CancellationToken cancellationToken = default)
        {
            return SendDataAsync(
                    data, false, timeout, cancellationToken);
        }

        public Task<FlushResult> SendLastAsync(
            ReadOnlyMemory<byte> data,
            TimeSpan timeout = default,
            CancellationToken cancellationToken = default)
        {
            return SendDataAsync(
                    data, true, timeout, cancellationToken);
        }

        private async Task<FlushResult> SendDataAsync(
            ReadOnlyMemory<byte> data,
            bool isFin,
            TimeSpan timeout = default,
            CancellationToken cancellationToken = default)
        {
            var tokens = new List<CancellationToken>
            {
                _local.Cancellation,
                cancellationToken
            };

            if (timeout != default)
            {
                tokens.Add(new CancellationTokenSource(timeout).Token);
            }

            var token = CancellationTokenSource
                        .CreateLinkedTokenSource(tokens.ToArray())
                        .Token;

            using (_sendLock.TryAcquire(out var acquired))
            {
                if (acquired == false)
                {
                    throw new InvalidOperationException("Data is currently being sent");
                }

                var index = 0;
                var left = data.Length;
                while (left > 0)
                {
                    if (Local.IsClosed)
                    {
                        return new FlushResult(true, false);
                    }
                    
                    var windowSize = _windowSize;
                    var length = windowSize > left ? left : windowSize;

                    if (length <= 0)
                    {
                        try
                        {
                            await _windowSizeGate.WaitAsync(token)
                                                 .ConfigureAwait(false);
                        }
                        catch when (token.IsCancellationRequested)
                        {
                            return new FlushResult(true, false);
                        }

                        continue;
                    }

                    if (Interlocked.Add(ref _windowSize, -length) < 0)
                    {
                        Interlocked.Add(ref _windowSize, length);
                        continue;
                    }

                    var payload = data.Slice(index, length).ToArray();
                    var frame = left == length && isFin
                        ? Data.Last(Id, payload)
                        : new Data(Id, payload);

                    Send(frame);

                    index += length;
                    left = data.Length - index;
                }
            }

            if (isFin)
            {
                CloseLocal();
            }

            return new FlushResult(false, true);
        }

        public Task SendHeadersAsync(
            IReadOnlyDictionary<string, IReadOnlyList<string>> headers,
            Headers.Options options = Frames.Headers.Options.None,
            TimeSpan timeout = default,
            CancellationToken cancellationToken = default)
        {
            if (Local.IsClosed)
            {
                throw new InvalidOperationException("Stream is closed");
            }

            if (options == Frames.Headers.Options.Fin)
            {
                Send(Frames.Headers.Last(Id, headers));
                CloseLocal();
            }
            else
            {
                Send(new Headers(Id, headers));
            }

            return Task.CompletedTask;
        }

        private readonly SemaphoreSlim _windowSizeGate = new SemaphoreSlim(0);

        private void IncreaseWindowSize(
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

            // Check if we transitioned from no buffer available to having buffer at the receiving end
            if (newWindowSize > 0 &&
                newWindowSize <= delta)
            {
                _windowSizeGate.Release();
            }
        }

        public async Task<System.IO.Pipelines.ReadResult> ReceiveAsync(
            TimeSpan timeout = default,
            CancellationToken cancellationToken = default)
        {
            var tokens = new List<CancellationToken>
            {
                cancellationToken,
                _remote.Cancellation
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
                    _logger.Trace(
                        "Received data frame with payload length of {length} bytes",
                        frame.Payload.Length);
                    
                    if (frame.Payload.Length > 0)
                    {
                        Send(new WindowUpdate(Id, (uint)frame.Payload.Length));
                    }

                    return new System.IO.Pipelines.ReadResult(
                        new ReadOnlySequence<byte>(frame.Payload), false,
                        frame.IsLastFrame);
                }

                try
                {
                    _logger.Trace("Waiting for an available frame");
                    await _frameAvailable.WaitAsync(token)
                                         .ConfigureAwait(false);
                    _logger.Trace("Available frame signaled");
                }
                catch when (token.IsCancellationRequested)
                {
                    return new System.IO.Pipelines.ReadResult(
                        ReadOnlySequence<byte>.Empty, true, false);
                }
            } while (true);
        }

        public void Dispose()
        {
            if (Local.IsOpen)
            {
                Send(RstStream.Cancel(Id));
            }
            CloseLocal();
            CloseRemote();

            _frameAvailable.Dispose();
            _windowSizeGate.Dispose();
            _local.Dispose();
            _remote.Dispose();
        }
    }
}