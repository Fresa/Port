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
        private uint _streamId;
        /// <summary>
        /// A 31-bit value identifying the stream.
        /// </summary>
        public uint StreamId
        {
            get => _streamId;
            set
            {
                if (value >= (2^31))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(StreamId), $"Stream id must be less than {2^31}");
                }

                _streamId = value;
            }
        }

        /// <summary>
        /// FLAG_FIN - signifies that this frame represents the last frame to be transmitted on this stream. See Stream Close (Section 2.3.7) below.
        /// </summary>
        public bool IsLastFrame
        {
            get => Flags == 1;
            set => Flags = 1;
        }

        private byte Flags { get; set; }

        /// <summary>
        /// The variable-length data payload;
        /// Length is an unsigned 24-bit value representing the number of bytes after the length field. The total size of a data frame is 8 bytes + length. It is valid to have a zero-length data frame.
        /// </summary>
        public byte[] Payload { get; set; } = new byte[0];

        public new async ValueTask WriteAsync(
            IFrameWriter frameWriter,
            CancellationToken cancellationToken = default)
        {
            var data = StreamId
                .SetBit(31, false);
            await frameWriter.WriteUInt32Async(data, cancellationToken)
                .ConfigureAwait(false);

            await frameWriter.WriteByteAsync(Flags, cancellationToken)
                .ConfigureAwait(false);

            await frameWriter.WriteUInt24Async(UInt24.From((uint)Payload.Length), cancellationToken)
                .ConfigureAwait(false);
        }

        internal new static async ValueTask<Control> ReadAsync(
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            // Since bit 31 is the control bit which is 0 for data frames,
            // there is no need to blank it out
            var streamId =
                await frameReader.ReadUInt32Async(cancellation)
                    .ConfigureAwait(false);
        }
    }
}