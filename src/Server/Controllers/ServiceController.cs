using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Port.Shared;

namespace Port.Server.Controllers
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
                context)
                .ConfigureAwait(false);
        }

        [HttpPost("{context}/portforward")]
        public async Task PostAsync(
            string context,
            Shared.PortForward portForward)
        {
            await _kubernetesService.PortForwardAsync(context, portForward)
                .ConfigureAwait(false);
        }

    }
}