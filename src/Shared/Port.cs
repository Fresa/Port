using System.Net.Sockets;

namespace Port.Shared
{
    public sealed class Port
    {
        public Port(
            int number,
            ProtocolType protocolType)
        {
            Number = number;
            ProtocolType = protocolType;
        }

        public int Number { get; }
        public ProtocolType ProtocolType { get; }
    }
}