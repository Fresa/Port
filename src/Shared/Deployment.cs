namespace Port.Shared
{
    public sealed class Deployment
    {
        public Deployment(
            string name,
            string @namespace)
        {
            Name = name;
            Namespace = @namespace;
        }

        public string Name { get; }
        public string Namespace { get; }
    }
}