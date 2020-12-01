using Log.It.With.NLog;
using Test.It.With.XUnit;

namespace Port.Server.IntegrationTests.TestFramework
{
    internal static class NLogCapturingTargetExtensions
    {
        private static readonly ExclusiveLock NLogCapturingTargetLock = 
            new ExclusiveLock();
        internal static void RegisterOutputOnce()
        {
            if (NLogCapturingTargetLock.TryAcquire())
            {
                NLogCapturingTarget.Subscribe += Output.Writer.Write;
            }
        }
    }
}