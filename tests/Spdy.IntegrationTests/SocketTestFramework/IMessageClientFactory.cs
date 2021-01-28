using Spdy.Network;

namespace Spdy.IntegrationTests.SocketTestFramework
{
    internal interface IMessageClientFactory<T>
    {
        IMessageClient<T> Create(
            INetworkClient networkClient);
    }
}