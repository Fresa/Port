using System.IO.Pipelines;
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
        private readonly WebSocketReceiver _webSocketReceiver;

        public PodController(
            TestFramework testFramework,
            WebSocketReceiver webSocketReceiver)
        {
            _testFramework = testFramework;
            _webSocketReceiver = webSocketReceiver;
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
                var reader = _webSocketReceiver.Start(webSocket);
                ReadResult result;
                do
                {
                    result = await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false);
                    await _testFramework.WebSocketRequestSubscription
                        .WebSocketMessageReceivedAsync(
                            new PortForward(@namespace, name, ports), result.Buffer)
                        .ConfigureAwait(false);

                } while (result.IsCanceled == false &&
                         result.IsCompleted == false);

                await reader.CompleteAsync()
                    .ConfigureAwait(false);
                return Ok();
            }

            return BadRequest();
        }
    }
}