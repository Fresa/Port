namespace Kubernetes.Test.API.Server.Subscriptions.Models
{
    public abstract class Pod : Workload
    {
        protected internal Pod(
            string @namespace,
            string name)
            : base(@namespace)
        {
            Name = name;
        }

        public string Name { get; }
    }
}