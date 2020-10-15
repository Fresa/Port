using System.Collections.Generic;
using System.Threading;
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
            => _kubernetesService = kubernetesService;

        [HttpGet("{context}")]
        public Task<IEnumerable<Service>> GetAsync(
            string context)
            => _kubernetesService
                .ListServicesInAllNamespacesAsync(context);

        [HttpPost("{context}/portforward")]
        public Task PostAsync(
            string context,
            PortForward portForward,
            CancellationToken cancellationToken)
            => _kubernetesService.PortForwardAsync(
                context, portForward, cancellationToken);
    }
}