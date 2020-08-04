using System;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy
{
    public class Ping : Control
    {
        public Ping(
            byte flags)
            : base(flags)
        {
        }

        public const ushort Type = 6;
        public uint Id { get; set; }

        internal static async ValueTask<Ping> ReadAsync(
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }
    }
}