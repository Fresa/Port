using Port.Server.Spdy.Frames;

namespace Port.Server.Spdy.Extensions
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