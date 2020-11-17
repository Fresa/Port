using System.Threading;
using Microsoft.Extensions.Configuration;
using NLog.Extensions.Logging;
using NLog.Web;

namespace Port.Server.Observability
{
    internal static class NLogBuilderExtensions
    {
        private static int _nlogConfigurationLock;

        internal static void ConfigureNLogOnce(
            IConfiguration configuration)
        {
            var nLogConfig = new NLogLoggingConfiguration(
                configuration.GetSection("NLog"));
            if (Interlocked.CompareExchange(
                ref _nlogConfigurationLock, 1, 0) == 0)
            {
                NLogBuilder.ConfigureNLog(nLogConfig);
            }
        }
    }
}