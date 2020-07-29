using System;

namespace Port.Server.Spdy.Extensions
{
    internal static class IntExtensions
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
                return (uint)((int)value | (1 << bitIndex));
            }

            return (uint)(value & ~(1 << bitIndex));
        }
    }
}