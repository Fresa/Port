using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy
{
    public class Headers : Control
    {
        public const ushort Type = 8;
        public bool IsLastFrame => Flags == 1;
        public Dictionary<string, string> Values { get; set; } =
            new Dictionary<string, string>();

        internal static async ValueTask<Headers> ReadAsync(
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {

        }
    }
}