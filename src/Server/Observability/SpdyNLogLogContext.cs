using NLog;
using Spdy.Logging;

namespace Port.Server.Observability
{
    internal sealed class SpdyNLogLogContext : ILogicalThreadContext
    {
        public void Set<TValue>(
            string key,
            TValue value) where TValue : notnull
        {
            MappedDiagnosticsLogicalContext.Set(key, value);
        }

        public TValue? Get<TValue>(
            string key)
            => (TValue?) MappedDiagnosticsLogicalContext.GetObject(key);
        
        public void Remove(
            string key)
        {
            MappedDiagnosticsLogicalContext.Remove(key);
        }

        public bool Contains(
            string key)
            => MappedDiagnosticsLogicalContext.Contains(key);
    }
}