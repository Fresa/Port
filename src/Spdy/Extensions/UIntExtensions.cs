using System;
using System.Threading.Tasks;
using Spdy.Primitives;

namespace Spdy.Extensions
{
    internal static class UIntExtensions
    {
        internal static uint SetBit(
            this UInt31 value,
            int bitIndex,
            bool bitValue)
        {
            if (bitIndex < 0 || bitIndex > 31)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bitIndex),
                    "Index must be in range [0..31]");
            }

            if (bitValue)
            {
                return (uint) (value | (1 << bitIndex));
            }

            return (uint) (value & ~(1 << bitIndex));
        }

        internal static async ValueTask<T> ToEnumAsync<T>(
            this ValueTask<uint> value)
            where T : struct, Enum
        {
            return Enum.Parse<T>(
                (await value.ConfigureAwait(false))
                .ToString());
        }

        internal static async ValueTask<UInt31> AsUInt31Async(
            this ValueTask<uint> value)
        {
            return UInt31.From(await value
                .ConfigureAwait(false) & 0x7FFFFFFF);
        }

        internal static bool IsOdd(
            this uint value)
        {
            return value % 2 != 0;
        }
    }
}