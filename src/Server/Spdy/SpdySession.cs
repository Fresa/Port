using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Log.It;
using Port.Server.Spdy.Collections;
using Port.Server.Spdy.Extensions;
using Port.Server.Spdy.Frames;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    /// <summary>
    /// Client implementation
    /// </summary>
    public class SpdySession : IAsyncDisposable
    {
        private readonly ILogger _logger = LogFactory.Create<SpdySession>();
        private readonly INetworkClient _networkClient;

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

        private int _streamCounter = -1;
        private UInt31 _lastGoodRepliedStreamId;

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

        internal SpdySession(
            INetworkClient networkClient)
        {
            _sendingCancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(
                    SessionCancellationToken);

            _networkClient = networkClient;

            _sendingTask = StartBackgroundTask(SendFramesAsync);
            _receivingTask = StartBackgroundTask(ReceiveFromNetworkClientAsync);
            _messageHandlerTask = StartBackgroundTask(HandleMessagesAsync);
        }

        private Task StartBackgroundTask(
            Func<Task> action)
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
                    catch when (_sessionCancellationTokenSource
                        .IsCancellationRequested)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.Fatal(ex, "Unknown error, closing down");
                        await StopNetworkSender()
                            .ConfigureAwait(false);
                        try
                        {
                            await SendAsync(
                                    GoAway.InternalError(
                                        UInt31.From(_lastGoodRepliedStreamId)),
                                    SendingCancellationToken)
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
            while (_sessionCancellationTokenSource
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

        private async Task StopNetworkSender()
        {
            _sendingCancellationTokenSource.Cancel();
            await _sendingTask.ConfigureAwait(false);
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
                    while (Interlocked.Add(
                               ref _windowSize, -data.Payload.Length) <
                           0)
                    {
                        Interlocked.Add(ref _windowSize, data.Payload.Length);
                        await _windowSizeGate
                              .WaitAsync(SessionCancellationToken)
                              .ConfigureAwait(false);
                    }
                }
            }

            await SendAsync((Frame) data, cancellationToken)
                .ConfigureAwait(false);
        }

        private readonly SemaphoreSlim
            _windowSizeGate = new SemaphoreSlim(1, 1);

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
                _windowSizeGate.Release();
            }
        }

        private long _pingId = -1;

        private void EnqueuePing()
        {
            var id = Interlocked.Add(ref _pingId, 2);
            if (id > uint.MaxValue)
            {
                Interlocked.Exchange(ref _pingId, 1);
                id = 1;
            }

            var ping = new Ping((uint) id);
            _sendingPriorityQueue.Enqueue(SynStream.PriorityLevel.Top, ping);
        }

        private async Task<(bool Found, SpdyStream Stream)>
            TryGetStreamOrCloseSession(
                UInt31 streamId)
        {
            if (_streams.TryGetValue(
                streamId,
                out var stream))
            {
                return (true, stream);
            }

            await StopNetworkSender()
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
                var bytes = await _networkClient.ReceiveAsync(
                    _messageReceiver.Writer.GetMemory(),
                    SessionCancellationToken);

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
                    throw new NotImplementedException();
                case SynReply synReply:
                    (found, stream) =
                        await TryGetStreamOrCloseSession(synReply.StreamId)
                            .ConfigureAwait(false);
                    if (found == false)
                    {
                        return;
                    }

                    stream.Receive(frame);
                    break;
                case RstStream rstStream:
                    (found, stream) =
                        await TryGetStreamOrCloseSession(rstStream.StreamId)
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
                        await TryGetStreamOrCloseSession(headers.StreamId)
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
                        await TryGetStreamOrCloseSession(windowUpdate.StreamId)
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
                                    (uint) data.Payload.Length));
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
            var streamId = (uint) Interlocked.Add(ref _streamCounter, 2);

            var stream = new SpdyStream(
                UInt31.From(streamId), priority, _sendingPriorityQueue);
            _streams.TryAdd(stream.Id, stream);

            stream.Open(options, headers);
            return stream;
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
            await _networkClient.DisposeAsync()
                                .ConfigureAwait(false);
        }
    }
}