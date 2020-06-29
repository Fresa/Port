using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Kubernetes.Test.API.Server.Subscriptions
{
    public sealed class WebSocketRequestSubscription
    {
        private readonly ConcurrentDictionary<string,
                Func<ReadOnlyMemory<byte>, ValueTask<byte[]>>>
            _onWebSocketMessageSubscriptions =
                new ConcurrentDictionary<string,
                    Func<ReadOnlyMemory<byte>, ValueTask<byte[]>>
                >();

        internal async ValueTask WebSocketMessageReceivedAsync(
            string path,
            int port,
            ReadOnlyMemory<byte> message)
        {
            var key = GetWebSocketMessageSubscriptionId(path, port);
            if (_onWebSocketMessageSubscriptions.TryGetValue(
                key, out var subscription))
            {
                await subscription.Invoke(message)
                    .ConfigureAwait(false);
                return;
            }

            throw new InvalidOperationException(
                $"Missing subscription for websocket messages received at path {path} and port {port}");
        }

        public void OnWebSocketMessage(
            string path,
            int port,
            Func<ReadOnlyMemory<byte>, ValueTask<byte[]>> subscription)
        {
            var key = GetWebSocketMessageSubscriptionId(path, port);
            if (_onWebSocketMessageSubscriptions.TryAdd(key, subscription))
            {
                return;
            }

            throw new ArgumentException(
                $"Subscription for path {path} and port {port} already exists");
        }

        private static string GetWebSocketMessageSubscriptionId(
            string path,
            int port)
        {
            return $"{path}:{port}";
        }
    }
}