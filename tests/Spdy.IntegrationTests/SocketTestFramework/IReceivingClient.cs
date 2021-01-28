using System;
using System.Threading;
using System.Threading.Tasks;

namespace Spdy.IntegrationTests.SocketTestFramework
{
    public interface IReceivingClient<T> : IAsyncDisposable
    {
        ValueTask<T> ReceiveAsync(
            CancellationToken cancellationToken = default);
    }
}