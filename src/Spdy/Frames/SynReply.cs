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
    /// SYN_REPLY indicates the acceptance of a stream creation by the recipient of a SYN_STREAM frame.
    /// 
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
    public sealed class SynReply : Control
    {
        private SynReply(
            Options flags,
            UInt31 streamId,
            NameValueHeaderBlock? headers = null) : base(Type)
        {
            Flags = flags;
            StreamId = streamId;
            Headers = headers ?? new NameValueHeaderBlock();
        }

        public static SynReply AcceptAndClose(
            UInt31 streamId,
            NameValueHeaderBlock? headers = null)
        {
            return new SynReply(Options.Fin, streamId, headers);
        }

        public static SynReply Accept(
            UInt31 streamId,
            NameValueHeaderBlock? headers = null)
        {
            return new SynReply(Options.None, streamId, headers);
        }

        public const ushort Type = 2;

        /// <summary>
        /// Flags related to this frame.
        /// </summary>
        private new Options Flags
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
            Fin = 1
        }

        public bool IsLastFrame => Flags.HasFlag(Options.Fin);

        /// <summary>
        /// The 31-bit identifier for this stream.
        /// </summary>
        public UInt31 StreamId { get; }

        /// <summary>
        /// Name/Value Header Block: A set of name/value pairs carried as part of the SYN_STREAM. see Name/Value Header Block (Section 2.6.10).
        /// </summary>
        public IReadOnlyDictionary<string, string[]> Headers { get; }

        internal static async ValueTask<ReadResult<SynReply>> TryReadAsync(
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
            // The length is the number of bytes which follow the length field in the frame. For SYN_REPLY frames, this is 4 bytes plus the length of the compressed Name/Value block.
            var headerLength = (int)length.Value - 4;
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
                    return ReadResult<SynReply>.Error(
                        RstStream.ProtocolError(streamId, exception));
                }
            }

            return ReadResult.Ok(new SynReply(flags.ToEnum<Options>(), streamId, headers));
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

            var length = compressedHeaders.Length + 4;
            await frameWriter.WriteUInt24Async(
                    UInt24.From((uint)length), cancellationToken)
                .ConfigureAwait(false);
            await frameWriter.WriteUInt32Async(
                    StreamId, cancellationToken)
                .ConfigureAwait(false);
            await frameWriter.WriteBytesAsync(
                    compressedHeaders, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
