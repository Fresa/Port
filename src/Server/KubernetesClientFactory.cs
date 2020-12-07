using System.Net.Http;
using k8s;

namespace Port.Server
{
    internal sealed class KubernetesClientFactory : IKubernetesClientFactory
    {
        private readonly KubernetesConfiguration _configuration;

        public KubernetesClientFactory(
            KubernetesConfiguration configuration)
            => _configuration = configuration;

        public k8s.Kubernetes Create(
            string context)
        {
            var config =
                KubernetesClientConfiguration.BuildConfigFromConfigFile(
                    currentContext: context,
                    kubeconfigPath: _configuration.KubernetesConfigPath);
            return new KubernetesClient(
                config,
                _configuration.CreateClient(config.CreateDefaultHttpClientHandler()))
            {
                CreateWebSocketBuilder = _configuration.CreateWebSocketBuilder
            };
        }
    }

    internal sealed class KubernetesClient : k8s.Kubernetes
    {
        public KubernetesClient(
            KubernetesClientConfiguration kubernetesClientConfiguration,
            HttpClient httpClient)
            : base(kubernetesClientConfiguration, httpClient)
        // todo: This is a HACK to mitigate that the k8s websocket client
        // is dependent on that the HttpClientHandler is set in order to set
        // client certificates. Remove if we decide not to use websockets
        {
            HttpClientHandler =
                kubernetesClientConfiguration.CreateDefaultHttpClientHandler();
        }
    }
}