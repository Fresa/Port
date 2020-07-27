namespace Port.Server.Spdy
{
    public abstract class Frame
    {
        public abstract bool IsControlFrame { get; }
    }
}