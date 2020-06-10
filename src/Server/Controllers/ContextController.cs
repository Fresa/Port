using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using k8s;
using Microsoft.AspNetCore.Mvc;
using Port.Shared;

namespace Port.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContextController : ControllerBase
    {
        [HttpGet]
        public async Task<IEnumerable<Context>> Get()
        {
            return (await KubernetesClientConfiguration.LoadKubeConfigAsync())
                .Contexts.Select(
                    context => new Context
                    {
                        Name = context.Name
                    });
        }
    }
}