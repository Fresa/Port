using System.Net;
using System.Net.Sockets;

namespace Port.Server
{
    internal interface INetworkServerFactory
    {
        INetworkServer CreateAndStart(
            IPAddress address,
            int port,
            ProtocolType protocolType);
    }
}