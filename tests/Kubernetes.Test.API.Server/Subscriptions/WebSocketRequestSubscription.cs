using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kubernetes.Test.API.Server.Subscriptions.Models;

namespace Kubernetes.Test.API.Server.Subscriptions
{
    public sealed class WebSocketRequestSubscription
    {
        private readonly ConcurrentDictionary<string,
                Func<ReadOnlySequence<byte>, ValueTask<byte[]>>>
            _onWebSocketMessageSubscriptions =
                new ConcurrentDictionary<string,
                    Func<ReadOnlySequence<byte>, ValueTask<byte[]>>
                >();

        internal async ValueTask WebSocketMessageReceivedAsync(
            PortForward portForward,
            ReadOnlySequence<byte> message)
        {
            var keys = GetWebSocketMessageSubscriptionIds(portForward);
            var tasks = keys.Select(
                async
                    key =>
                {
                    if (_onWebSocketMessageSubscriptions.TryGetValue(
                        key, out var subscription))
                    {
                        await subscription.Invoke(message)
                            .ConfigureAwait(false);
                        return;
                    }

                    throw new InvalidOperationException(
                        $"Missing subscription for websocket messages sent to {key}");
                });
            await Task.WhenAll(tasks);
        }

        public void OnWebSocketMessage(
            PortForward portForward,
            Func<ReadOnlySequence<byte>, ValueTask<byte[]>> subscription)
        {
            var keys = GetWebSocketMessageSubscriptionIds(
                portForward);
            foreach (var key in keys)
            {
                if (_onWebSocketMessageSubscriptions.TryAdd(key, subscription))
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