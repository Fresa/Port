using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Log.It;
using Port.Server.Spdy.Collections;
using Port.Server.Spdy.Extensions;
using Port.Server.Spdy.Frames;
using Port.Server.Spdy.Helpers;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    public class SpdySession : IAsyncDisposable
    {
        private readonly ILogger _logger = LogFactory.Create<SpdySession>();
        private readonly INetworkClient _networkClient;
        private readonly bool _isClient;

        private readonly SemaphoreSlimGate _sendingGate =
            SemaphoreSlimGate.OneAtATime;

        private readonly Pipe _messageReceiver = new Pipe();

        private readonly CancellationTokenSource
            _sendingCancellationTokenSource;

        private CancellationToken SendingCancellationToken
            => _sendingCancellationTokenSource.Token;

        private readonly CancellationTokenSource
            _sessionCancellationTokenSource = new CancellationTokenSource();

        private CancellationToken SessionCancellationToken
            => _sessionCancellationTokenSource.Token;

        private readonly Task _sendingTask;
        private readonly Task _receivingTask;
        private readonly Task _messageHandlerTask;

        private readonly ConcurrentPriorityQueue<Frame> _sendingPriorityQueue =
            new ConcurrentPriorityQueue<Frame>();

        private int _lastReceivedStreamId;
        private int _streamCounter;
        private UInt31 _lastGoodRepliedStreamId;

        private readonly BufferBlock<SynStream> _receivedStreamRequests = new BufferBlock<SynStream>();

        private readonly ConcurrentDictionary<UInt31, SpdyStream> _streams =
            new ConcurrentDictionary<UInt31, SpdyStream>();

        private readonly
            ObservableConcurrentDictionary<Settings.Id, Settings.Setting>
            _settings =
                new ObservableConcurrentDictionary<Settings.Id, Settings.Setting
                >();

        public IObservableReadOnlyCollection<Settings.Setting> Settings
            => _settings;

        private const int InitialWindowSize = 64000;
        private int _windowSize = InitialWindowSize;

        private SpdySession(
            INetworkClient networkClient,
            bool isClient)
        {
            _pingId = _streamCounter = isClient ? -1 : 0;
            _isClient = isClient;

            _sendingCancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(
                    SessionCancellationToken);

            _networkClient = networkClient;

            _sendingTask = StartBackgroundTaskAsync(SendFramesAsync, _sendingCancellationTokenSource);
            _receivingTask = StartBackgroundTaskAsync(ReceiveFromNetworkClientAsync, _sessionCancellationTokenSource);
            _messageHandlerTask = StartBackgroundTaskAsync(HandleMessagesAsync, _sessionCancellationTokenSource);
        }

        internal static SpdySession CreateClient(
            INetworkClient networkClient)
        {
            return new SpdySession(networkClient, true);
        }

        internal static SpdySession CreateServer(
            INetworkClient networkClient)
        {
            return new SpdySession(networkClient, false);
        }

        private Task StartBackgroundTaskAsync(
            Func<Task> action,
            CancellationTokenSource cancellationTokenSource)
        {
            // ReSharper disable once MethodSupportsCancellation
            // Will gracefully handle cancellation
            return Task.Run(
                async () =>
                {
                    try
                    {
                        await action()
                            .ConfigureAwait(false);
                    }
                    catch when (cancellationTokenSource
                        .IsCancellationRequested)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.Fatal(ex, "Unknown error, closing down");
                        await StopNetworkSenderAsync()
                            .ConfigureAwait(false);
                        try
                        {
                            await SendAsync(
                                    GoAway.InternalError(
                                        UInt31.From(_lastGoodRepliedStreamId)),
                                    SessionCancellationToken)
                                .ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Could not send GoAway");
                        }

                        _sessionCancellationTokenSource.Cancel(false);
                    }
                });
        }

        private async Task SendFramesAsync()
        {
            while (_sendingCancellationTokenSource
                .IsCancellationRequested == false)
            {
                var frame = await _sendingPriorityQueue
                                  .DequeueAsync(SendingCancellationToken)
                                  .ConfigureAwait(false);

                if (frame is Data data)
                {
                    // todo: Should we always wait for data frames to be sent before sending the next frame (which might be a control frame that are not under flow control)?
                    await SendAsync(data, SendingCancellationToken)
                        .ConfigureAwait(false);
                    continue;
                }

                await SendAsync(frame, SendingCancellationToken)
                    .ConfigureAwait(false);
            }
        }

        private Task StopNetworkSenderAsync()
        {
            _sendingCancellationTokenSource.Cancel();
            return _sendingTask;
        }

        private async Task SendAsync(
            Frame frame,
            CancellationToken cancellationToken)
        {
            using (await _sendingGate.WaitAsync(cancellationToken)
                                     .ConfigureAwait(false))
            {
                await _networkClient.SendAsync(frame, cancellationToken)
                                    .ConfigureAwait(false);
            }
        }

        private readonly SemaphoreSlimGate _sendDataGate =
            SemaphoreSlimGate.OneAtATime;

        private async Task SendAsync(
            Data data,
            CancellationToken cancellationToken)
        {
            if (data.Payload.Length > 0)
            {
                using (await _sendDataGate.WaitAsync(cancellationToken)
                                          .ConfigureAwait(false))
                {
                    using var windowSizeIncreased = _windowSizeIncreased.Subscribe();
                    while (Interlocked.Add(
                               ref _windowSize, -data.Payload.Length) <
                           0)
                    {
                        Interlocked.Add(ref _windowSize, data.Payload.Length);
                        await windowSizeIncreased
                              .WaitAsync(SessionCancellationToken)
                              .ConfigureAwait(false);
                    }
                }
            }

            await SendAsync((Frame)data, cancellationToken)
                .ConfigureAwait(false);
        }

        private readonly Signaler
            _windowSizeIncreased = new Signaler();

        private void TryIncreaseWindowSizeOrCloseSession(
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
                _sendingPriorityQueue.Enqueue(SynStream.PriorityLevel.Top,
                        RstStream.FlowControlError(_lastGoodRepliedStreamId));

                _sessionCancellationTokenSource.Cancel(false);
                return;
            }

            if (newWindowSize > 0)
            {
                _windowSizeIncreased.Signal();
            }
        }

        private long _pingId;

        private void EnqueuePing()
        {
            var id = Interlocked.Add(ref _pingId, 2);
            if (id > uint.MaxValue)
            {
                id = id.IsOdd() ? 1 : 2;
                Interlocked.Exchange(ref _pingId, id);
            }

            var ping = new Ping((uint)id);
            _sendingPriorityQueue.Enqueue(SynStream.PriorityLevel.Top, ping);
        }

        private async Task<(bool Found, SpdyStream Stream)>
            TryGetStreamOrCloseSessionAsync(
                UInt31 streamId)
        {
            if (_streams.TryGetValue(
                streamId,
                out var stream))
            {
                return (true, stream);
            }

            await StopNetworkSenderAsync()
                .ConfigureAwait(false);
            await SendAsync(
                    GoAway.ProtocolError(_lastGoodRepliedStreamId),
                    SessionCancellationToken)
                .ConfigureAwait(false);

            _sessionCancellationTokenSource.Cancel(false);
            return (false, stream)!;
        }

        private async Task ReceiveFromNetworkClientAsync()
        {
            FlushResult result;
            do
            {
                var bytes = await _networkClient
                                  .ReceiveAsync(
                                      _messageReceiver.Writer.GetMemory(),
                                      SessionCancellationToken)
                                  .ConfigureAwait(false);

                // End of the stream! 
                // https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.sockettaskextensions.receiveasync?view=netcore-3.1
                if (bytes == 0)
                {
                    _logger.Info("Got 0 bytes, stopping receiving more from the network client");
                    return;
                }
                _messageReceiver.Writer.Advance(bytes);
                result = await _messageReceiver
                               .Writer.FlushAsync(SessionCancellationToken)
                               .ConfigureAwait(false);
            } while (_sessionCancellationTokenSource
                         .IsCancellationRequested ==
                     false &&
                     result.IsCanceled == false &&
                     result.IsCompleted == false);
        }

        private async Task HandleMessagesAsync()
        {
            var frameReader = new FrameReader(_messageReceiver.Reader);

            while (_sessionCancellationTokenSource
                .IsCancellationRequested == false)
            {
                await HandleNextMessageAsync(frameReader)
                    .ConfigureAwait(false);
            }
        }

        private async Task HandleNextMessageAsync(
            IFrameReader frameReader)
        {
            if ((await Frame.TryReadAsync(
                                frameReader,
                                SessionCancellationToken)
                            .ConfigureAwait(false)).Out(
                out var frame, out var error) == false)
            {
                _sendingPriorityQueue.Enqueue(SynStream.PriorityLevel.High, error);
                return;
            }

            bool found;
            SpdyStream? stream;
            switch (frame)
            {
                case SynStream synStream:
                    var previousId = Interlocked.Exchange(
                        ref _lastReceivedStreamId, synStream.StreamId);

                    if (previousId >= synStream.StreamId ||
                        _isClient == synStream.IsClient())
                    {
                        await StopNetworkSenderAsync()
                            .ConfigureAwait(false);
                        await SendAsync(
                                GoAway.ProtocolError(_lastGoodRepliedStreamId),
                                SessionCancellationToken)
                            .ConfigureAwait(false);

                        _sessionCancellationTokenSource.Cancel(false);
                        return;
                    }

                    await _receivedStreamRequests
                          .SendAsync(
                              synStream,
                              SessionCancellationToken)
                          .ConfigureAwait(false);
                    break;
                case SynReply synReply:
                    (found, stream) =
                        await TryGetStreamOrCloseSessionAsync(synReply.StreamId)
                            .ConfigureAwait(false);
                    if (found == false)
                    {
                        return;
                    }

                    stream.Receive(frame);
                    break;
                case RstStream rstStream:
                    (found, stream) =
                        await TryGetStreamOrCloseSessionAsync(rstStream.StreamId)
                            .ConfigureAwait(false);
                    if (found == false)
                    {
                        return;
                    }

                    stream.Receive(frame);
                    break;
                case Settings settings:
                    if (settings.ClearSettings)
                    {
                        _settings.Clear();
                    }

                    foreach (var setting in settings.Values)
                    {
                        _settings[setting.Id] =
                            new Settings.Setting(
                                setting.Id,
                                // todo: Persist values!
                                setting.ShouldPersist
                                    ? Frames.Settings.ValueOptions.Persisted
                                    : Frames.Settings.ValueOptions.None,
                                setting.Value);
                    }

                    foreach (var spdyStream in _streams.Values)
                    {
                        spdyStream.Receive(settings);
                    }

                    break;
                case Ping ping:
                    // If a client receives an odd numbered PING which it did not initiate, it must ignore the PING.
                    if (ping.Id % 2 != 0)
                    {
                        break;
                    }

                    // Pong
                    _sendingPriorityQueue.Enqueue(
                        SynStream.PriorityLevel.Top, ping);
                    break;
                case GoAway goAway:
                    _sessionCancellationTokenSource.Cancel(false);
                    Interlocked.Exchange(
                        ref _streamCounter, goAway.LastGoodStreamId);
                    return;
                case Headers headers:
                    (found, stream) =
                        await TryGetStreamOrCloseSessionAsync(headers.StreamId)
                            .ConfigureAwait(false);
                    if (found == false)
                    {
                        return;
                    }

                    stream.Receive(headers);
                    break;
                case WindowUpdate windowUpdate:
                    if (windowUpdate
                        .IsConnectionFlowControl)
                    {
                        TryIncreaseWindowSizeOrCloseSession(
                                windowUpdate.DeltaWindowSize);
                        break;
                    }

                    (found, stream) =
                        await TryGetStreamOrCloseSessionAsync(windowUpdate.StreamId)
                            .ConfigureAwait(false);
                    if (found == false)
                    {
                        return;
                    }

                    stream.Receive(windowUpdate);
                    break;
                case Data data:
                    if (_streams.TryGetValue(
                        data.StreamId,
                        out stream))
                    {
                        stream.Receive(data);
                        if (data.Payload.Length > 0)
                        {
                            _sendingPriorityQueue.Enqueue(SynStream
                                    .PriorityLevel.AboveNormal,
                                WindowUpdate.ConnectionFlowControl(
                                    (uint)data.Payload.Length));
                        }

                        break;
                    }

                    // If an endpoint receives a data frame for a stream-id which is not open and the endpoint has not sent a GOAWAY (Section 2.6.6) frame, it MUST issue a stream error (Section 2.4.2) with the error code INVALID_STREAM for the stream-id.
                    _sendingPriorityQueue.Enqueue(
                        SynStream.PriorityLevel.High,
                        RstStream.InvalidStream(data.StreamId));
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Frame {frame.GetType().FullName} is unknown");
            }
        }

        public SpdyStream Open(
            SynStream.PriorityLevel priority = SynStream.PriorityLevel.Normal,
            SynStream.Options options = SynStream.Options.None,
            IReadOnlyDictionary<string, IReadOnlyList<string>>? headers = null)
        {
            headers ??= new Dictionary<string, IReadOnlyList<string>>();
            var streamId = (uint)Interlocked.Add(ref _streamCounter, 2);

            var stream = SpdyStream.Open(
                    new SynStream(options, streamId, UInt31.From(0), priority, headers),
                _sendingPriorityQueue);
            _streams.TryAdd(stream.Id, stream);

            return stream;
        }

        private readonly SemaphoreSlimGate _receiveGate = SemaphoreSlimGate.OneAtATime;
        public async Task<SpdyStream> ReceiveAsync(
            CancellationToken cancellationToken = default)
        {
            using (await _receiveGate.WaitAsync(cancellationToken)
                              .ConfigureAwait(false))
            {
                while (true)
                {
                    var synStream = await _receivedStreamRequests
                                          .ReceiveAsync(cancellationToken)
                                          .ConfigureAwait(false);

                    if (_streams.ContainsKey(synStream.StreamId))
                    {
                        _sendingPriorityQueue.Enqueue(
                            SynStream.PriorityLevel.Urgent,
                            RstStream.ProtocolError(synStream.StreamId));
                        continue;
                    }

                    var stream = SpdyStream.Accept(synStream, _sendingPriorityQueue);
                    _streams.TryAdd(stream.Id, stream);
                    // This is thread safe since this is the only place
                    // where this property changes and we are inside
                    // a one-at-a-time gate
                    _lastGoodRepliedStreamId = stream.Id;

                    return stream;
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            var isClosed = _sessionCancellationTokenSource
                .IsCancellationRequested;
            try
            {
                _sessionCancellationTokenSource.Cancel(false);
            }
            catch
            {
                // Try cancel
            }

            await Task.WhenAll(
                          _receivingTask, _sendingTask, _messageHandlerTask)
                      .ConfigureAwait(false);

            if (isClosed == false)
            {
                await SendAsync(
                        GoAway.Ok(UInt31.From(_lastGoodRepliedStreamId)),
                        CancellationToken.None)
                    .ConfigureAwait(false);
            }

            _sendDataGate.Dispose();
            _receiveGate.Dispose();
            _windowSizeIncreased.Dispose();
            await _networkClient.DisposeAsync()
                                .ConfigureAwait(false);
        }
    }
}