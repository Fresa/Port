using System;
using System.Threading;
using System.Threading.Tasks;
using Log.It;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy.Frames
{
    public abstract class Frame
    {
        private static readonly ILogger Logger = LogFactory.Create<Frame>();

        internal static async ValueTask<ReadResult<Frame>> TryReadAsync(
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            try
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
            catch (Exception e)
            {
                Logger.Error(e, "Error while parsing frame");
                return ReadResult<Frame>.Error(RstStream.ProtocolError(UInt31.From(0)));
            }
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