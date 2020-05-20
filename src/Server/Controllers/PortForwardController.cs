using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Kubernetes.PortForward.Manager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PortForwardController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<Shared.PortForward> Get()
        {
            return Enumerable.Empty<Shared.PortForward>();
        }
    }
}
