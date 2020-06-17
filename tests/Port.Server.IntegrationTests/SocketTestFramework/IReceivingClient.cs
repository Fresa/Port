using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.IntegrationTests.SocketTestFramework
{
    public interface IReceivingClient
    {
        ValueTask<IMessage> ReceiveAsync(
            CancellationToken cancellationToken = default);
    }
}