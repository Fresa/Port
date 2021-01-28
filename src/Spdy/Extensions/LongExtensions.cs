namespace Spdy.Extensions
{
    internal static class LongExtensions
    {
        internal static bool IsOdd(
            this long value)
        {
            return value % 2 != 0;
        }
    }
}