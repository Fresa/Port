using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy
{
    public class SynReply : Control
    {
        public const ushort Key = 2;
        protected override ushort Type => Key;
        public bool IsFin => Flags == 1;
        public int StreamId { get; set; }

        internal static async ValueTask<SynReply> ReadAsync(
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {

        }

        public Dictionary<string, string> Headers { get; set; } =
            new Dictionary<string, string>();
    }
}