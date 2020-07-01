using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Test.API.Server.Subscriptions.Models;

namespace Kubernetes.Test.API.Server.Subscriptions
{
    public sealed class WebSocketRequestSubscription
    {
        private readonly ConcurrentDictionary<PortForward,
                OnConnectedWebSocketAsync>
            _onConnectedWebSocketSubscriptions =
                new ConcurrentDictionary<PortForward, OnConnectedWebSocketAsync>();

        public delegate Task OnConnectedWebSocketAsync(
            WebSocket portForwardSocket,
            CancellationToken cancellationToken = default);

        internal async Task WaitAsync(
            PortForward portForward,
            WebSocket webSocket,
            CancellationToken cancellationToken = default)
        {
            if (_onConnectedWebSocketSubscriptions.TryGetValue(
                portForward, out var subscription))
            {
                await subscription.Invoke(webSocket, cancellationToken)
                    .ConfigureAwait(false);
                return;
            }

            throw new InvalidOperationException(
                $"Missing subscription for websocket messages sent to {portForward}");
        }

        public void OnConnected(
            PortForward portForward,
            OnConnectedWebSocketAsync asyncSubscription)
        {
            if (_onConnectedWebSocketSubscriptions.TryAdd(
                portForward, asyncSubscription))
            {
                return;
            }

            throw new ArgumentException(
                $"Subscription for {portForward} already exists");
        }
    }
}