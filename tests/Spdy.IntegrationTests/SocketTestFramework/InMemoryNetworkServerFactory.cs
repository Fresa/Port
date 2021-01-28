using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks.Dataflow;
using Spdy.Network;

namespace Spdy.IntegrationTests.SocketTestFramework
{
    internal sealed class InMemoryNetworkServerFactory : INetworkServerFactory
    {
        private readonly ConcurrentDictionary<string, BufferBlock<INetworkClient>>
            _networkServers
                = new ConcurrentDictionary<string, BufferBlock<INetworkClient>>();
        
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
            var socketId = GetSocketId(address, port, protocolType);
            var clients = new BufferBlock<INetworkClient>();
            if (_networkServers.TryAdd(
                socketId, clients) == false)
            {
                throw new InvalidOperationException($"Socket {socketId} has already started");
            }
            return InMemoryServer.Start(clients);
        }
    }
}