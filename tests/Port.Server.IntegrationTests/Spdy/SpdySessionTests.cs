using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using FluentAssertions;
using Port.Server.IntegrationTests.TestFramework;
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
            public void It_should_receive_a_syn_stream_frame_with_priority()
            {
                _synStream.Priority.Should()
                          .Be(SynStream.PriorityLevel.High);
            }

            [Fact]
            public void It_should_receive_a_syn_stream_frame_with_headers()
            {
                _synStream.Headers.Should()
                          .HaveCount(1)
                          .And.ContainEquivalentOf(
                              new KeyValuePair<string, string[]>(
                                  "header1", new[] {"value1"}));
            }

            [Fact]
            public void It_should_receive_a_syn_stream_frame_with_stream_id()
            {
                _synStream.StreamId.Should()
                          .Be(1u);
            }
        }
    }
}