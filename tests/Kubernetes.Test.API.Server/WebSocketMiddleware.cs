using System.Buffers;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Kubernetes.Test.API.Server
{
    internal sealed class WebSocketMiddleware : IMiddleware
    {
        private readonly TestFramework _testFramework;
        private readonly IHostApplicationLifetime _lifeTime;

        public WebSocketMiddleware(
            TestFramework testFramework,
            IHostApplicationLifetime lifeTime)
        {
            _testFramework = testFramework;
            _lifeTime = lifeTime;
        }

        public async Task InvokeAsync(
            HttpContext context,
            RequestDelegate next)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync()
                    .ConfigureAwait(false);
                await WaitForCloseOrApplicationStoppingAsync(
                        webSocket, context.Request)
                    .ConfigureAwait(false);
            }
            else
            {
                await next(context);
            }
        }

        private async Task WaitForCloseOrApplicationStoppingAsync(
            WebSocket webSocket,
            HttpRequest request)
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(65536);
            var memory = memoryOwner.Memory;

            while (webSocket.State.HasFlag(WebSocketState.Open))
            {
                try
                {
                    var received =
                        await webSocket.ReceiveAsync(
                                memory, _lifeTime.ApplicationStopping)
                            .ConfigureAwait(false);
                    await _testFramework.WebSocketRequestSubscription
                        .WebSocketMessageReceivedAsync(
                            request.Path, int.Parse(
                                request.Query["ports"]
                                    .First()), memory)
                        .ConfigureAwait(false);
                }
                catch when (webSocket.State != WebSocketState.Open ||
                            _lifeTime.ApplicationStopping
                                .IsCancellationRequested)
                {
                    break;
                }
            }

            if (webSocket.State == WebSocketState.Closed ||
                webSocket.State == WebSocketState.Aborted)
            {
                return;
            }

            await webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Socket closed",
                    _lifeTime.ApplicationStopping)
                .ConfigureAwait(false);
        }
    }
}