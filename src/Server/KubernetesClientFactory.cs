using k8s;

namespace Kubernetes.PortForward.Manager.Server
{
    internal interface IKubernetesClientFactory
    {
        IKubernetes Create(
            string context);
    }

    internal sealed class KubernetesClientFactory : IKubernetesClientFactory
    {
        public IKubernetes Create(
            string context)
        {
            var a = new k8s.Kubernetes(
                KubernetesClientConfiguration.BuildConfigFromConfigFile(
                    currentContext: context));
            return a;
        }
    }
}