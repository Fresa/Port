using System.Collections.Generic;
using System.Threading.Tasks;
using Kubernetes.PortForward.Manager.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Kubernetes.PortForward.Manager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServiceController : ControllerBase
    {
        private readonly KubernetesService _kubernetesService;

        public ServiceController(KubernetesService kubernetesService)
        {
            _kubernetesService = kubernetesService;
        }

        [HttpGet]
        public async Task<IEnumerable<Service>> GetAsync()
        {
            return await _kubernetesService.ListServicesInAllNamespacesAsync();
        }
    }
}
