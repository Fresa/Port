using System.Threading;
using System.Threading.Tasks;

namespace Port.Server
{
    internal interface INetworkServer
    {
        Task<INetworkClient> WaitForConnectedClientAsync(
            CancellationToken cancellationToken = default);
    }
}