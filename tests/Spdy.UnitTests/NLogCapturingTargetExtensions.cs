using Spdy.Helpers;
using Spdy.UnitTests.Logging;
using Test.It.With.XUnit;

namespace Spdy.UnitTests
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