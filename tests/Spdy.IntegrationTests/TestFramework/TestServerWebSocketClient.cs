using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Test.It.While.Hosting.Your.Web.Application;

namespace Spdy.IntegrationTests.TestFramework
{
    internal sealed class TestServerWebSocketClient : IWebSocketClient
    {
        private readonly WebSocketClient _webSocketClient;

        public TestServerWebSocketClient(WebSocketClient webSocketClient)
        {
            _webSocketClient = webSocketClient;
        }

        public async Task<WebSocket> ConnectAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            return await _webSocketClient.ConnectAsync(uri, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}