using System;
using System.Threading.Tasks;

namespace Port.Server.Spdy.Extensions
{
    static internal class UIntExtensions
    {
        internal static uint SetBit(
            this uint value,
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
                return (uint) ((int) value | (1 << bitIndex));
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
    }
}