using Spdy.Logging;

namespace Spdy.UnitTests.Logging
{
    public class NLogFactory : ILogFactory
    {
        private readonly ILogicalThreadContext _logContext;

        public NLogFactory(ILogicalThreadContext logContext)
        {
            _logContext = logContext;
        }

        public ILogger Create(string logger)
        {
            return new NLogLogger(logger, _logContext);
        }

        public ILogger Create<T>()
        {
            return new NLogLogger(typeof(T).GetPrettyName(), _logContext);
        }
    }
}