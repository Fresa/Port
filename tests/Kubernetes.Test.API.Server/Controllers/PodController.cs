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
        private readonly PortForwardSocketFactory _portForwardSocketFactory;

        public PodController(
            TestFramework testFramework,
            PortForwardSocketFactory portForwardSocketFactory)
        {
            _testFramework = testFramework;
            _portForwardSocketFactory = portForwardSocketFactory;
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

                //var portForwarder = _portForwardSocketFactory
                //    .Create(webSocket);
                //await using (portForwarder
                //    .ConfigureAwait(false))
                //{
                var subscription = new PortForward(@namespace, name, ports);
                await _testFramework.Pod.PortForward.WaitAsync(
                        subscription, webSocket, cancellationToken)
                    .ConfigureAwait(false);

                //ReadResult result;
                //do
                //{
                //    result = await portForwarder.Reader.ReadAsync(cancellationToken)
                //        .ConfigureAwait(false);
                //    await _testFramework.Pod.PortForward
                //        .MessageReceivedAsync(
                //            new PortForward(@namespace, name, ports), result.Buffer)
                //        .ConfigureAwait(false);

                //} while (result.IsCanceled == false &&
                //         result.IsCompleted == false);

                //    await portForwarder.Reader.CompleteAsync()
                //        .ConfigureAwait(false);
                //}

                return Ok();
            }

            return BadRequest();
        }
    }
}