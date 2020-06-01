using k8s;

namespace Kubernetes.PortForward.Manager.Server
{
    internal interface IKubernetesClientFactory
    {
        IKubernetes Create(
            string context);
    }
}