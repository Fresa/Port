using System.Collections.Generic;

namespace Port.Shared
{
    public sealed class Service
    {
        public Service(
            string @namespace,
            string name,
            IEnumerable<Port> ports)
        {
            Namespace = @namespace;
            Name = name;
            Ports = ports;
        }

        public string Name { get; }
        public string Namespace { get; }
        public IEnumerable<Port> Ports { get; }
    }
}