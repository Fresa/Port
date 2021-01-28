using Microsoft.Extensions.Configuration;
using NLog.Extensions.Logging;
using Spdy.Helpers;

namespace Spdy.UnitTests.Observability
{
    public static class NLogBuilderExtensions
    {
        private static readonly ExclusiveLock NlogConfigurationLock =
            new ExclusiveLock();

        public static void ConfigureNLogOnce(
            IConfiguration configuration)
        {
            if (!NlogConfigurationLock.TryAcquire())
            {
                return;
            }

            var nLogConfig = new NLogLoggingConfiguration(
                configuration.GetSection("NLog"));
            NLog.LogManager.Configuration = nLogConfig;
        }
    }
}