using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Spdy.Extensions;
using Spdy.Frames.Readers;
using Spdy.Frames.Writers;
using Spdy.Primitives;

namespace Spdy.Frames
{
    /// <summary>
    /// The RST_STREAM frame allows for abnormal termination of a stream. When sent by the creator of a stream, it indicates the creator wishes to cancel the stream. When sent by the recipient of a stream, it indicates an error or that the recipient did not want to accept the stream, so the stream should be closed.
    /// 
    /// +----------------------------------+
    /// |1|   version    |         3       |
    /// +----------------------------------+
    /// | Flags (8)  |         8           |
    /// +----------------------------------+
    /// |X|          Stream-ID (31bits)    |
    /// +----------------------------------+
    /// |          Status code             |
    /// +----------------------------------+
    /// </summary>
    public sealed class RstStream : Control
    {
        private RstStream(
            in Options flags,
            in UInt24 length,
            in UInt31 streamId,
            in StatusCode status) : this(streamId, status)
        {
            Flags = flags;
            Length = length;
        }

        private RstStream(
            in UInt31 streamId,
            in StatusCode status,
            Exception? exception = default) : base(Type)
        {
            StreamId = streamId;
            Status = status;

            var statusName = Enum.GetName(typeof(StatusCode), status);
            _exception = exception == null
                ? (Exception)new ProtocolViolationException(statusName)
                : new AggregateException(statusName, exception);
        }

        /// <summary>
        /// This is a generic error, and should only be used if a more specific error is not available.
        /// </summary>
        public static RstStream ProtocolError(
            in UInt31 streamId, Exception? exception = default)
        {
            return new RstStream(streamId, StatusCode.ProtocolError, exception);
        }

        /// <summary>
        /// This is returned when a frame is received for a stream which is not active.
        /// </summary>
        public static RstStream InvalidStream(
            in UInt31 streamId)
        {
            return new RstStream(streamId, StatusCode.InvalidStream);
        }

        /// <summary>
        /// Indicates that the stream was refused before any processing has been done on the stream.
        /// </summary>
        public static RstStream RefusedStream(
            in UInt31 streamId)
        {
            return new RstStream(streamId, StatusCode.RefusedStream);
        }

        /// <summary>
        /// Indicates that the recipient of a stream does not support the SPDY version requested.
        /// </summary>
        public static RstStream UnsupportedVersion(
            in UInt31 streamId)
        {
            return new RstStream(streamId, StatusCode.UnsupportedVersion);
        }

        /// <summary>
        /// Used by the creator of a stream to indicate that the stream is no longer needed.
        /// </summary>
        public static RstStream Cancel(
            in UInt31 streamId)
        {
            return new RstStream(streamId, StatusCode.Cancel);
        }

        /// <summary>
        /// This is a generic error which can be used when the implementation has internally failed, not due to anything in the protocol.
        /// </summary>
        public static RstStream InternalError(
            in UInt31 streamId)
        {
            return new RstStream(streamId, StatusCode.InternalError);
        }

        /// <summary>
        /// The endpoint detected that its peer violated the flow control protocol.
        /// </summary>
        public static RstStream FlowControlError(
            in UInt31 streamId)
        {
            return new RstStream(streamId, StatusCode.FlowControlError);
        }

        /// <summary>
        /// The endpoint received a <see cref="SynReply">SYN_REPLY</see> for a stream already open.
        /// </summary>
        public static RstStream StreamInUse(
            in UInt31 streamId)
        {
            return new RstStream(streamId, StatusCode.StreamInUse);
        }

        /// <summary>
        /// The endpoint received a data or <see cref="SynReply">SYN_REPLY</see> frame for a stream which is half closed.
        /// </summary>
        public static RstStream StreamAlreadyClosed(
            in UInt31 streamId)
        {
            return new RstStream(streamId, StatusCode.StreamAlreadyClosed);
        }

        /// <summary>
        /// The endpoint received a frame which this implementation could not support. If FRAME_TOO_LARGE is sent for a <see cref="SynStream">SYN_STREAM</see>, <see cref="Headers">HEADERS</see>, or <see cref="SynReply">SYN_REPLY</see> frame without fully processing the compressed portion of those frames, then the compression state will be out-of-sync with the other endpoint. In this case, senders of FRAME_TOO_LARGE MUST close the session.
        /// </summary>
        public static RstStream FrameToLarge(
            in UInt31 streamId)
        {
            return new RstStream(streamId, StatusCode.FrameToLarge);
        }

        public const ushort Type = 3;

        /// <summary>
        /// Flags related to this frame. RST_STREAM does not define any flags. This value must be 0.
        /// </summary>
        private new Options Flags
        {
            set => base.Flags = (byte)value;
        }

        [Flags]
        public enum Options : byte
        {
            None = 0
        }

        /// <summary>
        /// An unsigned 24-bit value representing the number of bytes after the length field. For RST_STREAM control frames, this value is always 8.
        /// </summary>
        public UInt24 Length
        {
            get => UInt24.From(8);
            private set
            {
                if (value.Value != 8)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(Length), "Length can only be 8");
                }
            }
        }

        private readonly Exception _exception;

        public static implicit operator Exception(
            RstStream stream) => stream._exception;

        /// <summary>
        /// The 31-bit identifier for this stream.
        /// </summary>
        public UInt31 StreamId { get; }

        /// <summary>
        /// An indicator for why the stream is being terminated.
        /// </summary>
        public StatusCode Status { get; }

        internal static async ValueTask<ReadResult<RstStream>> TryReadAsync(
            byte flags,
            UInt24 length,
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            var streamId =
                await frameReader.ReadUInt32Async(cancellation)
                    .AsUInt31Async()
                    .ConfigureAwait(false);
            var status =
                await frameReader.ReadUInt32Async(cancellation)
                    .ToEnumAsync<StatusCode>()
                    .ConfigureAwait(false);

            return ReadResult.Ok(new RstStream(flags.ToEnum<Options>(), length, streamId, status));
        }

        public enum StatusCode : uint
        {
            ProtocolError = 1,
            InvalidStream = 2,
            RefusedStream = 3,
            UnsupportedVersion = 4,
            Cancel = 5,
            InternalError = 6,
            FlowControlError = 7,
            StreamInUse = 8,
            StreamAlreadyClosed = 9,
            Unused = 10,
            FrameToLarge = 11
        }

        protected override async ValueTask WriteControlFrameAsync(
            IFrameWriter frameWriter,
            IHeaderWriterProvider _,
            CancellationToken cancellationToken = default)
        {
            await frameWriter.WriteUInt24Async(
                    Length, cancellationToken)
                .ConfigureAwait(false);
            await frameWriter.WriteUInt32Async(
                    StreamId, cancellationToken)
                .ConfigureAwait(false);
            await frameWriter.WriteUInt32Async(
                    (uint)Status, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}