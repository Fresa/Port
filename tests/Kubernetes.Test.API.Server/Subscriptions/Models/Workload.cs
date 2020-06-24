namespace Kubernetes.Test.API.Server.Subscriptions.Models
{
    public abstract class Workload
    {
        protected internal Workload(
            string @namespace)
        {
            Namespace = @namespace;
        }

        public string Namespace { get; }
    }
}