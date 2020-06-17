using System;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server
{
    internal interface INetworkServer : IAsyncDisposable
    {
        Task<INetworkClient> WaitForConnectedClientAsync(
            CancellationToken cancellationToken = default);
    }
}