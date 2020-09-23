﻿using System;
using System.Buffers;
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

        public IReadOnlyDictionary<string, string[]> Headers => _headers;

        private bool IsRemoteClosed => _remoteStream.IsCancellationRequested;
        private bool IsLocalClosed => _localStream.IsCancellationRequested;

        private int _windowSize = 64000;
        private int _initialWindowSize = 64000;

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

                    SetHeaders(synReply.Headers);

                    if (synReply.IsLastFrame)
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

                    if (data.IsLastFrame)
                    {
                        CloseRemote();
                    }

                    _receivingQueue.Enqueue(data);
                    _frameAvailable.Release();
                    return;
                default:
                    throw new InvalidOperationException($"{frame.GetType()} was not handled");
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

                Send(_protocolError);
                break;
            }
        }

        public SynStream.PriorityLevel Priority { get; }

        internal void Open(
            SynStream.Options options,
            IReadOnlyDictionary<string, string[]> headers)
        {
            var open = new SynStream(
                options, Id, UInt31.From(0), Priority,
                headers);

            if (!open.IsUnidirectional)
            {
                Interlocked.Exchange(
                    ref _remoteStream, new CancellationTokenSource());
            }

            if (!open.IsFin)
            {
                Interlocked.Exchange(
                    ref _localStream, new CancellationTokenSource());
            }

            Send(open);
        }

        public void Close()
        {
            if (IsLocalClosed)
            {
                return;
            }

            CloseLocal();
            Send(Data.Last(Id, new byte[0]));
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

        private CancellationTokenSource _localStream = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken(true));
        private readonly ExclusiveLock _sendLock = new ExclusiveLock();

        public async Task SendAsync(
            ReadOnlyMemory<byte> data,
            TimeSpan timeout = default,
            CancellationToken cancellationToken = default)
        {
            await SendDataAsync(
                    data, false, timeout, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task SendLastAsync(
            ReadOnlyMemory<byte> data,
            TimeSpan timeout = default,
            CancellationToken cancellationToken = default)
        {
            await SendDataAsync(
                    data, true, timeout, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task SendDataAsync(
            ReadOnlyMemory<byte> data,
            bool isFin,
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
                    var windowSize = _windowSize;
                    var length = windowSize > left ? left : windowSize;

                    if (length <= 0)
                    {
                        await _windowSizeGate.WaitAsync(token)
                                             .ConfigureAwait(false);
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
        }

        public Task SendHeadersAsync(
            IReadOnlyDictionary<string, string[]> headers,
            Headers.Options options = Frames.Headers.Options.None,
            TimeSpan timeout = default,
            CancellationToken cancellationToken = default)
        {
            if (_localStream.IsCancellationRequested)
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

        private readonly SemaphoreSlim _windowSizeGate = new SemaphoreSlim(1, 1);

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

            if (newWindowSize > 0)
            {
                _windowSizeGate.Release();
            }
        }

        private CancellationTokenSource _remoteStream = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken(true));
        public async Task<System.IO.Pipelines.ReadResult> ReceiveAsync(
            TimeSpan timeout = default,
            CancellationToken cancellationToken = default)
        {
            var tokens = new List<CancellationToken>
            {
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
                    return new System.IO.Pipelines.ReadResult(
                        new ReadOnlySequence<byte>(frame.Payload), false,
                        frame.IsLastFrame);
                }

                try
                {
                    await _frameAvailable.WaitAsync(token)
                                         .ConfigureAwait(false);
                }
                catch when (_remoteStream.IsCancellationRequested)
                {
                    return new System.IO.Pipelines.ReadResult(
                        ReadOnlySequence<byte>.Empty, true, false);
                }
            } while (true);
        }

        public void Dispose()
        {
            if (IsLocalClosed == false)
            {
                Send(RstStream.Cancel(Id));
            }
            CloseLocal();
            CloseRemote();

            _frameAvailable.Dispose();
            _localStream.Dispose();
            _windowSizeGate.Dispose();
            _remoteStream.Dispose();
        }
    }
}