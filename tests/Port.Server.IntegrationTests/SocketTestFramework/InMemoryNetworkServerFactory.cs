using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks.Dataflow;

namespace Port.Server.IntegrationTests.SocketTestFramework
{
    internal sealed class InMemoryNetworkServerFactory : INetworkServerFactory
    {
        private readonly Dictionary<string, BufferBlock<INetworkClient>>
            _networkServers
                = new Dictionary<string, BufferBlock<INetworkClient>>();

        internal BufferBlock<INetworkClient> Get(
            IPAddress address,
            int port,
            ProtocolType protocolType)
        {
            return _networkServers[GetSocketId(address, port, protocolType)];
        }

        private static string GetSocketId(
            IPAddress address,
            int port,
            ProtocolType protocolType)
            => $"{address}:{port}-{protocolType}";

        public INetworkServer CreateAndStart(
            IPAddress address,
            int port,
            ProtocolType protocolType)
        {
            var clients = new BufferBlock<INetworkClient>();
            _networkServers.Add(GetSocketId(address, port, protocolType), clients);
            return InMemoryServer.Start(clients);
        }
    }
}