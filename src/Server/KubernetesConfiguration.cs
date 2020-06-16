using System.Net.Http;
using k8s;

namespace Port.Server
{
    internal sealed class KubernetesConfiguration
    {
        public KubernetesConfiguration(
            string? kubernetesConfigPath = default,
            WebSocketBuilder webSocketBuilder = default,
            DelegatingHandler[] handlers = default)
        {
            KubernetesConfigPath = kubernetesConfigPath;
            Handlers = handlers ?? new DelegatingHandler[0];
            WebSocketBuilder = webSocketBuilder ?? new WebSocketBuilder();
        }

        public string? KubernetesConfigPath { get; }

        public DelegatingHandler[] Handlers { get; }

        public WebSocketBuilder WebSocketBuilder { get; }
    }
}