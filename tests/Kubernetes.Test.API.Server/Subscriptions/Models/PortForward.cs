namespace Kubernetes.Test.API.Server.Subscriptions.Models
{
    public sealed class PortForward : Pod
    {
        internal PortForward(
            string @namespace,
            string name,
            int[] ports)
            : base(@namespace, name)
        {
            Ports = ports;
        }

        public int[] Ports { get; }
    }
}