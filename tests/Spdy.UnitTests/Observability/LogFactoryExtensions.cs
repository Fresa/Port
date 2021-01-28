using Spdy.UnitTests.Logging;

namespace Spdy.UnitTests.Observability
{
    public static class LogFactoryExtensions
    {
        private static readonly NLogFactory LogFactory =
            new (new NLogLogicalThreadContext());

        public static void InitializeOnce()
        {
            Spdy.Logging.LogFactory.TryInitializeOnce(LogFactory);
        }
    }
}