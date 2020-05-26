using System.Collections.Generic;
using System.Threading.Tasks;
using Kubernetes.PortForward.Manager.Shared;

namespace Kubernetes.PortForward.Manager.Server
{
    public interface IKubernetesService
    {
        Task<IEnumerable<Deployment>>
            ListDeploymentsInAllNamespacesAsync(string context);

        Task<IEnumerable<Pod>> ListPodsInAllNamespacesAsync(string context);

        Task<IEnumerable<Service>>
            ListServicesInAllNamespacesAsync(string context);

        Task PortForwardAsync(
            string context,
            Shared.PortForward portForward);
    }
}