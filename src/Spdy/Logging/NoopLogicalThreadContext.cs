namespace Spdy.Logging
{
    internal sealed class NoopLogicalThreadContext : ILogicalThreadContext
    {
        public void Set<TValue>(
            string key,
            TValue value) where TValue : notnull
        {
        }

        public TValue? Get<TValue>(
            string key)
            => default;

        public void Remove(
            string key)
        {
        }

        public bool Contains(
            string key)
            => false;
    }
}