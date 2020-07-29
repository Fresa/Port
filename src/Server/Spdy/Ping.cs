using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy
{
    public class Ping : Control
    {
        public const ushort Key = 6;
        protected override ushort Type => Key;
        public uint Id { get; set; }

        internal static async ValueTask<Ping> ReadAsync(
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {

        }
    }
}