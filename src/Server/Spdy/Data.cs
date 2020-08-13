using System;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Extensions;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    /// <summary>
    /// +----------------------------------+
    /// |C|       Stream-ID (31bits)       |
    /// +----------------------------------+
    /// | Flags (8)  |  Length (24 bits)   |
    /// +----------------------------------+
    /// |               Data               |
    /// +----------------------------------+
    /// </summary>
    public class Data : Frame
    {
        private Data(
            UInt31 streamId,
            Options flags,
            byte[] payload)
        {
            StreamId = streamId;
            Flags = flags;
            Payload = payload;
        }
        /// <summary>
          /// A 31-bit value identifying the stream.
          /// </summary>
        public UInt31 StreamId { get; }

        /// <summary>
        /// Flags related to this frame. 
        /// </summary>
        private Options Flags { get; }

        [Flags]
        public enum Options : byte
        {
            None = 0,
            /// <summary>
            /// signifies that this frame represents the last frame to be transmitted on this stream. See Stream Close (Section 2.3.7) below.
            /// </summary>
            Fin = 1
        }

        /// <summary>
        /// The variable-length data payload;
        /// Length is an unsigned 24-bit value representing the number of bytes after the length field. The total size of a data frame is 8 bytes + length. It is valid to have a zero-length data frame.
        /// </summary>
        public byte[] Payload { get; }

        public static Data LastFrame(
            UInt31 streamId,
            byte[] payload)
        {
            return new Data(streamId, Options.Fin, payload);
        }

        public static Data Frame(
            UInt31 streamId,
            byte[] payload)
        {
            return new Data(streamId, Options.None, payload);
        }

        internal async ValueTask WriteAsync(
            IFrameWriter frameWriter,
            CancellationToken cancellationToken = default)
        {
            var data = StreamId.Value
                .SetBit(31, false);
            await frameWriter.WriteUInt32Async(data, cancellationToken)
                .ConfigureAwait(false);

            await frameWriter.WriteByteAsync((byte)Flags, cancellationToken)
                .ConfigureAwait(false);

            await frameWriter.WriteUInt24Async(
                    UInt24.From((uint)Payload.Length), cancellationToken)
                .ConfigureAwait(false);

            await frameWriter.WriteBytesAsync(Payload, cancellationToken)
                .ConfigureAwait(false);
        }

        protected override async ValueTask WriteFrameAsync(
            IFrameWriter frameWriter,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        internal new static async ValueTask<Data> ReadAsync(
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            // Since bit 31 is the control bit which is 0 for data frames,
            // there is no need to blank it out
            var streamId = UInt31.From(
                await frameReader.ReadUInt32Async(cancellation)
                    .ConfigureAwait(false));
            var flags = await frameReader.ReadByteAsync(cancellation)
                .ConfigureAwait(false);
            var length = await frameReader.ReadUInt24Async(cancellation)
                .ConfigureAwait(false);
            var payload = await frameReader.ReadBytesAsync(
                    (int)length.Value, cancellation)
                .ConfigureAwait(false);
            return new Data(streamId, flags.ToEnum<Options>(), payload);
        }
    }
}