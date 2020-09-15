using Log.It;

namespace Port.Server.Observability
{
    internal static class LogFactoryExtensions
    {
        private static readonly object Lock = new object();

        public static void InitializeOnce(
            ILogFactory logFactory)
        {
            lock (Lock)
            {
                if (LogFactory.HasFactory)
                {
                    return;
                }

                LogFactory.Initialize(logFactory);
            }
        }
    }
}