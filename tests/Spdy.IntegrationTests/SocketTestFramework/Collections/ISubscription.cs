using System.Threading;
using System.Threading.Tasks;

namespace Spdy.IntegrationTests.SocketTestFramework.Collections
{
    internal interface ISubscription<T>
    {
        Task<T> ReceiveAsync(
            CancellationToken cancellation = default);
    }
}