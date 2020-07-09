using System.Collections.Generic;
using System.Linq;

namespace Port.Shared
{
    public sealed class Pod
    {
        public Pod(
            string @namespace,
            string name,
            IDictionary<string, string>? labels)
        {
            labels ??= new Dictionary<string, string>();
            Namespace = @namespace;
            Name = name;
            Labels = labels.ToDictionary(
                pair => pair.Key, pair => pair.Value);
        }

        public string Name { get; }
        public IReadOnlyDictionary<string, string> Labels { get; }
        public string Namespace { get; }
    }
}