using System;

namespace Port.Server.Spdy.Primitives
{
    public readonly struct UInt24
    {
        internal byte One { get; }
        internal byte Two { get; }
        internal byte Three { get; }

        public UInt24(byte one, byte two, byte three)
        {
            One = one;
            Two = two;
            Three = three;
        }

        public static UInt24 From(
            uint value)
        {
            if (value >= (2 ^ 24))
            {
                throw new InvalidOperationException($"value {value} must be less than {2 ^ 24}");
            }

            return new UInt24(
                (byte)(value & 0xFF),
                (byte)((value >> 8) & 0xFF),
                (byte)((value >> 16) & 0xFF));
        }

        public uint Value => (uint)(One | (Two << 8) | (Three << 16));
    }
}