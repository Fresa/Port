using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy
{
    public abstract class Frame
    {
        protected abstract bool IsControlFrame { get; }

        internal static async ValueTask<Frame> ReadAsync(
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            var firstByte = await frameReader.PeekByteAsync(cancellation)
                .ConfigureAwait(false);
            var isControlFrame = (firstByte & 0x80) != 0;
            if (isControlFrame)
            {
                return await Control.ReadAsync(frameReader, cancellation)
                    .ConfigureAwait(false);
            }
            else
            {

            }
        }
    }
}