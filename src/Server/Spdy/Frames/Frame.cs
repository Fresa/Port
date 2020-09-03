using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy.Frames
{
    public abstract class Frame
    {
        internal static async ValueTask<ReadResult<Frame>> TryReadAsync(
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            var firstByte = await frameReader.PeekByteAsync(cancellation)
                                             .ConfigureAwait(false);
            var isControlFrame = (firstByte & 0x80) != 0;
            if (isControlFrame)
            {
                return await Control.TryReadAsync(frameReader, cancellation)
                                    .ConfigureAwait(false);
            }

            return await Data.TryReadAsync(frameReader, cancellation)
                             .ConfigureAwait(false);
        }

        internal async ValueTask WriteAsync(
            IFrameWriter frameWriter,
            CancellationToken cancellationToken = default)
        {
            await WriteFrameAsync(frameWriter, cancellationToken)
                .ConfigureAwait(false);
        }

        protected abstract ValueTask WriteFrameAsync(
            IFrameWriter frameWriter,
            CancellationToken cancellationToken = default);
    }
}