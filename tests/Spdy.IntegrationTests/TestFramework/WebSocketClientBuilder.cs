using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using Microsoft.AspNetCore.TestHost;

namespace Spdy.IntegrationTests.TestFramework
{
    internal sealed class WebSocketClientBuilder : WebSocketBuilder
    {
        private readonly WebSocketClient _webSocketClient;

        public WebSocketClientBuilder(
            WebSocketClient webSocketClient)
        {
            _webSocketClient = webSocketClient;
        }

        public override async Task<WebSocket> BuildAndConnectAsync(
            Uri uri,
            CancellationToken cancellationToken)
        {
            return await _webSocketClient.ConnectAsync(uri, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}