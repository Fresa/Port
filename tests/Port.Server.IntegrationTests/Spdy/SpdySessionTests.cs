using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using FluentAssertions;
using Port.Server.IntegrationTests.SocketTestFramework;
using Port.Server.IntegrationTests.TestFramework;
using Port.Server.Spdy;
using Port.Server.Spdy.Frames;
using Port.Server.Spdy.Primitives;
using Xunit;
using Xunit.Abstractions;

namespace Port.Server.IntegrationTests.Spdy
{
    public partial class Given_a_spdy_session
    {
        public partial class
            When_opening_a_stream : XUnit2UnitTestSpecificationAsync
        {
            private readonly InMemorySocketTestFramework _testFramework =
                SocketTestFramework.SocketTestFramework.InMemory();

            private SpdySession _session = null!;
            private SynStream _synStream = null!;

            private BufferBlock<SynStream> _messages = default!;

            public When_opening_a_stream(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override async Task GivenAsync(
                CancellationToken cancellationToken)
            {
                var server = DisposeAsyncOnTearDown(
                    _testFramework.NetworkServerFactory.CreateAndStart(
                        IPAddress.Any, 1,
                        ProtocolType.Tcp));
                _messages = _testFramework.On<SynStream>(cancellationToken);
                await _testFramework.ConnectAsync(
                                        new FrameClientFactory(),
                                        IPAddress.Any, 1,
                                        ProtocolType.Tcp,
                                        cancellationToken)
                                    .ConfigureAwait(false);

                _session = new SpdySession(
                    await server.WaitForConnectedClientAsync(
                        cancellationToken));
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                _session.Open(
                    SynStream.PriorityLevel.High, SynStream.Options.None,
                    new Dictionary<string, string[]>
                        {{"header1", new[] {"value1"}}});
                _synStream = await _messages.ReceiveAsync(cancellationToken)
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