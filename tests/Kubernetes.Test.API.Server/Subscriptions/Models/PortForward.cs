using System;
using System.Linq;

namespace Kubernetes.Test.API.Server.Subscriptions.Models
{
    public sealed class PortForward : Pod
    {
        public PortForward(
            string @namespace,
            string name,
            params int[] ports)
            : base(@namespace, name)
        {
            if (ports.Any() == false)
            {
                throw new ArgumentOutOfRangeException(nameof(ports), "Must specify at least one port");
            }
            Ports = ports;
        }

        public int[] Ports { get; }

        public bool Equals(PortForward other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Namespace == Namespace && 
                   other.Name == Name && 
                   other.Ports.All(port => Ports.Contains(port));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is PortForward portForward &&
                   Equals(portForward);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = Namespace.GetHashCode();
                result = (result * 397) ^ Name.GetHashCode();
                return Ports.Aggregate(
                    result, (
                        current,
                        port) => (current * 397) ^ port.GetHashCode());
            }
        }

        public override string ToString()
        {
            return $"{nameof(Namespace)}: {Namespace}, " +
                   $"{nameof(Name)}: {Name}, " +
                   $"{nameof(Ports)}: {string.Join(',', Ports)}, ";

        }
    }
}