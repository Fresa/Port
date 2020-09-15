using Port.Server.IntegrationTests.SocketTestFramework;
using Port.Server.Spdy.Frames;

namespace Port.Server.IntegrationTests.Spdy
{
    internal sealed class FrameClientFactory : IMessageClientFactory<Frame>
    {
        public IMessageClient<Frame> Create(
            INetworkClient networkClient)
            => new FrameClient(networkClient);
    }
}