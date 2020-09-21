using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using FluentAssertions;
using Port.Server.IntegrationTests.TestFramework;
using Port.Server.Spdy;
using Port.Server.Spdy.Frames;
using Xunit;
using Xunit.Abstractions;

namespace Port.Server.IntegrationTests.Spdy
{
    public partial class Given_a_connected_spdy_session
    {
        public partial class
            When_opening_a_stream : XUnit2UnitTestSpecificationAsync
        {
            private SynStream _synStream = null!;
            private ISourceBlock<SynStream> _synStreamSubscription = default!;
            private SpdySessionTester _tester = default!;

            public When_opening_a_stream(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override async Task GivenAsync(
                CancellationToken cancellationToken)
            {
                _tester = DisposeAsyncOnTearDown(await SpdySessionTester
                                .ConnectAsync(cancellationToken)
                                .ConfigureAwait(false));
                _synStreamSubscription =
                    _tester.On<SynStream>(cancellationToken);
                _tester.On<GoAway>(cancellationToken);
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                _tester.Session.Open(
                    SynStream.PriorityLevel.High,
                    SynStream.Options.None,
                    new Dictionary<string, string[]>
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
                                  "header1", new[] {"value1"}));
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
            When_sending_data : XUnit2UnitTestSpecificationAsync
        {
            private Data _dataSent = null!;
            private SpdyStream _stream = null!;
            private ISourceBlock<Data> _dataSubscription = null!;

            public When_sending_data(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override async Task GivenAsync(
                CancellationToken cancellationToken)
            {
                var tester = DisposeAsyncOnTearDown(await SpdySessionTester
                                .ConnectAsync(cancellationToken)
                                .ConfigureAwait(false));
                tester.On<SynStream>(cancellationToken);
                tester.On<GoAway>(cancellationToken);
                _dataSubscription = tester.On<Data>(cancellationToken);
                _stream = tester.Session.Open(
                    SynStream.PriorityLevel.High,
                    SynStream.Options.None,
                    new Dictionary<string, string[]>());
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
}