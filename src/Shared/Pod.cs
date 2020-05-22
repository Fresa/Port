namespace Kubernetes.PortForward.Manager.Shared
{
    public sealed class Pod
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
    }
}