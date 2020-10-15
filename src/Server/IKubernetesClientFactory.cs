using k8s;

namespace Port.Server
{
    internal interface IKubernetesClientFactory
    {
        k8s.Kubernetes Create(
            string context);
    }
}