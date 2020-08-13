using System;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Extensions;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    public class GoAway : Control
    {
        private GoAway(
            GoAwayFlags flags,
            UInt24 length,
            UInt31 lastGoodStreamId,
            StatusCode status)
            : base(Type)
        {
            Flags = flags;
            Length = length;
            LastGoodStreamId = lastGoodStreamId;
            Status = status;
        }

        public GoAway(
            UInt24 length,
            UInt31 lastGoodStreamId,
            StatusCode status)
            : this(
                GoAwayFlags.None,
                length,
                lastGoodStreamId,
                status)
        {
        }

        public const ushort Type = 7;

        /// <summary>
        /// Flags related to this frame. 
        /// </summary>
        private new GoAwayFlags Flags
        {
            set => base.Flags = (byte) value;
        }

        public enum GoAwayFlags : byte
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

        /// <summary>
        /// The reason for closing the session.
        /// </summary>
        public StatusCode Status { get; }

        public enum StatusCode
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

        internal static async ValueTask<GoAway> ReadAsync(
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

            return new GoAway(flags.ToEnum<GoAwayFlags>(), length, lastGoodStreamId, status);
        }

        protected override async ValueTask WriteControlFrameAsync(
            IFrameWriter frameWriter,
            CancellationToken cancellationToken = default)
        {
            await frameWriter.WriteUInt24Async(Length, cancellationToken)
                .ConfigureAwait(false);
            await frameWriter.WriteUInt32Async(
                    LastGoodStreamId.Value, cancellationToken)
                .ConfigureAwait(false);
            await frameWriter.WriteUInt32Async((uint) Status, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}