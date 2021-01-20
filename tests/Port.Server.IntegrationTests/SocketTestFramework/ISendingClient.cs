using System;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.IntegrationTests.SocketTestFramework
{
    public interface ISendingClient<in T> : IAsyncDisposable
    {
        ValueTask SendAsync(
            T payload,
            CancellationToken cancellationToken = default);
    }
}