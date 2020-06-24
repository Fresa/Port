using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.IntegrationTests.SocketTestFramework
{
    public interface ISendingClient<T>
    {
        ValueTask SendAsync(
            T payload,
            CancellationToken cancellationToken = default);
    }
}