using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Port.Server.IntegrationTests.SocketTestFramework
{
    internal sealed class InMemoryTestFramework : SocketTestFramework
    {
        private readonly IMessageClientFactory _messageClientFactory;

        internal InMemoryTestFramework(
            IMessageClientFactory messageClientFactory)
        {
            _messageClientFactory = messageClientFactory;
        }

        private readonly InMemoryNetworkServerFactory _networkServerFactory =
            new InMemoryNetworkServerFactory();
        public INetworkServerFactory NetworkServerFactory
            => _networkServerFactory;
    
        public async Task<ISendingClient> CreateClientAsync(
            IPAddress address,
            int port,
            ProtocolType protocolType,
            CancellationToken cancellationToken = default)
        {
            var first = new InMemoryNetworkClient();
            var second = new InMemoryNetworkClient();
            var requestClient =
                new CrossWiredMemoryNetworkClient(first, second);
            var responseClient =
                new CrossWiredMemoryNetworkClient(second, first);
            
            var requestMessageClient =
                _messageClientFactory.Create(requestClient);
            ReceiveMessagesFor(requestMessageClient);
            
            var server = _networkServerFactory.Get(
                address, port, protocolType);
            await server
                .SendAsync(responseClient, cancellationToken)
                .ConfigureAwait(false);
            
            return requestMessageClient;
        }
    }
}