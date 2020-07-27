namespace Port.Server.Spdy
{
    public class Ping : Control
    {
        public override short Type => 6;
        public uint Id { get; set; }
    }
}