using System.Threading;
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

        [HttpGet("{name}/portforward")]
        [HttpPost("{name}/portforward")]
        public async Task<ActionResult<string>> PortForward(
            string @namespace,
            string name,
            [FromQuery] int[] ports,
            CancellationToken cancellationToken)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets
                    .AcceptWebSocketAsync()
                    .ConfigureAwait(false);

                var subscription = new PortForward(@namespace, name, ports);
                await _testFramework.Pod.PortForward.WaitAsync(
                        subscription, webSocket, cancellationToken)
                    .ConfigureAwait(false);

                return Ok();
            }

            return BadRequest();
        }
    }
}