using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Port.Server.IntegrationTests.SocketTestFramework;
using Port.Server.Spdy;
using Port.Server.Spdy.Frames;

namespace Port.Server.IntegrationTests.Spdy
{
    public sealed class SpdyTestServer : IAsyncDisposable
    {
        private readonly InMemorySocketTestFramework _testFramework =
            SocketTestFramework.SocketTestFramework.InMemory();

        private ISendingClient<Frame> _client = default!;
        private INetworkServer _server = default!;

        internal async Task<SpdySession> ConnectAsync(
            CancellationToken cancellationToken = default)
        {
            _server =
                _testFramework.NetworkServerFactory.CreateAndStart(
                    IPAddress.Any, 1,
                    ProtocolType.Tcp);
            _client = await _testFramework.ConnectAsync(
                                              new FrameClientFactory(),
                                              IPAddress.Any, 1,
                                              ProtocolType.Tcp,
                                              cancellationToken)
                                          .ConfigureAwait(false);
        
            return new SpdySession(await _server.WaitForConnectedClientAsync(cancellationToken));
        }

        internal async Task SendAsync(
            Frame frame,
            CancellationToken cancellationToken = default)
        {
            await _client.SendAsync(frame, cancellationToken)
                         .ConfigureAwait(false);
        }

        internal ISourceBlock<T> On<T>(
            CancellationToken cancellationToken = default)
            where T : Frame
            => _testFramework.On<T>(cancellationToken);

        public async ValueTask DisposeAsync()
        {
            await _testFramework.DisposeAsync()
                                .ConfigureAwait(false);
            await _server.DisposeAsync()
                         .ConfigureAwait(false);
        }
    }
}