using k8s;

namespace Kubernetes.PortForward.Manager.Server
{
    internal sealed class KubernetesClientFactory : IKubernetesClientFactory
    {
        public IKubernetes Create(
            string context)
        {
            return new k8s.Kubernetes(
                KubernetesClientConfiguration.BuildConfigFromConfigFile(
                    currentContext: context));
        }
    }
}