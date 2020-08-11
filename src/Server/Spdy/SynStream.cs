using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Extensions;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    /// <summary>
    /// +------------------------------------+
    /// |1|    version    |         1        |
    /// +------------------------------------+
    /// |  Flags (8)  |  Length (24 bits)    |
    /// +------------------------------------+
    /// |X|           Stream-ID (31bits)     |
    /// +------------------------------------+
    /// |X| Associated-To-Stream-ID (31bits) |
    /// +------------------------------------+
    /// | Pri|Unused | Slot |                |
    /// +-------------------+                |
    /// | Number of Name/Value pairs (int32) |   &lt;+
    /// +------------------------------------+    |
    /// |     Length of name (int32)         |    | This section is the "Name/Value
    /// +------------------------------------+    | Header Block", and is compressed.
    /// |           Name (string)            |    |
    /// +------------------------------------+    |
    /// |     Length of value  (int32)       |    |
    /// +------------------------------------+    |
    /// |          Value   (string)          |    |
    /// +------------------------------------+    |
    /// |           (repeats)                |   &lt;+
    /// </summary>
    public class SynStream : Control
    {
        public SynStream(
            byte flags,
            UInt31 streamId,
            UInt31 associatedToStreamId,
            PriorityLevel priority,
            IReadOnlyDictionary<string, string> headers)
        {
            Flags = flags;
            StreamId = streamId;
            AssociatedToStreamId = associatedToStreamId;
            Priority = priority;
            Headers = headers;
        }

        public const ushort Type = 1;

        /// <summary>
        /// Flags related to this frame. 
        /// </summary>
        protected new byte Flags
        {
            get => base.Flags;
            set
            {
                if (value > 2)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(Flags),
                        $"Flags can only be 0 = none, 1 = {nameof(IsFin)} or 2 = {nameof(IsUnidirectional)}");
                }

                base.Flags = value;
            }
        }

        /// <summary>
        /// 0x01 = FLAG_FIN - marks this frame as the last frame to be transmitted on this stream and puts the sender in the half-closed (Section 2.3.6) state.
        /// </summary>
        public bool IsFin => Flags == 1;

        /// <summary>
        /// 0x02 = FLAG_UNIDIRECTIONAL - a stream created with this flag puts the recipient in the half-closed (Section 2.3.6) state.
        /// </summary>
        public bool IsUnidirectional => Flags == 2;

        /// <summary>
        /// The 31-bit identifier for this stream. This stream-id will be used in frames which are part of this stream.
        /// </summary>
        public UInt31 StreamId { get; }

        /// <summary>
        /// The 31-bit identifier for a stream which this stream is associated to. If this stream is independent of all other streams, it should be 0.
        /// </summary>
        public UInt31 AssociatedToStreamId { get; }

        public bool IsIndependentStream => AssociatedToStreamId.Value == 0;

        /// <summary>
        /// A 3-bit priority (Section 2.3.3) field.
        /// </summary>
        public PriorityLevel Priority { get; }

        /// <summary>
        /// Name/Value Header Block: A set of name/value pairs carried as part of the SYN_STREAM. see Name/Value Header Block (Section 2.6.10).
        /// </summary>
        public IReadOnlyDictionary<string, string> Headers { get; }

        internal static async ValueTask<SynStream> ReadAsync(
            byte flags,
            UInt24 length,
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            var streamId = UInt31.From(
                await frameReader.ReadUInt32Async(cancellation)
                    .ConfigureAwait(false) & 0x7FFF);
            var associatedToStreamId = UInt31.From(
                await frameReader.ReadUInt32Async(cancellation)
                    .ConfigureAwait(false) & 0x7FFF);
            var priority = Enum.Parse<PriorityLevel>(
                (await frameReader.ReadByteAsync(cancellation)
                    .ConfigureAwait(false) & 0xE0).ToString(), true);
            // Slot: 8 bits of unused space, reserved for future use. 
            await frameReader.ReadByteAsync(cancellation)
                .ConfigureAwait(false);
            // The length is the number of bytes which follow the length field in the frame. For SYN_STREAM frames, this is 10 bytes plus the length of the compressed Name/Value block.
            var headerLength = (int)length.Value - 10;
            var headers =
                await
                    (await frameReader
                        .ReadBytesAsync(headerLength, cancellation)
                        .ConfigureAwait(false))
                    .ZlibDecompress(SpdyConstants.HeadersDictionary)
                    .AsFrameReader()
                    .ReadNameValuePairs(cancellation)
                    .ConfigureAwait(false);

            return new SynStream(
                flags, streamId, associatedToStreamId, priority, headers);
        }

        public enum PriorityLevel
        {
            Top,
            Urgent,
            High,
            AboveNormal,
            Normal,
            BelowNormal,
            Low,
            Lowest
        }
    }
}
