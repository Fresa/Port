using System;
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

        Task<IEnumerable<Pod>> ListPodsInAllNamespacesAsync(string context);

        Task<IEnumerable<Service>>
            ListServicesInAllNamespacesAsync(string context);

        Task<IAsyncDisposable> PortForwardAsync(
            string context,
            PortForward portForward,
            CancellationToken cancellationToken = default);
    }
}