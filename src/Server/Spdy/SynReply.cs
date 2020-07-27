using System.Collections.Generic;

namespace Port.Server.Spdy
{
    public class SynReply : Control
    {
        public override short Type => 2;
        public bool IsFin => Flags == 1;
        public int StreamId { get; set; }

        public Dictionary<string, string> Headers { get; set; } =
            new Dictionary<string, string>();
    }
}