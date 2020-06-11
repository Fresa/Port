using System.Net.Http;

namespace Port.Server
{
    internal sealed class KubernetesConfiguration
    {
        public KubernetesConfiguration(
            string? kubernetesConfigPath = default,
            DelegatingHandler[] handlers = default)
        {
            KubernetesConfigPath = kubernetesConfigPath;
            Handlers = handlers ?? new DelegatingHandler[0];
        }

        public string? KubernetesConfigPath { get; }

        public DelegatingHandler[] Handlers { get; }
    }
}