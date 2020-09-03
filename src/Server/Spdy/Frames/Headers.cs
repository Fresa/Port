using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Extensions;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy.Frames
{
    /// <summary>
    /// The HEADERS frame augments a stream with additional headers. It may be optionally sent on an existing stream at any time. Specific application of the headers in this frame is application-dependent. The name/value header block within this frame is compressed.
    /// 
    /// +------------------------------------+
    /// |1|   version     |          8       |
    /// +------------------------------------+
    /// | Flags (8)  |   Length (24 bits)    |
    /// +------------------------------------+
    /// |X|          Stream-ID (31bits)      |
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
    public class Headers : Control
    {
        private Headers(
            Options flags,
            UInt31 streamId,
            IReadOnlyDictionary<string, string[]> values)
            : base(Type)
        {
            Flags = flags;
            StreamId = streamId;
            Values = values;
        }

        public Headers(
            UInt31 streamId,
            IReadOnlyDictionary<string, string[]> values)
            : this(
                Options.None,
                streamId,
                values)
        {
        }

        public static Headers Last(
            UInt31 streamId,
            IReadOnlyDictionary<string, string[]> values)
        {
            return new Headers(Options.Fin, streamId, values);
        }

        public const ushort Type = 8;

        /// <summary>
        /// Flags related to this frame. 
        /// </summary>
        private new Options Flags
        {
            get => (Options) base.Flags;
            set => base.Flags = (byte) value;
        }

        [Flags]
        public enum Options : byte
        {
            None = 0,
            /// <summary>
            /// Marks this frame as the last frame to be transmitted on this stream and puts the sender in the half-closed (Section 2.3.6) state.
            /// </summary>
            Fin = 1
        }

        public bool IsLastFrame => Flags == Options.Fin;

        /// <summary>
        /// The stream this HEADERS block is associated with.
        /// </summary>
        public UInt31 StreamId { get; }

        /// <summary>
        /// A set of name/value pairs carried as part of the SYN_STREAM. see Name/Value Header Block (Section 2.6.10).
        /// </summary>
        public IReadOnlyDictionary<string, string[]> Values { get; }

        internal static async ValueTask<ReadResult<Headers>> TryReadAsync(
            byte flags,
            UInt24 length,
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            var streamId =
                await frameReader.ReadUInt32Async(cancellation)
                    .AsUInt31Async()
                    .ConfigureAwait(false);
            // An unsigned 24 bit value representing the number of bytes after the length field. The minimum length of the length field is 4 (when the number of name value pairs is 0).
            var headerLength = (int) length.Value - 4;
            var values =
                await
                    (await frameReader
                        .ReadBytesAsync(headerLength, cancellation)
                        .ConfigureAwait(false))
                    .ZlibDecompress(SpdyConstants.HeadersDictionary)
                    .ToFrameReader()
                    .ReadNameValuePairs(cancellation)
                    .ConfigureAwait(false);

            return ReadResult.Ok(new Headers(flags.ToEnum<Options>(), streamId, values));
        }

        protected override async ValueTask WriteControlFrameAsync(
            IFrameWriter frameWriter,
            CancellationToken cancellationToken = default)
        {
            await using var headerStream = new MemoryStream(1024);
            await using var headerWriter = new FrameWriter(headerStream);
            {
                await headerWriter.WriteNameValuePairs(
                        Values, cancellationToken)
                    .ConfigureAwait(false);
            }

            var compressedHeaders = headerStream.ToArray()
                .ZlibCompress(SpdyConstants.HeadersDictionary);

            var length = compressedHeaders.Length + 4;
            await frameWriter.WriteUInt24Async(
                    UInt24.From((uint) length), cancellationToken)
                .ConfigureAwait(false);
            await frameWriter.WriteUInt32Async(
                    StreamId.Value, cancellationToken)
                .ConfigureAwait(false);
            await frameWriter.WriteBytesAsync(
                    compressedHeaders, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}