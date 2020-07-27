namespace Port.Server.Spdy
{
    public class Data : Frame
    {
        public int StreamId { get; set; }
        public bool LastFrame => Flags == 1;
        public byte Flags { get; set; }
        public uint Length { get; set; }
        public byte[] Payload { get; set; } = new byte[0];
        public override bool IsControlFrame => false;
    }
}