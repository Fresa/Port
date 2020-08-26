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

        private readonly ConcurrentPriorityQueue<byte[]> _sendingPriorityQueue =
            new ConcurrentPriorityQueue<byte[]>();

        private readonly ConcurrentPriorityQueue<Frame> _receivingPriorityQueue
            =
            new ConcurrentPriorityQueue<Frame>();

        private int _streamCounter;

        private readonly ConcurrentDictionary<UInt31, SpdyStream> _streams =
            new ConcurrentDictionary<UInt31, SpdyStream>();

        private Dictionary<Settings.Id, Settings.Setting> _settings =
            new Dictionary<Settings.Id, Settings.Setting>();

        internal SpdySession(
            INetworkClient networkClient)
        {
            _sendingCancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(
                    SessionCancellationToken);

            _networkClient = networkClient;
            RunNetworkSender();
            RunNetworkReceiver();
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
                            await _networkClient.SendAsync(
                                                    new ReadOnlyMemory<byte>(
                                                        frame),
                                                    SendingCancellationToken)
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
            Frame frame)
        {
            await foreach (var bufferSequence in frame
                                                 .WriteAsync(
                                                     SessionCancellationToken)
                                                 .ConfigureAwait(false))
            {
                foreach (var buffer in bufferSequence)
                {
                    await _networkClient.SendAsync(
                                            buffer,
                                            SessionCancellationToken)
                                        .ConfigureAwait(false);
                }
            }
        }

        private long _pingId = -1;

        // ReSharper disable once SuggestBaseTypeForParameter Explicitly enqueue pings
        private async Task EnqueuePongAsync(
            Ping ping)
        {
            await ping.WriteToAsync(
                          _sendingPriorityQueue, SynStream.PriorityLevel.Top,
                          SessionCancellationToken)
                      .ConfigureAwait(false);
        }

        private async Task EnqueuePingAsync()
        {
            var id = Interlocked.Add(ref _pingId, 2);
            if (id > uint.MaxValue)
            {
                Interlocked.Exchange(ref _pingId, 1);
                id = 1;
            }

            var ping = new Ping((uint)id);
            await ping.WriteToAsync(
                          _sendingPriorityQueue, SynStream.PriorityLevel.Top,
                          SessionCancellationToken)
                      .ConfigureAwait(false);
        }

        private async Task<(bool Found, SpdyStream Stream)>
            TryGetStreamOrSendGoAway(
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
            await Send(GoAway.ProtocolError(streamId))
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
                    var pipe = new Pipe();

                    var frameReader = new FrameReader(pipe.Reader);
                    try
                    {
                        while (_sessionCancellationTokenSource
                                   .IsCancellationRequested ==
                               false)
                        {
                            await _networkClient.ReceiveAsync(
                                pipe.Writer.GetMemory(),
                                SessionCancellationToken);

                            var frame = await Frame.ReadAsync(
                                                       frameReader,
                                                       SessionCancellationToken)
                                                   .ConfigureAwait(false);
                            bool found;
                            SpdyStream? stream;
                            switch (frame)
                            {
                                case SynStream synStream:
                                    throw new NotImplementedException();
                                case SynReply synReply:
                                    (found, stream) =
                                        await TryGetStreamOrSendGoAway(
                                                synReply.StreamId)
                                            .ConfigureAwait(false);
                                    if (found == false)
                                    {
                                        return;
                                    }

                                    stream.Receive(frame);
                                    break;
                                case RstStream rstStream:
                                    (found, stream) =
                                        await TryGetStreamOrSendGoAway(
                                                rstStream.StreamId)
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
                                                Settings.ValueOptions.Persisted,
                                                setting.Value);
                                    }

                                    break;
                                case Ping ping:
                                    // If a client receives an odd numbered PING which it did not initiate, it must ignore the PING.
                                    if (ping.Id % 2 != 0)
                                    {
                                        break;
                                    }

                                    await EnqueuePongAsync(ping)
                                        .ConfigureAwait(false);
                                    break;
                            }
                        }
                    }
                    catch when (_sessionCancellationTokenSource
                        .IsCancellationRequested)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.Fatal(ex, "Unknown error, closing down");
                        _sessionCancellationTokenSource.Cancel(false);
                    }
                });
        }

        public async Task<SpdyStream> Open(
            SynStream.PriorityLevel priority,
            CancellationToken cancellationToken = default)
        {
            var streamId = (uint)Interlocked.Increment(ref _streamCounter);

            var stream = new SpdyStream(
                UInt31.From(streamId), priority, _sendingPriorityQueue);
            _streams.TryAdd(stream.Id, stream);

            await stream.Open(cancellationToken)
                        .ConfigureAwait(false);
            return stream;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                _sessionCancellationTokenSource.Cancel(false);
            }
            catch
            {
                // Try cancel
            }

            await Task.WhenAll(_receivingTask, _sendingTask)
                      .ConfigureAwait(false);

            await Send(GoAway.Ok(UInt31.From((uint) _streamCounter)))
                .ConfigureAwait(false);

            await _networkClient.DisposeAsync()
                                .ConfigureAwait(false);
        }
    }
}