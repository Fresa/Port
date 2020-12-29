using System;
using System.Net.Http;
using k8s;

namespace Port.Server
{
    internal sealed class KubernetesConfiguration
    {
        public KubernetesConfiguration(
            string? kubernetesConfigPath = default,
            Func<WebSocketBuilder>? createWebSocketBuilder = default,
            Func<HttpMessageHandler, HttpClient>? createClient = default)
        {
            KubernetesConfigPath = kubernetesConfigPath;
            CreateClient = createClient ?? (handler => new HttpClient(handler));
            CreateWebSocketBuilder = createWebSocketBuilder ??
                                     (() => new WebSocketBuilder());
        }

        public string? KubernetesConfigPath { get; }

        public Func<HttpMessageHandler, HttpClient> CreateClient { get; }

        public Func<WebSocketBuilder> CreateWebSocketBuilder { get; }
    }
}