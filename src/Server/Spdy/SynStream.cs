using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy
{
    public class SynStream : Control
    {
        public const ushort Type = 1;
        public bool IsFin => Flags == 1;
        public bool IsUnidirectional => Flags == 2;
        public int StreamId { get; set; }
        public int AssociatedToStreamId { get; set; }
        public bool IsIndependentStream => AssociatedToStreamId == 0;
        public ushort Priority { get; set; }

        internal static async ValueTask<SynStream> ReadAsync(
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {

        }

        public Dictionary<string, string> Headers { get; set; } =
            new Dictionary<string, string>();
    }
}