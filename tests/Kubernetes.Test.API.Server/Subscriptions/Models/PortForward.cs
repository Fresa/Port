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
    }
}