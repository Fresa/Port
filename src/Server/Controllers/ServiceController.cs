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
        private readonly IKubernetesService _kubernetesService;

        public ServiceController(
            IKubernetesService kubernetesService)
        {
            _kubernetesService = kubernetesService;
        }

        [HttpGet("{context}")]
        public async Task<IEnumerable<Service>> GetAsync(
            string context)
        {
            return await _kubernetesService.ListServicesInAllNamespacesAsync(
                context);
        }

        [HttpPost("{context}/portforward")]
        public async Task PostAsync(
            string context,
            Shared.PortForward portForward)
        {
            await _kubernetesService.PortForwardAsync(context, portForward);
        }

    }
}