using System.Collections.Generic;

namespace Port.Shared
{
    public sealed class Service
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public IEnumerable<Port> Ports { get; set; }
    }
}