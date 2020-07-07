namespace Port.Shared
{
    public sealed class Context
    {
        public Context(
            string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}