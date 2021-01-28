using Spdy.Frames;

namespace Spdy.Extensions
{
    internal static class SynStreamExtensions
    {
        internal static bool IsClient(
            this SynStream synStream)
        {
            return synStream.StreamId.IsOdd();
        }
    }
}