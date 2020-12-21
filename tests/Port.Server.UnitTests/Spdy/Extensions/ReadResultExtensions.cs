using System.Runtime.ExceptionServices;
using Port.Server.Spdy;
using Port.Server.Spdy.Frames;

namespace Port.Server.UnitTests.Spdy.Extensions
{
    internal static class ReadResultExtensions
    {
        internal static T GetOrThrow<T>(
            this ReadResult<T> readResult) where T : Frame
        {
            if (!readResult.Out(out var result, out var error))
            {
                ExceptionDispatchInfo.Capture(error).Throw();
            }

            return result;
        }
    }
}