using Port.Server.Spdy.Frames;

namespace Port.Server.Spdy.Extensions
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