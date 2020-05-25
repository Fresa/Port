using k8s;

namespace Kubernetes.PortForward.Manager.Server
{
    internal sealed class KubernetesClientFactory
    {
        internal IKubernetes Create(
            string context)
        {
            return new k8s.Kubernetes(
                KubernetesClientConfiguration.BuildConfigFromConfigFile(
                    currentContext: context));
        }
    }
}