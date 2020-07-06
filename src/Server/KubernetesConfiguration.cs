using System;
using System.Net.Http;
using k8s;

namespace Port.Server
{
    internal sealed class KubernetesConfiguration
    {
        public KubernetesConfiguration(
            string? kubernetesConfigPath = default,
            WebSocketBuilder webSocketBuilder = default,
            Func<DelegatingHandler[]> handlers = default)
        {
            KubernetesConfigPath = kubernetesConfigPath;
            CreateHandlers = handlers ?? (() => new DelegatingHandler[0]);
            WebSocketBuilder = webSocketBuilder ?? new WebSocketBuilder();
        }

        public string? KubernetesConfigPath { get; }

        public Func<DelegatingHandler[]> CreateHandlers { get; }

        public WebSocketBuilder WebSocketBuilder { get; }
    }
}