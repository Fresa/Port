using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Spdy.Collections;
using Spdy.Configuration.Metrics;
using Spdy.Extensions;
using Spdy.Frames;
using Spdy.IntegrationTests.Extensions;
using Spdy.IntegrationTests.SocketTestFramework.Collections;
using Spdy.Primitives;
using Xunit;
using Xunit.Abstractions;
using ReadResult = System.IO.Pipelines.ReadResult;

namespace Spdy.IntegrationTests
{
    public partial class Given_a_connected_spdy_session
    {
        public partial class
            When_opening_a_stream : SpdyClientSessionTestSpecification
        {
            private SynStream _synStream = null!;

            public When_opening_a_stream(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                Session.Open(SynStream.PriorityLevel.High,
                    headers: new NameValueHeaderBlock(
                        ("header1", new[] { "value1" })));
                _synStream = await Subscriptions.Get<SynStream>()
                                   .ReceiveAsync(cancellationToken)
                                   .ConfigureAwait(false);
            }

            [Fact]
            public void It_should_send_a_syn_stream_frame_with_priority()
            {
                _synStream.Priority.Should()
                          .Be(SynStream.PriorityLevel.High);
            }

            [Fact]
            public void It_should_send_a_syn_stream_frame_with_headers()
            {
                _synStream.Headers.Should()
                          .HaveCount(1)
                          .And.ContainEquivalentOf(
                              new KeyValuePair<string, string[]>(
                                  "header1", new[] { "value1" }));
            }

            [Fact]
            public void It_should_send_a_syn_stream_frame_with_stream_id()
            {
                _synStream.StreamId.Should()
                          .Be(1u);
            }
        }
    }

    public partial class Given_an_opened_spdy_stream
    {
        public partial class
            When_sending_data : SpdyClientSessionTestSpecification
        {
            private Data _dataSent = null!;
            private SpdyStream _stream = null!;

            public When_sending_data(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override Task GivenASessionAsync(
                CancellationToken cancellationToken)
            {
                _stream = Session.Open();
                return Task.CompletedTask;
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                await _stream.SendAsync(Encoding.UTF8.GetBytes("my data"), cancellationToken: cancellationToken)
                       .ConfigureAwait(false);
                _dataSent = await Subscriptions.Get<Data>()
                                   .ReceiveAsync(cancellationToken)
                                   .ConfigureAwait(false);
            }

            [Fact]
            public void It_should_send_a_data_frame_with_payload()
            {
                _dataSent.Payload.Should().HaveCount(7)
                         .And.ContainInOrder(Encoding.UTF8.GetBytes("my data"));
            }

            [Fact]
            public void It_should_send_a_data_frame_that_is_not_last()
            {
                _dataSent.IsLastFrame.Should().BeFalse();
            }

            [Fact]
            public void It_should_send_a_data_frame_with_stream_id()
            {
                _dataSent.StreamId.Should()
                          .Be(1u);
            }
        }
    }

    public partial class Given_an_opened_spdy_stream
    {
        public partial class
            When_sending_headers : SpdyClientSessionTestSpecification
        {
            private Headers _headersSent = null!;
            private SpdyStream _stream = null!;

            public When_sending_headers(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override Task GivenASessionAsync(
                CancellationToken cancellationToken)
            {
                _stream = Session.Open();
                return Task.CompletedTask;
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                await _stream.SendHeadersAsync(new NameValueHeaderBlock(
                                     ("header1", new[] { "value1", "value2" })),
                                 cancellationToken: cancellationToken)
                       .ConfigureAwait(false);
                _headersSent = await Subscriptions.Get<Headers>()
                                                  .ReceiveAsync(
                                                      cancellationToken)
                                                  .ConfigureAwait(false);
            }

            [Fact]
            public void It_should_send_headers()
            {
                _headersSent.Values.Should()
                            .HaveCount(1)
                            .And.BeEquivalentTo(new NameValueHeaderBlock(
                                    ("header1", new[] { "value1", "value2" })));
            }

            [Fact]
            public void It_should_send_a_header_that_is_not_last()
            {
                _headersSent.IsLastFrame.Should().BeFalse();
            }

            [Fact]
            public void It_should_send_a_header_with_stream_id()
            {
                _headersSent.StreamId.Should()
                          .Be(1u);
            }
        }
    }

    public partial class Given_an_opened_spdy_stream
    {
        public partial class
            When_receiving_data : SpdyClientSessionTestSpecification
        {
            private SpdyStream _stream = null!;
            private readonly List<byte> _dataReceived = new List<byte>();
            private readonly List<WindowUpdate> _flowControlMessages = new List<WindowUpdate>();

            public When_receiving_data(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override async Task GivenASessionAsync(
                CancellationToken cancellationToken)
            {
                _stream = Session.Open();
                await Subscriptions.Get<SynStream>().ReceiveAsync(cancellationToken)
                                           .ConfigureAwait(false);
                await Server.SendAsync(
                                SynReply.Accept(
                                    _stream.Id),
                                cancellationToken)
                            .ConfigureAwait(false);
                await Server.SendAsync(
                                Data.Last(
                                    _stream.Id,
                                    Encoding.UTF8.GetBytes("my data")),
                                cancellationToken)
                            .ConfigureAwait(false);
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                ReadResult data;
                do
                {
                    data = await _stream.ReceiveAsync(cancellationToken: cancellationToken)
                                             .ConfigureAwait(false);

                    _dataReceived.AddRange(data.Buffer.ToArray());
                } while (data.IsCompleted == false);

                _flowControlMessages.AddRange(await Subscriptions.Get<WindowUpdate>()
                                               .ReceiveAsync(2, cancellationToken)
                                               .ConfigureAwait(false));
            }

            [Fact]
            public void It_should_receive_data()
            {
                _dataReceived.Should().HaveCount(7)
                             .And.ContainInOrder(Encoding.UTF8.GetBytes("my data"));
            }

            [Fact]
            public void It_should_send_window_update_for_the_stream()
            {
                _flowControlMessages
                    .Should().ContainEquivalentOf(new WindowUpdate(_stream.Id, 7));
            }

            [Fact]
            public void It_should_send_window_update_for_the_connection()
            {
                _flowControlMessages
                    .Should().ContainEquivalentOf(WindowUpdate.ConnectionFlowControl(7));
            }
        }
    }

    public partial class Given_an_opened_spdy_stream
    {
        public partial class
            When_accepting_a_stream_multiple_times : SpdyClientSessionTestSpecification
        {
            private SpdyStream _stream = null!;
            private RstStream _rstStream = default!;

            public When_accepting_a_stream_multiple_times(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override async Task GivenASessionAsync(
                CancellationToken cancellationToken)
            {
                _stream = DisposeOnTearDown(
                    Session.Open());
                await Subscriptions.Get<SynStream>().ReceiveAsync(cancellationToken)
                                           .ConfigureAwait(false);
                await Server.SendAsync(
                                SynReply.Accept(_stream.Id),
                                cancellationToken)
                            .ConfigureAwait(false);
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                await Server.SendAsync(
                                SynReply.Accept(_stream.Id),
                                cancellationToken)
                            .ConfigureAwait(false);
                _rstStream = await Subscriptions.Get<RstStream>()
                                   .ReceiveAsync(CancellationToken)
                                   .ConfigureAwait(false);
            }

            [Fact]
            public void It_should_send_stream_in_use_error()
            {
                _rstStream.Status.Should()
                          .Be(RstStream.StatusCode.StreamInUse);
            }

            [Fact]
            public void It_should_send_stream_in_use_error_with_stream_id()
            {
                _rstStream.StreamId.Should()
                          .Be(_stream.Id);
            }

            [Fact]
            public void It_should_close_the_local_endpoint()
            {
                _stream.Local.IsClosed.Should()
                       .BeTrue();
            }

            [Fact]
            public void It_should_close_the_remote_endpoint()
            {
                _stream.Remote.IsClosed.Should()
                       .BeTrue();
            }
        }
    }

    public partial class Given_an_opened_spdy_stream
    {
        public partial class
            When_receiving_a_window_update_that_exceeds_maximum_window_size : SpdyClientSessionTestSpecification
        {
            private SpdyStream _stream = null!;
            private RstStream _rstStream = default!;

            public When_receiving_a_window_update_that_exceeds_maximum_window_size(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override async Task GivenASessionAsync(
                CancellationToken cancellationToken)
            {
                _stream = DisposeOnTearDown(
                    Session.Open());
                await Subscriptions.Get<SynStream>().ReceiveAsync(cancellationToken)
                                           .ConfigureAwait(false);
                await Server.SendAsync(
                                SynReply.Accept(_stream.Id),
                                cancellationToken)
                            .ConfigureAwait(false);
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                await Server.SendAsync(
                                new WindowUpdate(_stream.Id, UInt31.MaxValue),
                                cancellationToken)
                            .ConfigureAwait(false);
                _rstStream = await Subscriptions.Get<RstStream>()
                                   .ReceiveAsync(CancellationToken)
                                   .ConfigureAwait(false);
            }

            [Fact]
            public void It_should_send_flow_control_error()
            {
                _rstStream.Status.Should()
                          .Be(RstStream.StatusCode.FlowControlError);
            }

            [Fact]
            public void It_should_send_flow_control_error_with_stream_id()
            {
                _rstStream.StreamId.Should()
                          .Be(_stream.Id);
            }

            [Fact]
            public void It_should_close_the_local_endpoint()
            {
                _stream.Local.IsClosed.Should()
                       .BeTrue();
            }

            [Fact]
            public void It_should_close_the_remote_endpoint()
            {
                _stream.Remote.IsClosed.Should()
                       .BeTrue();
            }
        }
    }

    public partial class Given_an_opened_spdy_stream
    {
        public partial class
            When_receiving_headers : SpdyClientSessionTestSpecification
        {
            private SpdyStream _stream = null!;
            private ISubscription<(string, string[])> _headersSubscription = default!;

            public When_receiving_headers(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override async Task GivenASessionAsync(
                CancellationToken cancellationToken)
            {
                _stream = DisposeOnTearDown(
                    Session.Open(options: SynStream.Options.Fin));
                await Subscriptions.Get<SynStream>().ReceiveAsync(cancellationToken)
                                           .ConfigureAwait(false);
                _headersSubscription = _stream.Headers.Subscribe();

            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                await Server.SendAsync(
                                SynReply.Accept(
                                    _stream.Id,
                                    new NameValueHeaderBlock(
                                        ("header1", new[] { "Value1" })
                                    )),
                                cancellationToken)
                            .ConfigureAwait(false);
                await Server.SendAsync(
                                new Headers(
                                    _stream.Id,
                                    new NameValueHeaderBlock(
                                        ("header2", new[] { "Value2" })
                                    )),
                                cancellationToken)
                            .ConfigureAwait(false);

                await _headersSubscription.ReceiveAsync(2, cancellationToken)
                                          .ConfigureAwait(false);
            }

            [Fact]
            public void It_should_have_headers()
            {
                _stream.Headers.ToList().Should()
                       .HaveCount(2)
                       .And.ContainEquivalentOf(
                           new KeyValuePair<string, string[]>("header1", new[] { "Value1" }))
                       .And.ContainEquivalentOf(
                           new KeyValuePair<string, string[]>("header2", new[] { "Value2" }));
            }
        }
    }

    public partial class Given_an_opened_spdy_stream
    {
        public partial class
            When_receiving_ping : SpdyClientSessionTestSpecification
        {
            private SpdyStream _stream = null!;
            private Ping _pingReceived = default!;

            public When_receiving_ping(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override Configuration.Configuration SpdySessionConfiguration { get; } =
                new(
                    Configuration.Ping.Disabled,
                    Metrics.Default);

            protected override async Task GivenASessionAsync(
                CancellationToken cancellationToken)
            {
                _stream = DisposeOnTearDown(
                    Session.Open(options: SynStream.Options.Fin));
                await Subscriptions.Get<SynStream>()
                                   .ReceiveAsync(cancellationToken)
                                   .ConfigureAwait(false);
                await Server.SendAsync(
                                SynReply.Accept(
                                    _stream.Id,
                                    new NameValueHeaderBlock(
                                        ("header1", new[] { "Value1" })
                                    )),
                                cancellationToken)
                            .ConfigureAwait(false);
                await Server.SendAsync(
                                new Ping(0),
                                cancellationToken)
                            .ConfigureAwait(false);
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                _pingReceived = await Subscriptions.Get<Ping>()
                                                   .ReceiveAsync(
                                                       cancellationToken)
                                                   .ConfigureAwait(false);
            }

            [Fact]
            public void It_should_have_sent_a_ping_response_back()
            {
                _pingReceived.Id.Should()
                             .Be(0);
            }
        }
    }

    public partial class Given_an_spdy_client_session
    {
        public partial class
            When_connecting : SpdyClientSessionTestSpecification
        {
            private Ping _pingReceived = default!;

            public When_connecting(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                _pingReceived = await Subscriptions.Get<Ping>()
                                                   .ReceiveAsync(
                                                       cancellationToken)
                                                   .ConfigureAwait(false);
            }

            [Fact]
            public void It_should_have_sent_a_ping_with_an_odd_id()
            {
                _pingReceived.Id.IsOdd().Should().BeTrue();
            }
        }
    }

    public partial class Given_an_opened_spdy_stream
    {
        public partial class
            When_receiving_rst : SpdyClientSessionTestSpecification
        {
            private SpdyStream _stream = null!;

            public When_receiving_rst(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override async Task GivenASessionAsync(
                CancellationToken cancellationToken)
            {
                _stream = DisposeOnTearDown(
                    Session.Open());
                await Subscriptions.Get<SynStream>().ReceiveAsync(cancellationToken)
                                           .ConfigureAwait(false);
                await Server.SendAsync(
                                SynReply.Accept(_stream.Id), cancellationToken)
                            .ConfigureAwait(false);
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                await Server.SendAsync(
                                RstStream.Cancel(_stream.Id),
                                cancellationToken)
                            .ConfigureAwait(false);
                await _stream.Local.WaitForClosedAsync(cancellationToken)
                             .ConfigureAwait(false);
            }

            [Fact]
            public async Task It_should_not_be_possible_to_receive_more_data()
            {
                (await _stream.ReceiveAsync().ConfigureAwait(false))
                    .IsCanceled.Should()
                    .BeTrue();
            }

            [Fact]
            public async Task It_should_not_be_possible_to_send_more_data()
            {
                (await _stream.SendAsync(
                                  Encoding.UTF8.GetBytes("end"),
                                  cancellationToken: CancellationTokenSource
                                      .Token)
                              .ConfigureAwait(false))
                    .IsCanceled.Should()
                    .BeTrue();
            }
        }
    }

    public partial class Given_an_opened_spdy_session
    {
        public partial class
            When_receiving_settings : SpdyClientSessionTestSpecification
        {
            private IEnumerable<Settings.Setting> _settingsReceived = new List<Settings.Setting>();
            private ISubscription<Settings.Setting> _settingsSubscription = default!;

            public When_receiving_settings(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override Task GivenASessionAsync(
                CancellationToken cancellationToken)
            {
                _settingsSubscription = Session.Settings.Subscribe();
                return Server.SendAsync(
                    new Settings(
                        Settings.MaxConcurrentStreams(100),
                        Settings.UploadBandwidth(1000)),
                    cancellationToken);
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                _settingsReceived = await _settingsSubscription
                                    .ReceiveAsync(2, cancellationToken)
                                    .ConfigureAwait(false);
            }

            [Fact]
            public void It_should_have_received_settings()
            {
                _settingsReceived.Should()
                                 .HaveCount(2)
                                 .And.ContainEquivalentOf(Settings.MaxConcurrentStreams(100))
                                 .And.ContainEquivalentOf(Settings.UploadBandwidth(1000));
            }
        }
    }

    public partial class Given_an_opened_spdy_stream
    {
        public partial class
            When_sending_more_data_than_the_window_allows : SpdyClientSessionTestSpecification
        {
            private SpdyStream _stream = default!;
            private Data _receivedData = default!;
            private FlushResult _sendingResult;

            public When_sending_more_data_than_the_window_allows(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override async Task GivenASessionAsync(
                CancellationToken cancellationToken)
            {
                _stream = Session.Open();
                await Subscriptions.Get<SynStream>()
                                   .ReceiveAsync(cancellationToken)
                                   .ConfigureAwait(false);
                await Server.SendAsync(
                                SynReply.Accept(_stream.Id), cancellationToken)
                            .ConfigureAwait(false);
                await Server.SendAsync(
                    new Settings(Settings.InitialWindowSize(5)),
                    cancellationToken)
                            .ConfigureAwait(false);

                // Need to send something after settings in order to know when settings have been set
                await Server.SendAsync(
                                new Data(
                                    _stream.Id,
                                    Encoding.UTF8.GetBytes("dummy")),
                                cancellationToken)
                            .ConfigureAwait(false);
                await _stream.ReceiveAsync(cancellationToken: cancellationToken)
                       .ConfigureAwait(false);
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                var cancellationTokenSource =
                    CancellationTokenSource.CreateLinkedTokenSource(
                        cancellationToken);
                var sendingTask = _stream.SendLastAsync(
                                 Encoding.UTF8.GetBytes(
                                     "This is more than 5 bytes"),
                                 cancellationToken: cancellationTokenSource.Token)
                             .ConfigureAwait(false);
                _receivedData = await Subscriptions.Get<Data>()
                                                   .ReceiveAsync(
                                                       cancellationToken)
                                                   .ConfigureAwait(false);
                cancellationTokenSource.Cancel();
                _sendingResult = await sendingTask;
            }

            [Fact]
            public void It_should_have_received_the_amount_of_data_described_by_the_window_size()
            {
                _receivedData.Payload.Should()
                             .HaveCount(5);
            }

            [Fact]
            public void It_should_have_received_data_that_is_not_the_last()
            {
                _receivedData.IsLastFrame.Should().BeFalse();
            }

            [Fact]
            public void It_should_have_cancelled_sending_the_remaining_data()
            {
                _sendingResult.IsCanceled.Should()
                              .BeTrue();
            }

            [Fact]
            public void It_should_not_have_completed_sending_data()
            {
                _sendingResult.IsCompleted.Should()
                              .BeFalse();
            }
        }
    }

    public partial class Given_a_unidirectional_spdy_session
    {
        public partial class
            When_receiving_data : SpdyClientSessionTestSpecification
        {
            private SpdyStream _stream = default!;
            private RstStream _rst = default!;
            private WindowUpdate _windowUpdate = default!;

            public When_receiving_data(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override async Task GivenASessionAsync(
                CancellationToken cancellationToken)
            {
                _stream = DisposeOnTearDown(
                    Session.Open(options: SynStream.Options.Unidirectional));
                await Subscriptions.Get<SynStream>()
                                   .ReceiveAsync(cancellationToken)
                                   .ConfigureAwait(false);
                await Server.SendAsync(
                                SynReply.Accept(_stream.Id), cancellationToken)
                            .ConfigureAwait(false);
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                await Server.SendAsync(Data.Last(_stream.Id, Encoding.UTF8.GetBytes("data")),
                                cancellationToken)
                            .ConfigureAwait(false);
                _rst = await Subscriptions.Get<RstStream>()
                                          .ReceiveAsync(cancellationToken)
                                          .ConfigureAwait(false);
                _windowUpdate =
                    await Subscriptions.Get<WindowUpdate>()
                                       .ReceiveAsync(cancellationToken)
                                       .ConfigureAwait(false);
            }

            [Fact]
            public void It_should_have_sent_a_rst_for_the_stream()
            {
                _rst.StreamId.Should()
                    .Be(_stream.Id);
            }

            [Fact]
            public void It_should_have_sent_a_stream_already_closed_error()
            {
                _rst.Status.Should()
                    .Be(RstStream.StatusCode.StreamAlreadyClosed);
            }

            [Fact]
            public void It_should_have_sent_a_connection_window_update()
            {
                _windowUpdate.StreamId.Should()
                             .Be((uint)0);
            }

            [Fact]
            public void It_should_have_sent_a_connection_window_update_with_the_size_of_the_received_data()
            {
                _windowUpdate.DeltaWindowSize.Should()
                             .Be((uint)4);
            }
        }
    }

    public partial class Given_a_closed_spdy_session
    {
        public partial class
            When_receiving_data : SpdyClientSessionTestSpecification
        {
            private SpdyStream _stream = default!;
            private RstStream _rst = default!;
            private WindowUpdate _windowUpdate = default!;

            public When_receiving_data(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override async Task GivenASessionAsync(
                CancellationToken cancellationToken)
            {
                _stream = DisposeOnTearDown(
                    Session.Open(options: SynStream.Options.Unidirectional | SynStream.Options.Fin));
                await Subscriptions.Get<SynStream>().ReceiveAsync(cancellationToken)
                                           .ConfigureAwait(false);
                await Server.SendAsync(
                                SynReply.Accept(_stream.Id), cancellationToken)
                            .ConfigureAwait(false);
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                await Server.SendAsync(Data.Last(_stream.Id, Encoding.UTF8.GetBytes("data")),
                                cancellationToken)
                            .ConfigureAwait(false);
                _rst = await Subscriptions.Get<RstStream>().ReceiveAsync(cancellationToken)
                                     .ConfigureAwait(false);
                _windowUpdate =
                    await Subscriptions.Get<WindowUpdate>()
                          .ReceiveAsync(cancellationToken)
                          .ConfigureAwait(false);
            }

            [Fact]
            public void It_should_have_sent_a_rst_for_the_stream()
            {
                _rst.StreamId.Should()
                    .Be(_stream.Id);
            }

            [Fact]
            public void It_should_have_sent_a_protocol_error()
            {
                _rst.Status.Should()
                    .Be(RstStream.StatusCode.ProtocolError);
            }

            [Fact]
            public void It_should_have_sent_a_connection_window_update()
            {
                _windowUpdate.StreamId.Should()
                             .Be((uint)0);
            }

            [Fact]
            public void It_should_have_sent_a_connection_window_update_with_the_size_of_the_received_data()
            {
                _windowUpdate.DeltaWindowSize.Should()
                             .Be((uint)4);
            }
        }
    }
}