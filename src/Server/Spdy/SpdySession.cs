using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Log.It;
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
        private readonly Pipe _messageReceiver = new Pipe();

        private readonly CancellationTokenSource
            _sendingCancellationTokenSource;

        private CancellationToken SendingCancellationToken
            => _sendingCancellationTokenSource.Token;

        private readonly CancellationTokenSource
            _sessionCancellationTokenSource = new CancellationTokenSource();

        private CancellationToken SessionCancellationToken
            => _sessionCancellationTokenSource.Token;

        private Task _sendingTask = Task.CompletedTask;
        private Task _receivingTask = Task.CompletedTask;
        private Task _messageHandlerTask = Task.CompletedTask;

        private readonly ConcurrentPriorityQueue<Frame> _sendingPriorityQueue =
            new ConcurrentPriorityQueue<Frame>();

        private int _streamCounter = -1;
        private UInt31 _lastGoodRepliedStreamId;

        private readonly ConcurrentDictionary<UInt31, SpdyStream> _streams =
            new ConcurrentDictionary<UInt31, SpdyStream>();

        private readonly Dictionary<Settings.Id, Settings.Setting> _settings =
            new Dictionary<Settings.Id, Settings.Setting>();

        public IReadOnlyCollection<Settings.Setting> Settings
            => _settings.Values;

        private const int InitialWindowSize = 64000;
        private int _windowSize = InitialWindowSize;

        internal SpdySession(
            INetworkClient networkClient)
        {
            _sendingCancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(
                    SessionCancellationToken);

            _networkClient = networkClient;
            RunNetworkSender();
            RunNetworkReceiver();
            RunMessageHandler();
        }

        private void RunNetworkSender()
        {
            // ReSharper disable once MethodSupportsCancellation
            // Will gracefully handle cancellation
            _sendingTask = Task.Run(
                async () =>
                {
                    try
                    {
                        while (_sessionCancellationTokenSource
                            .IsCancellationRequested == false)
                        {
                            var frame = await _sendingPriorityQueue
                                              .DequeueAsync(
                                                  SendingCancellationToken)
                                              .ConfigureAwait(false);

                            if (frame is Data data)
                            {
                                // todo: Should we always wait for data frames to be sent before sending the next frame (which might be a control frame that are not under flow control)?
                                await Send(data, SendingCancellationToken)
                                    .ConfigureAwait(false);
                                continue;
                            }

                            await Send(frame, SendingCancellationToken)
                                .ConfigureAwait(false);
                        }
                    }
                    catch when (_sessionCancellationTokenSource
                        .IsCancellationRequested)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.Fatal(ex, "Unknown error, closing down");
                        try
                        {
                            await Send(
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

        private async Task StopNetworkSender()
        {
            _sendingCancellationTokenSource.Cancel();
            await _sendingTask.ConfigureAwait(false);
        }

        private async Task Send(
            Frame frame,
            CancellationToken cancellationToken)
        {
            await _networkClient.SendAsync(frame, cancellationToken)
                                .ConfigureAwait(false);
        }

        private async Task Send(
            Data data,
            CancellationToken cancellationToken)
        {
            using var gate = SemaphoreSlimGate.OneAtATime;
            {
                while (Interlocked.Add(ref _windowSize, -data.Payload.Length) <
                       0)
                {
                    Interlocked.Add(ref _windowSize, data.Payload.Length);
                    await _windowSizeGate.WaitAsync(SessionCancellationToken)
                                         .ConfigureAwait(false);
                }
            }

            await Send((Frame) data, cancellationToken)
                .ConfigureAwait(false);
        }

        private readonly SemaphoreSlim
            _windowSizeGate = new SemaphoreSlim(1, 1);

        private async Task<bool> TryIncreaseWindowSizeOrCloseSessionAsync(
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
                await Send(
                        RstStream.FlowControlError(_lastGoodRepliedStreamId),
                        SessionCancellationToken)
                    .ConfigureAwait(false);

                _sessionCancellationTokenSource.Cancel(false);
                return false;
            }

            if (newWindowSize > 0)
            {
                _windowSizeGate.Release();
            }

            return true;
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
            await Send(
                    GoAway.ProtocolError(_lastGoodRepliedStreamId),
                    SessionCancellationToken)
                .ConfigureAwait(false);

            _sessionCancellationTokenSource.Cancel(false);
            return (false, stream)!;
        }


        private void RunNetworkReceiver()
        {
            // ReSharper disable once MethodSupportsCancellation
            // Will gracefully handle cancellation
            _receivingTask = Task.Run(
                async () =>
                {
                    try
                    {
                        FlushResult result;
                        do
                        {
                            var bytes = await _networkClient.ReceiveAsync(
                                _messageReceiver.Writer.GetMemory(),
                                SessionCancellationToken);

                            _messageReceiver.Writer.Advance(bytes);
                            result = await _messageReceiver
                                           .Writer.FlushAsync(
                                               SessionCancellationToken)
                                           .ConfigureAwait(false);
                        } while (_sessionCancellationTokenSource
                                     .IsCancellationRequested ==
                                 false &&
                                 result.IsCanceled == false &&
                                 result.IsCompleted == false);
                    }
                    catch when (_sessionCancellationTokenSource
                        .IsCancellationRequested)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.Fatal(ex, "Unknown error, closing down");
                        try
                        {
                            await Send(
                                    GoAway.InternalError(
                                        _lastGoodRepliedStreamId),
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

        private void RunMessageHandler()
        {
            // ReSharper disable once MethodSupportsCancellation
            // Will gracefully handle cancellation
            _messageHandlerTask = Task.Run(
                async () =>
                {
                    var frameReader = new FrameReader(_messageReceiver.Reader);
                    try
                    {
                        while (_sessionCancellationTokenSource
                                   .IsCancellationRequested ==
                               false)
                        {
                            await HandleMessage(frameReader)
                                .ConfigureAwait(false);
                        }
                    }
                    catch when (_sessionCancellationTokenSource
                        .IsCancellationRequested)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.Fatal(ex, "Unknown error, closing down");
                        try
                        {
                            await Send(
                                    GoAway.InternalError(
                                        _lastGoodRepliedStreamId),
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

        private async Task HandleMessage(
            IFrameReader frameReader)
        {
            if ((await Frame.TryReadAsync(
                                frameReader,
                                SessionCancellationToken)
                            .ConfigureAwait(false)).Out(
                out var frame, out var error) == false)
            {
                await Send(error, SessionCancellationToken);
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
                                Frames.Settings.ValueOptions.Persisted,
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
                    if (await TryIncreaseWindowSizeOrCloseSessionAsync(
                            windowUpdate.DeltaWindowSize)
                        .ConfigureAwait(false) == false)
                    {
                        return;
                    }

                    if (windowUpdate
                        .IsConnectionFlowControl)
                    {
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
                        break;
                    }

                    // If an endpoint receives a data frame for a stream-id which is not open and the endpoint has not sent a GOAWAY (Section 2.6.6) frame, it MUST issue a stream error (Section 2.4.2) with the error code INVALID_STREAM for the stream-id.
                    _sendingPriorityQueue.Enqueue(
                        SynStream.PriorityLevel.High,
                        RstStream.InvalidStream(data.StreamId));
                    break;
            }
        }

        public SpdyStream Open(
            SynStream.PriorityLevel priority = SynStream.PriorityLevel.Normal,
            SynStream.Options options = SynStream.Options.None,
            IReadOnlyDictionary<string, string[]>? headers = null)
        {
            headers ??= new Dictionary<string, string[]>();
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
                await Send(
                        GoAway.Ok(UInt31.From(_lastGoodRepliedStreamId)),
                        CancellationToken.None)
                    .ConfigureAwait(false);
            }

            await _networkClient.DisposeAsync()
                                .ConfigureAwait(false);
        }
    }
}