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
        private readonly IKubernetesService _kubernetesService;

        public PodController(
            IKubernetesService kubernetesService)
        {
            _kubernetesService = kubernetesService;
        }

        [HttpGet("{context}")]
        public async Task<IEnumerable<Pod>> GetAsync(
            string context)
        {
            return await _kubernetesService.ListPodsInAllNamespacesAsync(
                context);
        }
    }
}