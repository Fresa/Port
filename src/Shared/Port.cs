using System.Net.Sockets;

namespace Kubernetes.PortForward.Manager.Shared
{
    public sealed class Port
    {
        public int Number { get; set; }
        public ProtocolType ProtocolType { get; set; }
    }
}