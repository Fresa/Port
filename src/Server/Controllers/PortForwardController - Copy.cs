using System.Collections.Generic;
using System.Threading.Tasks;
using Kubernetes.PortForward.Manager.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Kubernetes.PortForward.Manager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PodController : ControllerBase
    {
        private readonly KubernetesService _kubernetesService;

        public PodController(KubernetesService kubernetesService)
        {
            _kubernetesService = kubernetesService;
        }

        [HttpGet]
        public async Task<IEnumerable<Pod>> Get()
        {
            return await _kubernetesService.ListPodsInAllNamespaces();
        }
    }
}
