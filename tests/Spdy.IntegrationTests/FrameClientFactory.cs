using Spdy.Frames;
using Spdy.IntegrationTests.SocketTestFramework;
using Spdy.Network;

namespace Spdy.IntegrationTests
{
    internal sealed class FrameClientFactory : IMessageClientFactory<Frame>
    {
        public IMessageClient<Frame> Create(
            INetworkClient networkClient)
            => new FrameClient(networkClient);
    }
}