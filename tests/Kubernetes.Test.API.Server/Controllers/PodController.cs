using System.Threading.Tasks;
using Kubernetes.Test.API.Server.Subscriptions.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kubernetes.Test.API.Server.Controllers
{
    [ApiController]
    [Route("api/v1/namespaces/{namespace}/pods")]
    public class PodController : ControllerBase
    {
        private readonly TestFramework _testFramework;

        public PodController(
            TestFramework testFramework)
        {
            _testFramework = testFramework;
        }

        [HttpPost("{name}/portforward")]
        public async Task<ActionResult<string>> PortForward(
            string @namespace,
            string name,
            [FromQuery] int[] ports)
        {
            return await _testFramework.PodSubscriptions.PortForward(
                new PortForward(@namespace, name, ports));
        }
    }
}