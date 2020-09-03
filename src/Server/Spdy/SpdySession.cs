using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper.Configuration.Annotations;
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

        private readonly ConcurrentPriorityQueue<Frame> _sendingPriorityQueue =
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
                                        UInt31.From((uint) _streamCounter)),
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
            await foreach (var bufferSequence in frame
                                                 .WriteAsync(cancellationToken)
                                                 .ConfigureAwait(false))
            {
                foreach (var buffer in bufferSequence)
                {
                    await _networkClient.SendAsync(
                                            buffer,
                                            cancellationToken)
                                        .ConfigureAwait(false);
                }
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
            await Send(GoAway.ProtocolError(streamId), SessionCancellationToken)
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

                            if ((await Frame.TryReadAsync(
                                                frameReader,
                                                SessionCancellationToken)
                                            .ConfigureAwait(false)).Out(
                                out var frame, out var error) == false)
                            {
                                await Send(error, SessionCancellationToken);
                                continue;
                            }
                            bool found;
                            SpdyStream? stream;
                            switch (frame)
                            {
                                case SynStream synStream:
                                    throw new NotImplementedException();
                                case SynReply synReply:
                                    (found, stream) =
                                        await TryGetStreamOrCloseSession(
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
                                        await TryGetStreamOrCloseSession(
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

                                    // Pong
                                    _sendingPriorityQueue.Enqueue(
                                        SynStream.PriorityLevel.Top, ping);
                                    break;
                                case Data data:
                                    if (_streams.TryGetValue(
                                        data.StreamId,
                                        out stream))
                                    {
                                        stream.Receive(data);
                                    }

                                    // If an endpoint receives a data frame for a stream-id which is not open and the endpoint has not sent a GOAWAY (Section 2.6.6) frame, it MUST issue a stream error (Section 2.4.2) with the error code INVALID_STREAM for the stream-id.
                                    _sendingPriorityQueue.Enqueue(
                                        SynStream.PriorityLevel.High,
                                        RstStream.InvalidStream(data.StreamId));
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
                        try
                        {
                            await Send(
                                    GoAway.InternalError(
                                        UInt31.From((uint) _streamCounter)),
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

        public SpdyStream Open(
            SynStream.PriorityLevel priority)
        {
            var streamId = (uint) Interlocked.Increment(ref _streamCounter);

            var stream = new SpdyStream(
                UInt31.From(streamId), priority, _sendingPriorityQueue);
            _streams.TryAdd(stream.Id, stream);

            stream.Open();
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

            await Send(
                    GoAway.Ok(UInt31.From((uint) _streamCounter)),
                    CancellationToken.None)
                .ConfigureAwait(false);

            await _networkClient.DisposeAsync()
                                .ConfigureAwait(false);
        }
    }
}