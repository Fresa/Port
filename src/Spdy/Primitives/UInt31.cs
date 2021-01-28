using System;

namespace Spdy.Primitives
{
    public readonly struct UInt31
    {
        private byte One { get; }
        private byte Two { get; }
        private byte Three { get; }
        private byte Four { get; }

        public uint Value => (uint)(One | (Two << 8) | (Three << 16) | (Four << 24));

        internal static readonly UInt31 MaxValue = new UInt31(
            byte.MaxValue, byte.MaxValue, byte.MaxValue, 0x7F);

        private bool Equals(
            UInt31 other)
        {
            return One == other.One && Two == other.Two &&
                   Three == other.Three && Four == other.Four;
        }

        public override bool Equals(
            object? obj)
        {
            return obj switch
            {
                UInt31 uInt31 => Equals(uInt31),
                uint i => Equals(i),
                _ => false
            };
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

        public static UInt31 operator -(
            UInt31 left,
            UInt31 right)
        {
            return From(left.Value - right.Value);
        }

        private UInt31(
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

        public static implicit operator uint(
            UInt31 value) => value.Value;
        
        public static implicit operator int(
            UInt31 value) => (int)value.Value;

        public static implicit operator UInt31(
            uint value) => From(value);

        public static UInt31 From(
            uint value)
        {
            if (value > MaxValue.Value)
            {
                throw new InvalidOperationException(
                    $"value {value} must be less than or equal to {MaxValue}");
            }

            return new UInt31(
                (byte)(value & 0xFF),
                (byte)((value >> 8) & 0xFF),
                (byte)((value >> 16) & 0xFF),
                (byte)((value >> 24) & 0xFF));
        }
    }
}