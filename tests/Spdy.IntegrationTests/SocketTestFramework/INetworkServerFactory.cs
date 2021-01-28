using System.Net;
using System.Net.Sockets;

namespace Spdy.IntegrationTests.SocketTestFramework
{
    internal interface INetworkServerFactory
    {
        INetworkServer CreateAndStart(
            IPAddress address,
            int port,
            ProtocolType protocolType);
    }
}