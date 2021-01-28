using System.Runtime.ExceptionServices;
using Spdy.Frames;
using Spdy.Frames.Readers;

namespace Spdy.UnitTests.Extensions
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