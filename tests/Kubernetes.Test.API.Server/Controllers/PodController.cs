using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Test.API.Server.Subscriptions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Spdy.AspNetCore;

namespace Kubernetes.Test.API.Server.Controllers
{
    [ApiController]
    [Route("api/v1/namespaces/{namespace}/pods")]
    public class PodController : ControllerBase
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly TestFramework _testFramework;

        public PodController(
            IHostApplicationLifetime hostApplicationLifetime, 
            TestFramework testFramework)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
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
            if (!HttpContext.Spdy()
                            .IsSpdyRequest)
            {
                return BadRequest();
            }

            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                _hostApplicationLifetime.ApplicationStopping);

            var spdySession = await HttpContext.Spdy()
                                               .AcceptSpdyAsync()
                                               .ConfigureAwait(false);

            var subscription = new PortForward(@namespace, name, ports);
            await _testFramework.Pod.PortForward.WaitAsync(
                                    subscription, spdySession, cancellationTokenSource.Token)
                                .ConfigureAwait(false);

            return Ok();
        }
    }
}