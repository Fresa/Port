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
            return new k8s.Kubernetes(
                config,
                _configuration.CreateClient(config.CreateDefaultHttpClientHandler()));
        }
    }
}