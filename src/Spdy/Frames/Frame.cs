using System;
using System.Threading;
using System.Threading.Tasks;
using Spdy.Frames.Readers;
using Spdy.Frames.Writers;
using Spdy.Logging;
using Spdy.Primitives;

namespace Spdy.Frames
{
    public abstract class Frame
    {
        private static readonly ILogger Logger = LogFactory.Create<Frame>();
        internal static async ValueTask<ReadResult<Frame>> TryReadAsync(
            IFrameReader frameReader,
            IHeaderReader headerReader,
            CancellationToken cancellation = default)
        {
            try
            {
                var firstByte = await frameReader.PeekByteAsync(cancellation)
                                                 .ConfigureAwait(false);
                Logger.Trace("Started reading frame");
                var isControlFrame = (firstByte & 0x80) != 0;
                if (isControlFrame)
                {
                    return await Control.TryReadAsync(frameReader, headerReader, cancellation)
                                        .ConfigureAwait(false);
                }

                return await Data.TryReadAsync(frameReader, cancellation)
                                 .ConfigureAwait(false);
            }
            catch when (cancellation.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                return ReadResult<Frame>.Error(
                    RstStream.ProtocolError(UInt31.From(0), exception));
            }
        }

        internal abstract ValueTask WriteAsync(
            IFrameWriter frameWriter,
            IHeaderWriterProvider headerWriterProvider,
            CancellationToken cancellationToken = default);
    }
}