using System.Collections.Generic;

namespace Kubernetes.PortForward.Manager.Shared
{
    public sealed class Service
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public IEnumerable<int> Ports { get; set; }
    }
}