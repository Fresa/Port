namespace Spdy.Logging
{
    internal class NoopLogFactory : ILogFactory
    {
        public ILogger Create(
            string logger)
            => new NoopLogger();

        public ILogger Create<T>() => new NoopLogger();
    }
}