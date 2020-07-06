using System;
using System.Net.Http;
using k8s;

namespace Port.Server
{
    internal sealed class KubernetesConfiguration
    {
        public KubernetesConfiguration(
            string? kubernetesConfigPath = default,
            Func<WebSocketBuilder> createWebSocketBuilder = default,
            Func<DelegatingHandler[]> createHandlers = default)
        {
            KubernetesConfigPath = kubernetesConfigPath;
            CreateHandlers = createHandlers ?? (() => new DelegatingHandler[0]);
            CreateWebSocketBuilder = createWebSocketBuilder ?? (() => new WebSocketBuilder());
        }

        public string? KubernetesConfigPath { get; }

        public Func<DelegatingHandler[]> CreateHandlers { get; }

        public Func<WebSocketBuilder> CreateWebSocketBuilder { get; }
    }
}