using System.Net;
using System.Net.Sockets;

namespace Port.Server
{
    internal sealed class SocketNetworkServerFactory : INetworkServerFactory
    {
        public INetworkServer CreateAndStart(
            IPAddress address,
            int port ,
            ProtocolType protocolType)
        {
            return SocketServer.Start(
                address,
                port,
                protocolType);
        }
    }
}