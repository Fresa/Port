using System;
using System.Net.Http;

namespace Port.Server
{
    internal sealed class KubernetesConfiguration
    {
        public KubernetesConfiguration(
            string? kubernetesConfigPath = default,
            Func<HttpMessageHandler, HttpClient>? createClient = default)
        {
            KubernetesConfigPath = kubernetesConfigPath;
            CreateClient = createClient ?? (handler => new HttpClient(handler));
        }

        public string? KubernetesConfigPath { get; }

        public Func<HttpMessageHandler, HttpClient> CreateClient { get; }
    }
}