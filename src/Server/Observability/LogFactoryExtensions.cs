using Log.It;
using Log.It.With.NLog;
using LogFactory = Log.It.LogFactory;

namespace Port.Server.Observability
{
    internal static class LogFactoryExtensions
    {
        private static readonly ExclusiveLock Lock = new();

        public static void InitializeOnce()
        {
            if (!Lock.TryAcquire())
            {
                return;
            }

            if (LogFactory.HasFactory)
            {
                return;
            }

            LogFactory.Initialize(new NLogFactory(new LogicalThreadContext()));
            Spdy.Logging.LogFactory.TryInitializeOnce(
                new SpdyNLogFactory(new SpdyNLogLogContext()));
        }
    }
}