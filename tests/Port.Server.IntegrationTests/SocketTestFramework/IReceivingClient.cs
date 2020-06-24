using System;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.IntegrationTests.SocketTestFramework
{
    public interface IReceivingClient<T> : IAsyncDisposable
    {
        ValueTask<T> ReceiveAsync(
            CancellationToken cancellationToken = default);
    }
}