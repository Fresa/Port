using System;

namespace Port.Server.Spdy.Extensions
{
    internal static class IntExtensions
    {
        internal static int SetBit(
            this int value,
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
                return value | (1 << bitIndex);
            }

            return value & ~(1 << bitIndex);
        }
    }
}