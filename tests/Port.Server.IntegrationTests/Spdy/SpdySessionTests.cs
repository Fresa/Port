using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
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
            private ISendingClient<Frame> _client = null!;
            private SynStream _synStream = null!;

            private readonly SemaphoreSlim _frameReceived =
                new SemaphoreSlim(0, 1);

            public When_opening_a_stream(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override async Task GivenAsync(
                CancellationToken cancellationToken)
            {
                var server = _testFramework.NetworkServerFactory.CreateAndStart(
                    IPAddress.Any, 1,
                    ProtocolType.Tcp);
                _testFramework.On<SynStream>(
                    stream =>
                    {
                        _synStream = stream;
                        _frameReceived.Release();
                    });
                var clientTask = _testFramework.ConnectAsync(
                                                   new FrameClientFactory(),
                                                   IPAddress.Any, 1,
                                                   ProtocolType.Tcp,
                                                   cancellationToken)
                                               .ConfigureAwait(false);

                _session = new SpdySession(
                    await server.WaitForConnectedClientAsync(
                        cancellationToken));
                _client = await clientTask;
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                _session.Open(
                    SynStream.PriorityLevel.High, SynStream.Options.None,
                    new Dictionary<string, string[]>
                        {{"header1", new[] {"value1"}}});
                await _frameReceived.WaitAsync(cancellationToken)
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