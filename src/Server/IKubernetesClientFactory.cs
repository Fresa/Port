using k8s;

namespace Port.Server
{
    internal interface IKubernetesClientFactory
    {
        IKubernetes Create(
            string context);
    }
}