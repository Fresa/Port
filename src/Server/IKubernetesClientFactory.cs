using k8s;

namespace Port.Server
{
    internal interface IKubernetesClientFactory
    {
        Kubernetes Create(
            string context);
    }
}