namespace Kubernetes.Test.API.Server.Subscriptions
{
    public sealed class PodSubscriptions
    {
        public WebSocketRequestSubscription PortForward { get; } = new WebSocketRequestSubscription();
    }
}