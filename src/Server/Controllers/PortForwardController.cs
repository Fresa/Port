using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kubernetes.PortForward.Manager.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Kubernetes.PortForward.Manager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PortForwardController : ControllerBase
    {
        private readonly IKubernetesService _kubernetesService;

        public PortForwardController(
            IKubernetesService kubernetesService)
        {
            _kubernetesService = kubernetesService;
        }

        [HttpGet]
        public async Task<IEnumerable<Pod>> Get(
            string context)
        {
            return await _kubernetesService.ListPodsInAllNamespacesAsync(
                context);
        }
    }
}