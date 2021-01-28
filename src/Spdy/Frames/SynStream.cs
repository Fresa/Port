using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Spdy.Collections;
using Spdy.Extensions;
using Spdy.Frames.Readers;
using Spdy.Frames.Writers;
using Spdy.Primitives;
using ReadResult = Spdy.Frames.Readers.ReadResult;

namespace Spdy.Frames
{
    /// <summary>
    /// The SYN_STREAM control frame allows the sender to asynchronously create a stream between the endpoints. See Stream Creation (section 2.3.2)
    /// 
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
    public sealed class SynStream : Control
    {
        public SynStream(
            Options flags,
            UInt31 streamId,
            UInt31 associatedToStreamId,
            PriorityLevel priority,
            NameValueHeaderBlock headers)
            : base(Type)
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
        public new Options Flags
        {
            get => (Options)base.Flags;
            set => base.Flags = (byte)value;
        }

        [Flags]
        public enum Options : byte
        {
            None = 0,
            /// <summary>
            /// 0x01 = FLAG_FIN - marks this frame as the last frame to be transmitted on this stream and puts the sender in the half-closed (Section 2.3.6) state.
            /// </summary>
            Fin = 1,
            /// <summary>
            /// 0x02 = FLAG_UNIDIRECTIONAL - a stream created with this flag puts the recipient in the half-closed (Section 2.3.6) state.
            /// </summary>
            Unidirectional = 2
        }

        /// <summary>
        /// The local stream is in the half-closed (Section 2.3.6) state.
        /// </summary>
        public bool IsFin => Flags.HasFlag(Options.Fin);

        /// <summary>
        /// The remote stream is in the half-closed (Section 2.3.6) state.
        /// </summary>
        public bool IsUnidirectional => Flags.HasFlag(Options.Unidirectional);

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
        public IReadOnlyDictionary<string, string[]> Headers { get; }

        internal static async ValueTask<ReadResult<SynStream>> TryReadAsync(
            byte flags,
            UInt24 length,
            IFrameReader frameReader,
            IHeaderReader headerReader,
            CancellationToken cancellation = default)
        {
            var streamId =
                await frameReader.ReadUInt32Async(cancellation)
                    .AsUInt31Async()
                    .ConfigureAwait(false);
            var associatedToStreamId =
                await frameReader.ReadUInt32Async(cancellation)
                    .AsUInt31Async()
                    .ConfigureAwait(false);
            var priority =
                ((await frameReader.ReadByteAsync(cancellation)
                    .ConfigureAwait(false) & 0xE0) >> 5)
                .ToEnum<PriorityLevel>();
            // Slot: 8 bits of unused space, reserved for future use. 
            await frameReader.ReadByteAsync(cancellation)
                .ConfigureAwait(false);
            // The length is the number of bytes which follow the length field in the frame. For SYN_STREAM frames, this is 10 bytes plus the length of the compressed Name/Value block.
            var headerLength = (int)length.Value - 10;
            var headers = new NameValueHeaderBlock();
            if (headerLength > 0)
            {
                try
                {
                    headers =
                        await
                            headerReader
                            .ReadNameValuePairsAsync(headerLength, cancellation)
                            .ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    return ReadResult<SynStream>.Error(
                        RstStream.ProtocolError(streamId, exception));
                }
            }

            return ReadResult.Ok(new SynStream(
                flags.ToEnum<Options>(), streamId, associatedToStreamId, priority, headers));
        }

        public enum PriorityLevel : byte
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

        protected override async ValueTask WriteControlFrameAsync(
            IFrameWriter frameWriter,
            IHeaderWriterProvider headerWriterProvider,
            CancellationToken cancellationToken = default)
        {
            var pipe = new Pipe();
            var headerWriter = await headerWriterProvider
                                     .RequestWriterAsync(pipe.Writer, cancellationToken)
                                     .ConfigureAwait(false);
            await using (headerWriter.ConfigureAwait(false))
            {
                await headerWriter.WriteNameValuePairs(
                                      Headers, cancellationToken)
                                  .ConfigureAwait(false);
            }

            await using var memory = new MemoryStream();
            await pipe.Reader.CopyToAsync(memory, cancellationToken)
                      .ConfigureAwait(false);
            await pipe.Reader.CompleteAsync()
                      .ConfigureAwait(false);

            var compressedHeaders = memory.ToArray();
            var length = compressedHeaders.Length + 10;

            await frameWriter.WriteUInt24Async(
                                 UInt24.From((uint)length), cancellationToken)
                             .ConfigureAwait(false);
            await frameWriter.WriteUInt32Async(
                    StreamId, cancellationToken)
                .ConfigureAwait(false);
            await frameWriter.WriteUInt32Async(
                    AssociatedToStreamId, cancellationToken)
                .ConfigureAwait(false);
            await frameWriter.WriteByteAsync((byte)((byte)Priority << 5), cancellationToken)
                .ConfigureAwait(false);
            await frameWriter.WriteByteAsync(0, cancellationToken)
                             .ConfigureAwait(false);
            await frameWriter.WriteBytesAsync(
                                 compressedHeaders, cancellationToken)
                             .ConfigureAwait(false);
        }
    }
}