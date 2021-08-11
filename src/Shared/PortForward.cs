using System.Net.Sockets;

namespace Port.Shared
{
    public sealed class PortForward
    {
        public PortForward(
            string @namespace,
            string pod,
            string service,
            ProtocolType protocolType,
            int podPort)
        {
            Pod = pod;
            Service = service;
            Namespace = @namespace;
            ProtocolType = protocolType;
            PodPort = podPort;
        }

        public string Pod { get; }
        public string Service { get; }
        public string Namespace { get; }
        public ProtocolType ProtocolType { get; }
        public int PodPort { get; }
        public int? LocalPort { get; set; }
        public bool Forwarding { get; set; }
    }
}