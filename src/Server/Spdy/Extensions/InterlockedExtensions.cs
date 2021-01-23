using System.Threading;

namespace Port.Server.Spdy.Extensions
{
    internal static class InterlockedExtensions
    {
        internal static unsafe uint Exchange(ref uint target, uint value)
        {
            fixed (uint* pointerToTarget = &target)
            {
                return (uint)Interlocked.Exchange(ref *(int*)pointerToTarget, (int)value);
            }
        }
    }
}