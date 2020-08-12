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
    /// |1|    version    |         2        |
    /// +------------------------------------+
    /// |  Flags (8)  |  Length (24 bits)    |
    /// +------------------------------------+
    /// |X|           Stream-ID (31bits)     |
    /// +------------------------------------+
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
    public class SynReply : Control
    {
        public SynReply(
            byte flags,
            UInt31 streamId,
            IReadOnlyDictionary<string, string> headers)
        {
            Flags = flags;
            StreamId = streamId;
            Headers = headers;
        }
        
        public const ushort Type = 2;

        /// <summary>
        /// Flags related to this frame.
        /// </summary>
        protected new byte Flags
        {
            get => base.Flags;
            private set
            {
                if (value > 1)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(Flags),
                        $"Flags can only be 0 = none or 1 = {nameof(IsFin)}");
                }

                base.Flags = value;
            }
        }

        /// <summary>
        /// 0x01 = FLAG_FIN - marks this frame as the last frame to be transmitted on this stream and puts the sender in the half-closed (Section 2.3.6) state.
        /// </summary>
        public bool IsFin => Flags == 1;

        /// <summary>
        /// The 31-bit identifier for this stream.
        /// </summary>
        public UInt31 StreamId { get; }

        /// <summary>
        /// Name/Value Header Block: A set of name/value pairs carried as part of the SYN_STREAM. see Name/Value Header Block (Section 2.6.10).
        /// </summary>
        public IReadOnlyDictionary<string, string> Headers { get; }

        internal static async ValueTask<SynReply> ReadAsync(
            byte flags,
            UInt24 length,
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            var streamId = UInt31.From(
                await frameReader.ReadUInt32Async(cancellation)
                    .ConfigureAwait(false) & 0x7FFF);
            // The length is the number of bytes which follow the length field in the frame. For SYN_REPLY frames, this is 4 bytes plus the length of the compressed Name/Value block.
            var headerLength = (int)length.Value - 4;
            var headers =
                await
                    (await frameReader
                        .ReadBytesAsync(headerLength, cancellation)
                        .ConfigureAwait(false))
                    .ZlibDecompress(SpdyConstants.HeadersDictionary)
                    .AsFrameReader()
                    .ReadNameValuePairs(cancellation)
                    .ConfigureAwait(false);

            return new SynReply(flags, streamId, headers);
        }
    }
}
