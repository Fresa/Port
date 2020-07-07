using System.Net.Sockets;

namespace Port.Shared
{
    public sealed class PortForward
    {
        public PortForward(
            string @namespace,
            string name,
            ProtocolType protocolType,
            int podPort)
        {
            Name = name;
            Namespace = @namespace;
            ProtocolType = protocolType;
            PodPort = podPort;
        }

        public string Name { get; }
        public string Namespace { get; }
        public ProtocolType ProtocolType { get; }
        public int PodPort { get; }
        public int? LocalPort { get; set; }
    }
}