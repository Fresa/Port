using System;

namespace Spdy.Logging
{
    public static class LogFactory
    {
        private static ILogFactory _factory = new NoopLogFactory();

        public static bool TryInitializeOnce(ILogFactory logFactory)
        {
            lock (_factory)
            {
                if (_factory.GetType() != typeof(NoopLogFactory))
                {
                    return false;
                }

                _factory = logFactory;
            }

            return true;
        }

        internal static ILogger Create<T>()
        {
            if (_factory == null)
            {
                throw new InvalidOperationException($"Initialize the log factory before using it by calling {nameof(LogFactory)}.{nameof(TryInitializeOnce)}({nameof(ILogFactory)} logFactory)");
            }

            return _factory.Create<T>();
        }

        internal static ILogger Create(string logger)
        {
            if (_factory == null)
            {
                throw new InvalidOperationException($"Initialize the log factory before using it by calling {nameof(LogFactory)}.{nameof(TryInitializeOnce)}({nameof(ILogFactory)} logFactory)");
            }

            return _factory.Create(logger);
        }
    }
}