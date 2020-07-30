using System;

namespace Port.Server.Spdy.Primitives
{
    public readonly struct UInt31
    {
        internal byte One { get; }
        internal byte Two { get; }
        internal byte Three { get; }
        internal byte Four { get; }

        public uint Value => (uint)(One | (Two << 8) | (Three << 16) | (Four << 24));

        internal static readonly UInt31 MaxValue = new UInt31(
            0x7F, byte.MaxValue, byte.MaxValue, byte.MaxValue);

        public bool Equals(
            UInt31 other)
        {
            return One == other.One && Two == other.Two &&
                   Three == other.Three && Four == other.Four;
        }

        public override bool Equals(
            object? obj)
        {
            return obj is UInt31 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(One, Two, Three, Four);
        }

        public static bool operator ==(
            UInt31 left,
            UInt31 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            UInt31 left,
            UInt31 right)
        {
            return !left.Equals(right);
        }

        public UInt31(
            byte one,
            byte two,
            byte three,
            byte four)
        {
            One = one;
            Two = two;
            Three = three;
            Four = four;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static UInt31 From(
            uint value)
        {
            if (value >= (2 ^ 31))
            {
                throw new InvalidOperationException(
                    $"value {value} must be less than {MaxValue}");
            }

            return new UInt31(
                (byte)(value & 0xFF),
                (byte)((value >> 8) & 0xFF),
                (byte)((value >> 16) & 0xFF),
                (byte)((value >> 24) & 0xFF));
        }
    }
}