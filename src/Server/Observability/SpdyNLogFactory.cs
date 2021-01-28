using Log.It;
using Spdy.Logging;

namespace Port.Server.Observability
{
    internal sealed class SpdyNLogFactory : Spdy.Logging.ILogFactory
    {
        private readonly ILogicalThreadContext _logContext;

        public SpdyNLogFactory(ILogicalThreadContext logContext)
        {
            _logContext = logContext;
        }

        public Spdy.Logging.ILogger Create(string logger)
        {
            return new SpdyNLogLogger(logger, _logContext);
        }

        public Spdy.Logging.ILogger Create<T>()
        {
            return new SpdyNLogLogger(typeof(T).GetPrettyName(), _logContext);
        }
    }
}