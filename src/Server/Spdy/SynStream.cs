using System.Collections.Generic;

namespace Port.Server.Spdy
{
    public class SynStream : Control
    {
        public override short Type => 1;
        public bool IsFin => Flags == 1;
        public bool IsUnidirectional => Flags == 2;
        public int StreamId { get; set; }
        public int AssociatedToStreamId { get; set; }
        public bool IsIndependentStream => AssociatedToStreamId == 0;
        public ushort Priority { get; set; }

        public Dictionary<string, string> Headers { get; set; } =
            new Dictionary<string, string>();
    }
}