using System.Net.Sockets;

namespace Port.Shared
{
    public sealed class PortForward
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public ProtocolType ProtocolType { get; set; }
        public int From { get; set; }
        public int? To { get; set; }
    }
}