using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.PortForward.Manager.Server
{
    internal interface INetworkServer
    {
        Task<INetworkClient> WaitForConnectedClientAsync(
            CancellationToken cancellationToken = default);
    }
}