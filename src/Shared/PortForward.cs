namespace Kubernetes.PortForward.Manager.Shared
{
    public class PortForward
    {
        public string Namespace { get; set; }
        public string ApplicationName { get; set; }
        public int ContainerPort { get; set; }
        public int Port { get; set; }
    }
}