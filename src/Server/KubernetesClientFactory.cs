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
            => new k8s.Kubernetes(
                KubernetesClientConfiguration.BuildConfigFromConfigFile(
                    currentContext: context,
                    kubeconfigPath: _configuration.KubernetesConfigPath),
                _configuration.CreateClient())
            {
                CreateWebSocketBuilder =
                    _configuration.CreateWebSocketBuilder
            };
    }
}