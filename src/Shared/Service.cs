using System.Collections.Generic;
using System.Linq;

namespace Port.Shared
{
    public sealed class Service
    {
        public Service(
            string @namespace,
            string name,
            IEnumerable<Port> ports,
            IDictionary<string, string>? selectors)
        {
            selectors ??= new Dictionary<string, string>();
            Namespace = @namespace;
            Name = name;
            Ports = ports;
            Selectors = selectors.ToDictionary(
                pair => pair.Key, pair => pair.Value);
        }

        public string Name { get; }
        public string Namespace { get; }
        public IEnumerable<Port> Ports { get; }
        public IReadOnlyDictionary<string, string> Selectors { get; }
    }
}