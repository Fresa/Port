using System.Threading;
using Spdy.Primitives;

namespace Spdy.Extensions
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

        internal static unsafe UInt31 Exchange(ref UInt31 target, UInt31 value)
        {
            fixed (UInt31* pointerToTarget = &target)
            {
                return (uint)Interlocked.Exchange(ref *(int*)pointerToTarget, value);
            }
        }

        internal static unsafe uint CompareExchange(ref uint target, uint value, uint comparer)
        {
            fixed (uint* pointerToTarget = &target)
            {
                return (uint) Interlocked.CompareExchange(
                    ref *(int*) pointerToTarget, (int) value, (int) comparer);
            }
        }

        internal static unsafe uint Add(ref uint target, int value)
        {
            fixed (uint* pointerToTarget = &target)
            {
                return (uint)Interlocked.Add(ref *(int*)pointerToTarget, value);
            }
        }

        internal static unsafe UInt31 Add(ref UInt31 target, int value)
        {
            fixed (UInt31* pointerToTarget = &target)
            {
                return (uint)Interlocked.Add(ref *(int*)pointerToTarget, value);
            }
        }
    }
}