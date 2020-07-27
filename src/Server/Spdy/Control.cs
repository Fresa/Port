namespace Port.Server.Spdy
{
    public abstract class Control : Frame
    {
        public sealed override bool IsControlFrame => true;
        public short Version { get; set; } = 3;
        public abstract short Type { get; }
        protected byte Flags { get; set; }
        protected uint Length { get; set; }
        protected byte[] Data { get; set; } = new byte[0];
    }
}