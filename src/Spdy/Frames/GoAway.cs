using System;
using System.Threading;
using System.Threading.Tasks;
using Spdy.Extensions;
using Spdy.Frames.Readers;
using Spdy.Frames.Writers;
using Spdy.Primitives;

namespace Spdy.Frames
{
    /// <summary>
    /// The GOAWAY control frame is a mechanism to tell the remote side of the connection to stop creating streams on this session. It can be sent from the client or the server. Once sent, the sender will not respond to any new SYN_STREAMs on this session. Recipients of a GOAWAY frame must not send additional streams on this session, although a new session can be established for new streams. The purpose of this message is to allow an endpoint to gracefully stop accepting new streams (perhaps for a reboot or maintenance), while still finishing processing of previously established streams.
    /// 
    /// There is an inherent race condition between an endpoint sending SYN_STREAMs and the remote sending a GOAWAY message. To deal with this case, the GOAWAY contains a last-stream-id indicating the stream-id of the last stream which was created on the sending endpoint in this session. If the receiver of the GOAWAY sent new SYN_STREAMs for sessions after this last-stream-id, they were not processed by the server and the receiver may treat the stream as though it had never been created at all (hence the receiver may want to re-create the stream later on a new session).
    /// 
    /// Endpoints should always send a GOAWAY message before closing a connection so that the remote can know whether a stream has been partially processed or not. (For example, if an HTTP client sends a POST at the same time that a server closes a connection, the client cannot know if the server started to process that POST request if the server does not send a GOAWAY frame to indicate where it stopped working).
    /// 
    /// After sending a GOAWAY message, the sender must ignore all SYN_STREAM frames for new streams, and MUST NOT create any new streams.
    /// 
    /// +----------------------------------+
    /// |1|   version    |         7       |
    /// +----------------------------------+
    /// | 0 (flags) |     8 (length)       |
    /// +----------------------------------|
    /// |X|  Last-good-stream-ID (31 bits) |
    /// +----------------------------------+
    /// |          Status code             |
    /// +----------------------------------+
    /// </summary>
    public class GoAway : Control
    {
        private GoAway(
            Options flags,
            UInt24 length,
            UInt31 lastGoodStreamId,
            StatusCode status)
            : this(lastGoodStreamId, status)
        {
            Flags = flags;
            Length = length;
        }

        private GoAway(
            UInt31 lastGoodStreamId,
            StatusCode status)
            : base(Type)
        {
            LastGoodStreamId = lastGoodStreamId;
            Status = status;
        }

        public static GoAway Ok(
            UInt31 lastGoodStreamId)
            => new GoAway(lastGoodStreamId, StatusCode.Ok);

        public static GoAway ProtocolError(
            UInt31 lastGoodStreamId)
            => new GoAway(lastGoodStreamId, StatusCode.ProtocolError);

        public static GoAway InternalError(
            UInt31 lastGoodStreamId)
            => new GoAway(lastGoodStreamId, StatusCode.InternalError);

        public const ushort Type = 7;

        /// <summary>
        /// Flags related to this frame. 
        /// </summary>
        private new Options Flags
        {
            set => base.Flags = (byte) value;
        }

        [Flags]
        public enum Options : byte
        {
            None = 0
        }

        /// <summary>
        /// This frame is always 8 bytes long.
        /// </summary>
        private UInt24 Length
        {
            get => UInt24.From(8);
            set
            {
                if (value.Value != 8)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(Length), "Length can only be 8");
                }
            }
        }

        /// <summary>
        /// The last stream id which was accepted by the sender of the GOAWAY message. If no streams were replied to, this value MUST be 0.
        /// </summary>
        public UInt31 LastGoodStreamId { get; }

        public static UInt31 NoLastGoodStreamId { get; } = UInt31.From(0);

        /// <summary>
        /// The reason for closing the session.
        /// </summary>
        public StatusCode Status { get; }

        public enum StatusCode : uint
        {
            /// <summary>
            /// This is a normal session teardown.
            /// </summary>
            Ok = 0,

            /// <summary>
            /// This is a generic error, and should only be used if a more specific error is not available.
            /// </summary>
            ProtocolError = 1,

            /// <summary>
            /// This is a generic error which can be used when the implementation has internally failed, not due to anything in the protocol.
            /// </summary>
            InternalError = 2
        }

        internal static async ValueTask<ReadResult<GoAway>> TryReadAsync(
            byte flags,
            UInt24 length,
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            var lastGoodStreamId =
                await frameReader.ReadUInt32Async(cancellation)
                                 .AsUInt31Async()
                                 .ConfigureAwait(false);
            var status = await frameReader.ReadUInt32Async(cancellation)
                                          .ToEnumAsync<StatusCode>()
                                          .ConfigureAwait(false);

            return ReadResult.Ok(new GoAway(
                flags.ToEnum<Options>(), length, lastGoodStreamId, status));
        }

        protected override async ValueTask WriteControlFrameAsync(
            IFrameWriter frameWriter,
            IHeaderWriterProvider _,
            CancellationToken cancellationToken = default)
        {
            await frameWriter.WriteUInt24Async(Length, cancellationToken)
                             .ConfigureAwait(false);
            await frameWriter.WriteUInt32Async(
                                 LastGoodStreamId, cancellationToken)
                             .ConfigureAwait(false);
            await frameWriter.WriteUInt32Async((uint) Status, cancellationToken)
                             .ConfigureAwait(false);
        }
    }
}