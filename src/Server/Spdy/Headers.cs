using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Extensions;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    public class Headers : Control
    {
        private Headers(
            HeadersFlags flags,
            UInt31 streamId,
            IReadOnlyDictionary<string, string> values)
            : base(Type)
        {
            Flags = flags;
            StreamId = streamId;
            Values = values;
        }

        public Headers(
            UInt31 streamId,
            IReadOnlyDictionary<string, string> values)
            : this(
                HeadersFlags.None,
                streamId,
                values)
        {
        }

        public static Headers Last(
            UInt31 streamId,
            IReadOnlyDictionary<string, string> values)
        {
            return new Headers(HeadersFlags.Fin, streamId, values);
        }

        public const ushort Type = 8;

        /// <summary>
        /// Flags related to this frame. 
        /// </summary>
        private new HeadersFlags Flags
        {
            get => (HeadersFlags) base.Flags;
            set => base.Flags = (byte) value;
        }

        [Flags]
        public enum HeadersFlags : byte
        {
            None = 0,
            /// <summary>
            /// Marks this frame as the last frame to be transmitted on this stream and puts the sender in the half-closed (Section 2.3.6) state.
            /// </summary>
            Fin = 1
        }

        public bool IsLastFrame => Flags == HeadersFlags.Fin;

        /// <summary>
        /// The stream this HEADERS block is associated with.
        /// </summary>
        public UInt31 StreamId { get; }

        /// <summary>
        /// A set of name/value pairs carried as part of the SYN_STREAM. see Name/Value Header Block (Section 2.6.10).
        /// </summary>
        public IReadOnlyDictionary<string, string> Values { get; }

        internal static async ValueTask<Headers> ReadAsync(
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

            return new Headers(flags.ToEnum<HeadersFlags>(), streamId, values);
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