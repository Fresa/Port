using System.Net.Sockets;

namespace Port.Shared
{
    public sealed class Port
    {
        public int Number { get; set; }
        public ProtocolType ProtocolType { get; set; }
    }
}