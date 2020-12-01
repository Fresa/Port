using Log.It;
using Log.It.With.NLog;

namespace Port.Server.Observability
{
    internal static class LogFactoryExtensions
    {
        private static readonly ExclusiveLock Lock = new ExclusiveLock();

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
        }
    }
}