using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Spdy.IntegrationTests.SocketTestFramework
{
    internal sealed class InMemorySocketTestFramework : SocketTestFramework
    {
        private readonly InMemoryNetworkServerFactory _networkServerFactory =
            new InMemoryNetworkServerFactory();
        public INetworkServerFactory NetworkServerFactory
            => _networkServerFactory;

        public async Task<ISendingClient<T>> ConnectAsync<T>(
            IMessageClientFactory<T> messageClientFactory,
            IPAddress address,
            int port,
            ProtocolType protocolType,
            CancellationToken cancellationToken = default)
            where T : notnull
        {
            var requestClient =
                new CrossWiredInMemoryNetworkClient();
            var responseClient = requestClient.CreateReversed();

            var requestMessageClient =
                messageClientFactory.Create(requestClient);
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