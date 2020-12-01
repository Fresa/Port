using Microsoft.Extensions.Configuration;
using NLog.Extensions.Logging;
using NLog.Web;

namespace Port.Server.Observability
{
    internal static class NLogBuilderExtensions
    {
        private static readonly ExclusiveLock NlogConfigurationLock =
            new ExclusiveLock();

        internal static void ConfigureNLogOnce(
            IConfiguration configuration)
        {
            if (!NlogConfigurationLock.TryAcquire())
            {
                return;
            }

            var nLogConfig = new NLogLoggingConfiguration(
                configuration.GetSection("NLog"));
            NLogBuilder.ConfigureNLog(nLogConfig);
        }
    }
}