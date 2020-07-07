namespace Port.Shared
{
    public sealed class Pod
    {
        public Pod(
            string @namespace,
            string name)
        {
            Namespace = @namespace;
            Name = name;
        }

        public string Name { get; }
        public string Namespace { get; }
    }
}