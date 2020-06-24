using System;
using Microsoft.AspNetCore.Http;

namespace Kubernetes.Test.API.Server.Subscriptions
{
    public sealed class WebSocketRequestSubscription
    {
        private Action<HttpRequest> _onWebSocketRequestSubscription =
            request => { };
        internal void WebSocketRequest(
            HttpRequest httpRequest)
        {
            _onWebSocketRequestSubscription(httpRequest);
        }
        public void OnWebSocketRequest(
            Action<HttpRequest> subscription)
        {
            _onWebSocketRequestSubscription = subscription;
        }

    }
}