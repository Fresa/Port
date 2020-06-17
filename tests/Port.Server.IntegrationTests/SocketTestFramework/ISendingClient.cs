using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.IntegrationTests.SocketTestFramework
{
    public interface ISendingClient
    {
        ValueTask SendAsync(
            IMessage payload,
            CancellationToken cancellationToken = default);
    }
}