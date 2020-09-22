using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using FluentAssertions;
using Port.Server.Spdy;
using Port.Server.Spdy.Frames;
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
                    headers: new Dictionary<string, string[]>
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
                await _stream.SendHeadersAsync(new Dictionary<string, string[]> { { "header1", new[] { "value1", "value2" } } }, cancellationToken: cancellationToken)
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
                _stream = Session.Open();
                await synStreamSubscription.ReceiveAsync(cancellationToken)
                                           .ConfigureAwait(false);
                await Server.SendAsync(
                                SynReply.Accept(
                                    _stream.Id,
                                    new Dictionary<string, string[]>()),
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
                } while (data.IsCanceled == false &&
                         data.IsCompleted == false);
            }

            [Fact]
            public void It_should_receive_data()
            {
                _dataReceived.Should().HaveCount(7)
                             .And.ContainInOrder(Encoding.UTF8.GetBytes("my data"));
            }
        }
    }

    public partial class Given_an_opened_spdy_stream
    {
        public partial class
            When_receiving_headers : SpdySessionTestSpecification
        {
            private SpdyStream _stream = null!;
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
                await Server.SendAsync(
                                SynReply.Accept(
                                    _stream.Id,
                                    new Dictionary<string, string[]>{
                                    {
                                        "header1", new []{"Value1"}
                                    }}),
                                cancellationToken)
                            .ConfigureAwait(false);
                await Server.SendAsync(
                                new Headers(
                                    _stream.Id,
                                    new Dictionary<string, string[]>{
                                    {
                                        "header2", new []{"Value2"}
                                    }}),
                                cancellationToken)
                            .ConfigureAwait(false);
                await Server.SendAsync(
                                Data.Last(
                                    _stream.Id,
                                    Encoding.UTF8.GetBytes("end")),
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
                } while (data.IsCanceled == false &&
                         data.IsCompleted == false);
            }

            [Fact]
            public void It_should_have_headers()
            {
                _stream.Headers.Should()
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
                                    new Dictionary<string, string[]>{
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
}