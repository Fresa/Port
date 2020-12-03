using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy.Extensions
{
    internal static class UInt31Extensions
    {
        internal static bool IsOdd(
            this UInt31 value)
        {
            return value.Value.IsOdd();
        }
    }
}