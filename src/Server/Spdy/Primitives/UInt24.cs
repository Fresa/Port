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

        public uint Value => (uint)(One | (Two << 8) | (Three << 16));
    }
}