using Spdy.Frames;

namespace Spdy.Extensions
{
    internal static class PingExtensions
    {
        internal static bool IsOdd(
            this Ping ping)
        {
            return ping.Id.IsOdd();
        }
    }
}