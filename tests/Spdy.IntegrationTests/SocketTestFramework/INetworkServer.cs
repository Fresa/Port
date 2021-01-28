using System;
using System.Threading;
using System.Threading.Tasks;
using Spdy.Network;

namespace Spdy.IntegrationTests.SocketTestFramework
{
    internal interface INetworkServer : IAsyncDisposable
    {
        Task<INetworkClient> WaitForConnectedClientAsync(
            CancellationToken cancellationToken = default);
    }
}