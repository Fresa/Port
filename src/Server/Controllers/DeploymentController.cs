using System.Collections.Generic;
using System.Threading.Tasks;
using Kubernetes.PortForward.Manager.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Kubernetes.PortForward.Manager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DeploymentController : ControllerBase
    {
        private readonly KubernetesService _kubernetesService;

        public DeploymentController(KubernetesService kubernetesService)
        {
            _kubernetesService = kubernetesService;
        }

        [HttpGet]
        public async Task<IEnumerable<Deployment>> Get()
        {
            return await _kubernetesService.ListDeploymentsInAllNamespacesAsync();
        }
    }
}
