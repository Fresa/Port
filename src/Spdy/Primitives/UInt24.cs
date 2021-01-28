using System;

namespace Spdy.Primitives
{
    public readonly struct UInt24
    {
        internal byte One { get; }
        internal byte Two { get; }
        internal byte Three { get; }
        public uint Value => (uint) (One | (Two << 8) | (Three << 16));

        internal static readonly UInt24 MaxValue = new UInt24(
            byte.MaxValue, byte.MaxValue, byte.MaxValue);

        public UInt24(
            byte one,
            byte two,
            byte three)
        {
            One = one;
            Two = two;
            Three = three;
        }

        public static UInt24 From(
            uint value)
        {
            if (value > MaxValue.Value)
            {
                throw new InvalidOperationException(
                    $"value {value} must be less than or equal to {MaxValue}");
            }

            return new UInt24(
                (byte) (value & 0xFF),
                (byte) ((value >> 8) & 0xFF),
                (byte) ((value >> 16) & 0xFF));
        }

        public override bool Equals(
            object? obj)
        {
            return obj is UInt24 comparing && Equals(comparing);
        }

        public bool Equals(
            UInt24 other)
        {
            return this == other;
        }

        public static bool operator ==(
            UInt24 first,
            UInt24 second)
        {
            return first.One == second.One &&
                   first.Two == second.Two &&
                   first.Three == second.Three;
        }

        public static bool operator !=(
            UInt24 first,
            UInt24 second)
        {
            return !(first == second);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(One, Two, Three);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}