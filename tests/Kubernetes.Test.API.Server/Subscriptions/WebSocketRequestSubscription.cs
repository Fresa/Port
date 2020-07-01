using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Test.API.Server.Subscriptions.Models;

namespace Kubernetes.Test.API.Server.Subscriptions
{
    public sealed class WebSocketRequestSubscription
    {
        private readonly ConcurrentDictionary<string,
                OnConnectedSocketAsync>
            _onConnectedPortForwarderSubscriptions =
                new ConcurrentDictionary<string, OnConnectedSocketAsync>();

        private readonly ConcurrentDictionary<PortForward,
                OnConnectedWebSocketAsync>
            _onConnectedWebSocketSubscriptions =
                new ConcurrentDictionary<PortForward, OnConnectedWebSocketAsync>();


        public delegate Task OnConnectedSocketAsync(
            PortForwardSocket portForwardSocket,
            CancellationToken cancellationToken = default);

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


        internal async Task WaitAsync(
            PortForward portForward,
            PortForwardSocket portForwardSocket)
        {
            var keys = GetWebSocketMessageSubscriptionIds(portForward);
            var tasks = keys.Select(
                async
                    key =>
                {
                    if (_onConnectedPortForwarderSubscriptions.TryGetValue(
                        key, out var subscription))
                    {
                        await subscription.Invoke(portForwardSocket)
                            .ConfigureAwait(false);
                        return;
                    }

                    throw new InvalidOperationException(
                        $"Missing subscription for websocket messages sent to {key}");
                });
            await Task.WhenAll(tasks);
        }

        public void OnConnected(
            PortForward portForward,
            OnConnectedSocketAsync asyncSubscription)
        {
            var keys = GetWebSocketMessageSubscriptionIds(portForward);
            foreach (var key in keys)
            {
                if (_onConnectedPortForwarderSubscriptions.TryAdd(
                    key, asyncSubscription))
                {
                    return;
                }

                throw new ArgumentException(
                    $"Subscription for {key} already exists");
            }
        }

        private static IEnumerable<string> GetWebSocketMessageSubscriptionIds(
            PortForward portForward)
        {
            return portForward.Ports.Select(
                port => GetWebSocketMessageSubscriptionId(
                    portForward.Namespace, portForward.Name, port));
        }

        private static string GetWebSocketMessageSubscriptionId(
            string @namespace,
            string name,
            int port)
        {
            return $"{@namespace}.{name}:{port}";
        }
    }
}