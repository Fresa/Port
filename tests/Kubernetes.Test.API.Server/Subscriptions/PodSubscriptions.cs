namespace Kubernetes.Test.API.Server.Subscriptions
{
    public sealed class PodSubscriptions
    {
        public PortForwardRequestSubscription PortForward { get; } = new();
    }
}