using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Spdy.Collections;
using Spdy.Endpoint;
using Spdy.Extensions;
using Spdy.Frames;
using Spdy.Helpers;
using Spdy.Logging;
using Spdy.Primitives;

namespace Spdy
{
    public sealed class SpdyStream : IDisposable
    {
        private readonly ILogger _logger = LogFactory.Create<SpdyStream>();
        private readonly SynStream _synStream;
        private readonly ConcurrentPriorityQueue<Frame> _sendingPriorityQueue;

        private readonly ConcurrentQueue<Data> _receivingQueue
            = new ConcurrentQueue<Data>();
        private readonly SemaphoreSlim _frameAvailable = new SemaphoreSlim(0);

        private readonly RstStream _streamInUse;
        private readonly RstStream _protocolError;
        private readonly RstStream _flowControlError;
        private readonly RstStream _streamAlreadyClosedError;

        private readonly ConcurrentDistinctTypeBag _controlFramesReceived = new ConcurrentDistinctTypeBag();
        private readonly ConcurrentDistinctTypeBag _controlFramesSent = new ConcurrentDistinctTypeBag();

        private readonly ObservableConcurrentDictionary<string, string[]> _headers = new ObservableConcurrentDictionary<string, string[]>();

        public IObservableReadOnlyDictionary<string, string[]> Headers => _headers;
        public int SessionId { get; }

        private int _windowSize = 64000;
        private int _initialWindowSize = 64000;

        private SpdyStream(
            int sessionId,
            SynStream synStream,
            ConcurrentPriorityQueue<Frame> sendingPriorityQueue)
        {
            _synStream = synStream;
            _sendingPriorityQueue = sendingPriorityQueue;

            _streamInUse = RstStream.StreamInUse(Id);
            _protocolError = RstStream.ProtocolError(Id);
            _flowControlError = RstStream.FlowControlError(Id);
            _streamAlreadyClosedError = RstStream.StreamAlreadyClosed(Id);
            SessionId = sessionId;
        }

        public UInt31 Id => _synStream.StreamId;

        private readonly SpdyEndpoint _local = new SpdyEndpoint();
        public IEndpoint Local => _local;
        private readonly SpdyEndpoint _remote = new SpdyEndpoint();
        public IEndpoint Remote => _remote;

        private void OpenRemote()
        {
            if (_remote.Open())
            {
                _logger.Debug("[{SessionId}:{StreamId}]: Remote opened", SessionId, Id);
            }
        }
        private void CloseRemote()
        {
            if (_remote.Close())
            {
                _logger.Debug("[{SessionId}:{StreamId}]: Remote closed", SessionId, Id);
            }
        }
        private void OpenLocal()
        {
            if (_local.Open())
            {
                _logger.Debug("[{SessionId}:{StreamId}]: Local opened", SessionId, Id);
            }
        }
        private void CloseLocal()
        {
            if (_local.Close())
            {
                _logger.Debug("[{SessionId}:{StreamId}]: Local closed", SessionId, Id);
            }
        }

        internal void Receive(
            Frame frame)
        {
            if (Remote.IsClosed)
            {
                switch (frame)
                {
                    case RstStream _:
                        CloseLocal();
                        return;
                    case WindowUpdate _:
                        break;
                    case SynReply _:
                        break;
                    case Data _:
                        if (Local.IsClosed)
                        {
                            _logger.Error(
                                _protocolError,
                                "[{SessionId}:{StreamId}]: The stream is fully closed. Received: {@Frame}",
                                SessionId, Id,
                                frame.ToStructuredLogging());
                            Send(_protocolError);
                            return;
                        }

                        // If an endpoint receives a data frame after the stream is half-closed from the
                        // sender (e.g. the endpoint has already received a prior frame for the stream
                        // with the FIN flag set), it MUST send a RST_STREAM to the sender with the
                        // status STREAM_ALREADY_CLOSED.
                        _logger.Error(
                            _protocolError,
                            "[{SessionId}:{StreamId}]: The remote stream is closed. Received: {@Frame}",
                            SessionId, Id, frame.ToStructuredLogging());
                        Send(_streamAlreadyClosedError);
                        return;
                    default:
                        return;
                }
            }

            switch (frame)
            {
                case SynReply synReply:
                    if (_controlFramesReceived.TryAdd<SynReply>() == false)
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
                case RstStream rstStream:
                    _controlFramesReceived.TryAdd<RstStream>();
                    _logger.Warning(
                        rstStream,
                        "[{SessionId}:{StreamId}]: Received {FrameType}, closing stream. {@Frame}",
                        SessionId, Id, frame.GetType()
                                            .Name, frame);
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
                    // If the endpoint which created the stream receives a data frame
                    // before receiving a SYN_REPLY on that stream, it is a protocol
                    // error, and the recipient MUST issue a stream error (Section 2.4.2)
                    // with the status code PROTOCOL_ERROR for the stream-id.
                    if (_controlFramesReceived.Contains<SynReply>() == false)
                    {
                        _logger.Error(
                            _protocolError,
                            "[{SessionId}:{StreamId}]: {FrameType} has already been received",
                            SessionId, Id, nameof(SynReply));
                        Send(_protocolError);
                        return;
                    }

                    _receivingQueue.Enqueue(data);
                    _frameAvailable.Release();

                    if (data.IsLastFrame)
                    {
                        CloseRemote();
                    }
                    return;
                default:
                    throw new InvalidOperationException(
                        $"[{SessionId}:{Id}]: {frame.GetType()} was not handled");
            }
        }

        private void SetHeaders(
            IReadOnlyDictionary<string, string[]> headers)
        {
            foreach (var (key, values) in headers)
            {
                if (_headers.TryAdd(key, values))
                {
                    continue;
                }

                _logger.Error(
                    _protocolError,
                    "[{SessionId}:{StreamId}]: " +
                    $"Header with key '{key}' has been sent twice",
                    SessionId,
                    Id);
                Send(_protocolError);
                break;
            }
        }

        public SynStream.PriorityLevel Priority => _synStream.Priority;

        internal static SpdyStream Accept(
            int sessionId,
            SynStream synStream,
            ConcurrentPriorityQueue<Frame> sendingPriorityQueue,
            NameValueHeaderBlock? headers =
                default)
        {
            var stream = new SpdyStream(sessionId, synStream, sendingPriorityQueue);
            stream.Accept(headers);
            return stream;
        }

        private void Accept(NameValueHeaderBlock? headers =
            default)
        {
            _controlFramesReceived.TryAdd<SynReply>();
            var reply = SynReply.Accept(Id, headers);

            if (_synStream.IsUnidirectional)
            {
                CloseRemote();
            }
            else
            {
                OpenRemote();
            }

            if (_synStream.IsFin || reply.IsLastFrame)
            {
                CloseLocal();
            }
            else
            {
                OpenLocal();
            }

            Send(reply);
        }

        internal static SpdyStream Open(
            int sessionId,
            SynStream synStream,
            ConcurrentPriorityQueue<Frame> sendingPriorityQueue)
        {
            var stream = new SpdyStream(sessionId, synStream, sendingPriorityQueue);
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

            _controlFramesSent.TryAdd<RstStream>();
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
                    throw new InvalidOperationException(
                        $"[{SessionId}:{Id}]: Data is currently being sent");
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

            return new FlushResult(false, isFin);
        }

        public Task<FlushResult> SendHeadersAsync(
            NameValueHeaderBlock headers,
            Headers.Options options = Frames.Headers.Options.None,
            TimeSpan timeout = default,
            CancellationToken cancellationToken = default)
        {
            if (Local.IsClosed)
            {
                return Task.FromResult(new FlushResult(true, false));
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

            return Task.FromResult(new FlushResult(false, true));
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
                    _logger.Trace(
                        "[{SessionId}:{StreamId}]: Waiting for an available frame",
                        SessionId, Id);
                    await _frameAvailable.WaitAsync(token)
                                         .ConfigureAwait(false);
                    _logger.Trace(
                        "[{SessionId}:{StreamId}]: Available frame signaled",
                        SessionId, Id);
                }
                catch when (token.IsCancellationRequested)
                {
                    // There is a race condition between the remote closed signaler
                    // and the frame available semaphore which sometimes causes the 
                    // frame available semaphore to cancel when it is setting up the 
                    // awaitable task due to it's bail fast strategy.
                    // Give the receiver a chance to retrieve any data that has still
                    // not been consumed before cancelling.
                    if (_receivingQueue.IsEmpty)
                    {
                        return new System.IO.Pipelines.ReadResult(
                            ReadOnlySequence<byte>.Empty, true, false);
                    }
                }
            } while (true);
        }

        public void Dispose()
        {
            if (Local.IsOpen)
            {
                Send(RstStream.Cancel(Id));
            }
            
            if (Remote.IsOpen)
            {
                CloseRemote();
            }

            _frameAvailable.Dispose();
            _windowSizeGate.Dispose();
            _local.Dispose();
            _remote.Dispose();
        }
    }
}