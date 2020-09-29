using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using FluentAssertions;
using Port.Server.IntegrationTests.Spdy.Extensions;
using Port.Server.Spdy;
using Port.Server.Spdy.Frames;
using Port.Server.Spdy.Primitives;
using Xunit;
using Xunit.Abstractions;
using ReadResult = System.IO.Pipelines.ReadResult;

namespace Port.Server.IntegrationTests.Spdy
{
    public partial class Given_a_connected_spdy_session
    {
        public partial class
            When_opening_a_stream : SpdySessionTestSpecification
        {
            private SynStream _synStream = null!;
            private ISourceBlock<SynStream> _synStreamSubscription = default!;

            public When_opening_a_stream(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override Task GivenASessionAsync(
                CancellationToken cancellationToken)
            {
                _synStreamSubscription =
                    Server.On<SynStream>(cancellationToken);
                Server.On<GoAway>(cancellationToken);
                return Task.CompletedTask;
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                Session.Open(
                    SynStream.PriorityLevel.High,
                    headers: new Dictionary<string, IReadOnlyList<string>>
                        {{"header1", new[] {"value1"}}});
                _synStream = await _synStreamSubscription
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
            When_sending_data : SpdySessionTestSpecification
        {
            private Data _dataSent = null!;
            private SpdyStream _stream = null!;
            private ISourceBlock<Data> _dataSubscription = null!;

            public When_sending_data(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override Task GivenASessionAsync(
                CancellationToken cancellationToken)
            {
                Server.On<SynStream>(cancellationToken);
                Server.On<GoAway>(cancellationToken);
                _dataSubscription = Server.On<Data>(cancellationToken);
                _stream = Session.Open();
                return Task.CompletedTask;
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                await _stream.SendAsync(Encoding.UTF8.GetBytes("my data"), cancellationToken: cancellationToken)
                       .ConfigureAwait(false);
                _dataSent = await _dataSubscription
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
            When_sending_headers : SpdySessionTestSpecification
        {
            private Headers _headersSent = null!;
            private SpdyStream _stream = null!;
            private ISourceBlock<Headers> _headersSubscription = null!;

            public When_sending_headers(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override Task GivenASessionAsync(
                CancellationToken cancellationToken)
            {
                Server.On<SynStream>(cancellationToken);
                Server.On<GoAway>(cancellationToken);
                _headersSubscription = Server.On<Headers>(cancellationToken);
                _stream = Session.Open();
                return Task.CompletedTask;
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                await _stream.SendHeadersAsync(new Dictionary<string, IReadOnlyList<string>> { { "header1", new[] { "value1", "value2" } } }, cancellationToken: cancellationToken)
                       .ConfigureAwait(false);
                _headersSent = await _headersSubscription
                                   .ReceiveAsync(cancellationToken)
                                   .ConfigureAwait(false);
            }

            [Fact]
            public void It_should_send_headers()
            {
                _headersSent.Values.Should()
                            .HaveCount(1)
                            .And.AllBeEquivalentTo(new
                                KeyValuePair<string, string[]>(
                                    "header1", new[] { "value1", "value2" }));
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
            When_receiving_data : SpdySessionTestSpecification
        {
            private SpdyStream _stream = null!;
            private readonly List<byte> _dataReceived = new List<byte>();
            private ISourceBlock<WindowUpdate> _windowUpdateSubscription = default!;
            private readonly List<WindowUpdate> _flowControlMessages = new List<WindowUpdate>();

            public When_receiving_data(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override async Task GivenASessionAsync(
                CancellationToken cancellationToken)
            {
                var synStreamSubscription = Server.On<SynStream>(cancellationToken);
                Server.On<GoAway>(cancellationToken);
                _windowUpdateSubscription = Server.On<WindowUpdate>(cancellationToken);
                _stream = Session.Open();
                await synStreamSubscription.ReceiveAsync(cancellationToken)
                                           .ConfigureAwait(false);
                await Server.SendAsync(
                                SynReply.Accept(
                                    _stream.Id,
                                    new Dictionary<string, IReadOnlyList<string>>()),
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

                _flowControlMessages.AddRange(await _windowUpdateSubscription
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
            When_accepting_a_stream_multiple_times : SpdySessionTestSpecification
        {
            private SpdyStream _stream = null!;
            private ISourceBlock<RstStream> _rstStreamSubscription = default!;
            private RstStream _rstStream = default!;

            public When_accepting_a_stream_multiple_times(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override async Task GivenASessionAsync(
                CancellationToken cancellationToken)
            {
                var synStreamSubscription = Server.On<SynStream>(cancellationToken);
                _rstStreamSubscription = Server.On<RstStream>(cancellationToken);
                _stream = DisposeOnTearDown(
                    Session.Open());
                await synStreamSubscription.ReceiveAsync(cancellationToken)
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
                _rstStream = await _rstStreamSubscription
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
            When_receiving_a_window_update_that_exceeds_maximum_window_size : SpdySessionTestSpecification
        {
            private SpdyStream _stream = null!;
            private ISourceBlock<RstStream> _rstStreamSubscription = default!;
            private RstStream _rstStream = default!;

            public When_receiving_a_window_update_that_exceeds_maximum_window_size(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override async Task GivenASessionAsync(
                CancellationToken cancellationToken)
            {
                var synStreamSubscription = Server.On<SynStream>(cancellationToken);
                _rstStreamSubscription = Server.On<RstStream>(cancellationToken);
                _stream = DisposeOnTearDown(
                    Session.Open());
                await synStreamSubscription.ReceiveAsync(cancellationToken)
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
                _rstStream = await _rstStreamSubscription
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
            When_receiving_headers : SpdySessionTestSpecification
        {
            private SpdyStream _stream = null!;
            private ISourceBlock<(string, IReadOnlyList<string>)> _headersSubscription = default!;

            public When_receiving_headers(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override async Task GivenASessionAsync(
                CancellationToken cancellationToken)
            {
                var synStreamSubscription = Server.On<SynStream>(cancellationToken);
                Server.On<GoAway>(cancellationToken);
                _stream = DisposeOnTearDown(
                    Session.Open(options: SynStream.Options.Fin));
                await synStreamSubscription.ReceiveAsync(cancellationToken)
                                           .ConfigureAwait(false);
                _headersSubscription = _stream.Headers.Subscribe();

            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                await Server.SendAsync(
                                SynReply.Accept(
                                    _stream.Id,
                                    new Dictionary<string, IReadOnlyList<string>>{
                                    {
                                        "header1", new []{"Value1"}
                                    }}),
                                cancellationToken)
                            .ConfigureAwait(false);
                await Server.SendAsync(
                                new Headers(
                                    _stream.Id,
                                    new Dictionary<string, IReadOnlyList<string>>{
                                    {
                                        "header2", new []{"Value2"}
                                    }}),
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
                           new KeyValuePair<string, string[]>(
                               "header1", new[] { "Value1" }))
                       .And.ContainEquivalentOf(
                           new KeyValuePair<string, string[]>(
                               "header2", new[] { "Value2" }));
            }
        }
    }

    public partial class Given_an_opened_spdy_stream
    {
        public partial class
            When_receiving_ping : SpdySessionTestSpecification
        {
            private SpdyStream _stream = null!;
            private ISourceBlock<Ping> _pingSubscription = default!;
            private Ping _pingReceived = default!;

            public When_receiving_ping(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override async Task GivenASessionAsync(
                CancellationToken cancellationToken)
            {
                var synStreamSubscription = Server.On<SynStream>(cancellationToken);
                _pingSubscription = Server.On<Ping>(cancellationToken);
                _stream = DisposeOnTearDown(
                    Session.Open(options: SynStream.Options.Fin));
                await synStreamSubscription.ReceiveAsync(cancellationToken)
                                           .ConfigureAwait(false);
                await Server.SendAsync(
                                SynReply.Accept(
                                    _stream.Id,
                                    new Dictionary<string, IReadOnlyList<string>>{
                                    {
                                        "header1", new []{"Value1"}
                                    }}),
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
                _pingReceived = await _pingSubscription.ReceiveAsync(cancellationToken);
            }

            [Fact]
            public void It_should_have_sent_a_ping_response_back()
            {
                _pingReceived.Id.Should()
                             .Be(0);
            }
        }
    }

    public partial class Given_an_opened_spdy_stream
    {
        public partial class
            When_receiving_rst : SpdySessionTestSpecification
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
                var synStreamSubscription = Server.On<SynStream>(cancellationToken);
                _stream = DisposeOnTearDown(
                    Session.Open());
                await synStreamSubscription.ReceiveAsync(cancellationToken)
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
            When_receiving_settings : SpdySessionTestSpecification
        {
            private IEnumerable<Settings.Setting> _settingsReceived = new List<Settings.Setting>();
            private ISourceBlock<Settings.Setting> _settingsSubscription = default!;

            public When_receiving_settings(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override async Task GivenASessionAsync(
                CancellationToken cancellationToken)
            {
                _settingsSubscription = Session.Settings.Subscribe();
                await Server.SendAsync(
                                new Settings(
                                        Settings.MaxConcurrentStreams(100),
                                        Settings.UploadBandwidth(1000)),
                                cancellationToken)
                            .ConfigureAwait(false);
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
}