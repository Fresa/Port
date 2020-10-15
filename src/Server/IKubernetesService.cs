using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Port.Shared;

namespace Port.Server
{
    public interface IKubernetesService
    {
        Task<IEnumerable<Deployment>>
            ListDeploymentsInAllNamespacesAsync(string context);

        Task<IEnumerable<Shared.Pod>> ListPodsInAllNamespacesAsync(string context);

        Task<IEnumerable<Service>>
            ListServicesInAllNamespacesAsync(string context);

        Task PortForwardAsync(
            string context,
            Shared.PortForward portForward,
            CancellationToken cancellationToken = default);
    }
}