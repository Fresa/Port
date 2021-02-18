using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Test.API.Server.Subscriptions.Models;
using Spdy;

namespace Kubernetes.Test.API.Server.Subscriptions
{
    public sealed class PortForwardRequestSubscription
    {
        private readonly ConcurrentDictionary<PortForward,
                OnConnectedSpdySessionAsync>
            _onConnectedSpdySessionSubscriptions =
                new();
        
        public delegate Task OnConnectedSpdySessionAsync(
            SpdySession portForwardSpdySession,
            CancellationToken cancellationToken = default);

        internal async Task WaitAsync(
            PortForward portForward,
            SpdySession spdySession,
            CancellationToken cancellationToken = default)
        {
            if (_onConnectedSpdySessionSubscriptions.TryGetValue(
                portForward, out var subscription))
            {
                await subscription.Invoke(spdySession, cancellationToken)
                                  .ConfigureAwait(false);
                return;
            }

            throw new InvalidOperationException(
                $"Missing subscription for messages sent over spdy to {portForward}");
        }

        public void OnConnected(
            PortForward portForward,
            OnConnectedSpdySessionAsync asyncSubscription)
        {
            if (_onConnectedSpdySessionSubscriptions.TryAdd(
                portForward, asyncSubscription))
            {
                return;
            }

            throw new ArgumentException(
                $"Subscription for {portForward} already exists");
        }
    }
}