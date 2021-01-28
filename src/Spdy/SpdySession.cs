using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Spdy.Collections;
using Spdy.Extensions;
using Spdy.Frames;
using Spdy.Frames.Readers;
using Spdy.Frames.Writers;
using Spdy.Helpers;
using Spdy.Logging;
using Spdy.Network;
using Spdy.Primitives;

namespace Spdy
{
    public class SpdySession : IAsyncDisposable
    {
        private readonly ILogger _logger = LogFactory.Create<SpdySession>();
        private readonly INetworkClient _networkClient;
        private readonly bool _isClient;
        private readonly Configuration.Configuration _configuration;

        private static int _sessionCounter;

        private readonly FrameWriter _frameWriter;
        private readonly HeaderWriterProvider _headerWriterProvider;
        private readonly FrameReader _frameReader;
        private readonly IHeaderReader _headerReader;

        private readonly SemaphoreSlimGate _sendingGate =
            SemaphoreSlimGate.OneAtATime;

        private readonly Pipe _messageReceiver = new Pipe(new PipeOptions(useSynchronizationContext: false));

        private readonly CancellationTokenSource
            _sendingCancellationTokenSource;

        private readonly CancellationTokenSource
            _sessionCancellationTokenSource = new CancellationTokenSource();

        private CancellationToken SessionCancellationToken
            => _sessionCancellationTokenSource.Token;

        private readonly Task _sendingTask;
        private readonly Task _receivingTask;
        private readonly Task _messageHandlerTask;
        private readonly Task _sendPingTask;

        private readonly ConcurrentPriorityQueue<Frame> _sendingPriorityQueue =
            new ConcurrentPriorityQueue<Frame>();

        private UInt31 _nextStreamId;
        private UInt31 _lastGoodStreamId;

        private readonly BufferBlock<SpdyStream> _receivedStreamRequests = new BufferBlock<SpdyStream>();

        private readonly ConcurrentDictionary<UInt31, SpdyStream> _streams =
            new ConcurrentDictionary<UInt31, SpdyStream>();

        private readonly
            ObservableConcurrentDictionary<Settings.Id, Settings.Setting>
            _settings =
                new ObservableConcurrentDictionary<Settings.Id, Settings.Setting
                >();

        public IObservableReadOnlyCollection<Settings.Setting> Settings
            => _settings;
        public int Id { get; }

        private const int InitialWindowSize = 64000;
        private int _windowSize = InitialWindowSize;

        private SpdySession(
            INetworkClient networkClient,
            bool isClient,
            Configuration.Configuration configuration)
        {
            Id = Interlocked.Increment(ref _sessionCounter);
            _nextPingId = _nextStreamId = isClient ? 1u : 2u;
            _isClient = isClient;
            _configuration = configuration;

            _sendingCancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(
                    SessionCancellationToken);

            _networkClient = networkClient;
            _frameWriter = new FrameWriter(networkClient);
            _headerWriterProvider = new HeaderWriterProvider();
            _frameReader = new FrameReader(_messageReceiver.Reader);
            _headerReader = new HeaderReader(_frameReader);
            _sendingTask = StartBackgroundTaskAsync(SendFramesAsync, _sendingCancellationTokenSource);
            _receivingTask = StartBackgroundTaskAsync(ReceiveFromNetworkClientAsync, _sessionCancellationTokenSource);
            _messageHandlerTask = StartBackgroundTaskAsync(HandleMessagesAsync, _sessionCancellationTokenSource);
            _sendPingTask = StartBackgroundTaskAsync(SendPingsAsync, _sessionCancellationTokenSource);
        }

        public static SpdySession CreateClient(
            INetworkClient networkClient,
            Configuration.Configuration? configuration = default)
        {
            return new SpdySession(
                networkClient, true, configuration ?? Configuration.Configuration.Default);
        }

        public static SpdySession CreateServer(
            INetworkClient networkClient,
            Configuration.Configuration? configuration = default)
        {
            return new SpdySession(
                networkClient, false, configuration ?? Configuration.Configuration.Default);
        }

        private Task StartBackgroundTaskAsync(
            Func<CancellationToken, Task> action,
            CancellationTokenSource cancellationTokenSource)
        {
            // ReSharper disable once MethodSupportsCancellation
            // Will gracefully handle cancellation
            return Task.Run(
                async () =>
                {
                    try
                    {
                        await action(cancellationTokenSource.Token)
                            .ConfigureAwait(false);
                    }
                    catch when (cancellationTokenSource
                        .IsCancellationRequested)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.Fatal(
                            ex,
                            "[{SessionId}]: Unknown error, closing the session",
                            Id);
                        await StopNetworkSenderAsync()
                            .ConfigureAwait(false);
                        try
                        {
                            await SendAsync(
                                    GoAway.InternalError(
                                        _lastGoodStreamId),
                                    SessionCancellationToken)
                                .ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(
                                e, "[{SessionId}]: Could not send GoAway",
                                Id);
                        }

                        _sessionCancellationTokenSource.Cancel(false);
                    }
                });
        }

        private async Task SendFramesAsync(CancellationToken cancellationToken)
        {
            while (cancellationToken
                .IsCancellationRequested == false)
            {
                var frame = await _sendingPriorityQueue
                                  .DequeueAsync(cancellationToken)
                                  .ConfigureAwait(false);

                switch (frame)
                {
                    case Data data:
                        // todo: Should we always wait for data frames to be sent before sending
                        // the next frame (which might be a control frame that are not under
                        // flow control)?
                        await SendAsync(data, cancellationToken)
                            .ConfigureAwait(false);
                        break;
                    case Control control:
                        await SendAsync(control, cancellationToken)
                            .ConfigureAwait(false);
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"[{Id}]: Frame of type {frame.GetType()} is not supported");
                }
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
            _logger.LogSendingFrame(Id, frame);
            using (await _sendingGate.WaitAsync(cancellationToken)
                                     .ConfigureAwait(false))
            {
                await frame.WriteAsync(_frameWriter, _headerWriterProvider, cancellationToken)
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
                _logger.Debug(
                    "[{SessionId}:{StreamId}]: " +
                    $"Waiting to send Data frame with payload size {data.Payload.Length}",
                    Id, data.StreamId);
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
                var error =
                    RstStream.FlowControlError(_lastGoodStreamId);
                _sendingPriorityQueue.Enqueue(SynStream.PriorityLevel.Top,
                        error);
                _logger.Error(
                    error,
                    "[{SessionId}]: Received payload overflowing the buffer window, cancelling the session",
                    Id);
                _sessionCancellationTokenSource.Cancel(false);
                return;
            }

            if (newWindowSize > 0)
            {
                _windowSizeIncreased.Signal();
            }
        }

        private readonly ConcurrentDictionary<UInt31, Stopwatch> _pingsSent = new ConcurrentDictionary<UInt31, Stopwatch>();
        private async Task SendPingsAsync(CancellationToken cancellationToken)
        {
            if (_configuration.Ping.MaxOutstandingPings == 0)
            {
                _logger.Info($"{nameof(_configuration.Ping.MaxOutstandingPings)} is set to 0, ping disabled");
                return;
            }

            while (cancellationToken
                .IsCancellationRequested == false)
            {
                EnqueuePing();

                await Task.Delay(
                              _configuration.Ping.PingInterval,
                              cancellationToken)
                          .ConfigureAwait(false);
            }
        }

        private uint _nextPingId;
        private void EnqueuePing()
        {
            _logger.Debug(
                "[{SessionId}]: " +
                $"{_pingsSent.Count}/{_configuration.Ping.MaxOutstandingPings} pings outstanding",
                Id);
            if (_pingsSent.Count >= _configuration.Ping.MaxOutstandingPings)
            {
                return;
            }

            while (true)
            {
                var nextPingId = InterlockedExtensions.Add(ref _nextPingId, 2);
                var ping = new Ping(nextPingId - 2);

                // If a sender uses all possible PING ids (e.g. has sent all 2^31 possible IDs), it can wrap and start re-using IDs.
                if (nextPingId < ping.Id)
                {
                    var resetPingId = nextPingId.IsOdd() ? 1u : 2u;
                    // Prevent competing pings from resetting to the same id
                    InterlockedExtensions.CompareExchange(
                        ref _nextPingId, resetPingId, nextPingId);
                    continue;
                }

                _pingsSent.TryAdd(ping.Id, Stopwatch.StartNew());
                _sendingPriorityQueue.Enqueue(SynStream.PriorityLevel.Top, ping);
                break;
            }
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
                    GoAway.ProtocolError(_lastGoodStreamId),
                    SessionCancellationToken)
                .ConfigureAwait(false);
            _logger.Error(
                "[{SessionId}]: Stream with id {StreamId} was not found, closing the session",
                Id, streamId);
            _sessionCancellationTokenSource.Cancel(false);
            return (false, stream)!;
        }

        private async Task ReceiveFromNetworkClientAsync(CancellationToken cancellationToken)
        {
            FlushResult result;
            do
            {
                var bytes = await _networkClient
                                  .ReceiveAsync(
                                      _messageReceiver.Writer.GetMemory(),
                                      cancellationToken)
                                  .ConfigureAwait(false);

                // End of the stream! 
                // https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.sockettaskextensions.receiveasync?view=netcore-3.1
                if (bytes == 0)
                {
                    _logger.Info("[{SessionId}]: Got 0 bytes, stopping receiving data from remote", Id);
                    return;
                }
                _messageReceiver.Writer.Advance(bytes);
                result = await _messageReceiver
                               .Writer.FlushAsync(cancellationToken)
                               .ConfigureAwait(false);
            } while (cancellationToken
                         .IsCancellationRequested ==
                     false &&
                     result.HasMore());
        }

        private async Task HandleMessagesAsync(CancellationToken cancellationToken)
        {
            while (cancellationToken
                .IsCancellationRequested == false)
            {
                await HandleNextMessageAsync()
                    .ConfigureAwait(false);
            }
        }

        private async Task HandleNextMessageAsync()
        {
            if ((await Frame.TryReadAsync(
                                _frameReader,
                                _headerReader,
                                SessionCancellationToken)
                            .ConfigureAwait(false)).Out(
                out var frame, out var error) == false)
            {
                _logger.Error(
                    error, "[{SessionId}:{StreamId}]: Sending stream error",
                    Id, error.StreamId);
                _sendingPriorityQueue.Enqueue(SynStream.PriorityLevel.High, error);
                return;
            }

            _logger.LogFrameReceived(Id, frame);

            bool found;
            SpdyStream? stream;
            switch (frame)
            {
                case SynStream synStream:
                    var previousLastGoodStreamId = InterlockedExtensions.Exchange(
                        ref _lastGoodStreamId, synStream.StreamId);

                    if (previousLastGoodStreamId >= synStream.StreamId ||
                        _isClient == synStream.IsClient())
                    {
                        await StopNetworkSenderAsync()
                            .ConfigureAwait(false);
                        _logger.Error(
                            "[{SessionId}]: Received a stream with id {StreamId} which is less than the previous received stream which had id {PreviousStreamId}, closing the session",
                            Id,
                            synStream.StreamId,
                            previousLastGoodStreamId);
                        try
                        {
                            await SendAsync(
                                    GoAway.ProtocolError(previousLastGoodStreamId),
                                    SessionCancellationToken)
                                .ConfigureAwait(false);
                        }
                        finally
                        {
                            _sessionCancellationTokenSource.Cancel(false);
                        }

                        return;
                    }

                    if (_streams.ContainsKey(synStream.StreamId))
                    {
                        _sendingPriorityQueue.Enqueue(
                            SynStream.PriorityLevel.Urgent,
                            RstStream.ProtocolError(synStream.StreamId));
                        return;
                    }

                    stream = SpdyStream.Accept(Id, synStream, _sendingPriorityQueue);
                    _streams.TryAdd(stream.Id, stream);

                    await _receivedStreamRequests
                          .SendAsync(
                              stream,
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
                    if (_isClient != ping.IsOdd())
                    {
                        // Pong
                        _sendingPriorityQueue.Enqueue(
                            SynStream.PriorityLevel.Top, ping);
                        break;
                    }

                    // If a server receives an even numbered PING which it did not initiate,
                    // it must ignore the PING. If a client receives an odd numbered PING
                    // which it did not initiate, it must ignore the PING.
                    if (_pingsSent.TryRemove(ping.Id, out var stopWatch))
                    {
                        stopWatch.Stop();
                        _configuration.Metrics.PingRoundTripTime.Observe(
                            stopWatch.Elapsed);
                    }
                    break;
                case GoAway goAway:
                    _sessionCancellationTokenSource.Cancel(false);
                    InterlockedExtensions.Exchange(
                        ref _nextStreamId, goAway.LastGoodStreamId + 2u);
                    _logger.Info(
                        "[{SessionId}]: Received {FrameType} with reason {GoAwayStatus}, closing the session",
                        Id,
                        nameof(GoAway),
                        goAway.Status.GetName());
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
                    var invalidStream = RstStream.InvalidStream(data.StreamId);
                    _logger.Error(
                        invalidStream,
                        "[{SessionId}]: Received data for an unknown stream with id {StreamId}, sending {RstStatus}",
                        Id,
                        data.StreamId, invalidStream.Status.GetName());
                    _sendingPriorityQueue.Enqueue(
                        SynStream.PriorityLevel.High,
                        invalidStream);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Frame {frame.GetType().FullName} is unknown");
            }
        }

        public SpdyStream Open(
            SynStream.PriorityLevel priority = SynStream.PriorityLevel.Normal,
            SynStream.Options options = SynStream.Options.None,
            NameValueHeaderBlock? headers = null,
            UInt31 associatedToStreamId = default)
        {
            headers ??= new NameValueHeaderBlock();
            var streamId = InterlockedExtensions.Add(ref _nextStreamId, 2) - 2;

            var stream = SpdyStream.Open(
                Id,
                new SynStream(
                    options, streamId, associatedToStreamId, priority, headers),
                _sendingPriorityQueue);
            _streams.TryAdd(stream.Id, stream);

            return stream;
        }

        public Task<SpdyStream> ReceiveAsync(
            CancellationToken cancellationToken = default)
            => _receivedStreamRequests
                .ReceiveAsync(cancellationToken);

        public async ValueTask DisposeAsync()
        {
            try
            {
                var isClosed = _sessionCancellationTokenSource
                    .IsCancellationRequested;
                if (isClosed == false)
                {
                    await SendAsync(
                            GoAway.Ok(_lastGoodStreamId),
                            SessionCancellationToken)
                        .ConfigureAwait(false);
                }
            }
            finally
            {
                _sendingCancellationTokenSource.Cancel(false);
                _sessionCancellationTokenSource.Cancel(false);
            }

            await Task.WhenAll(
                          _receivingTask,
                          _sendingTask,
                          _messageHandlerTask,
                          _sendPingTask)
                      .ConfigureAwait(false);

            _sendDataGate.Dispose();
            _windowSizeIncreased.Dispose();
            await _networkClient.DisposeAsync()
                                .ConfigureAwait(false);
        }
    }
}