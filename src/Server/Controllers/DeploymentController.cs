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
        private readonly IKubernetesService _kubernetesService;

        public DeploymentController(IKubernetesService kubernetesService)
        {
            _kubernetesService = kubernetesService;
        }

        [HttpGet("{context}")]
        public async Task<IEnumerable<Deployment>> Get(string context)
        {
            return await _kubernetesService.ListDeploymentsInAllNamespacesAsync(context);
        }
    }
}
