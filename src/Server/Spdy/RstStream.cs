using System;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Extensions;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    public class RstStream : Control
    {
        public RstStream(
            in byte flags,
            in UInt24 length,
            in UInt31 streamId,
            StatusCode status) : base(Type)
        {
            Flags = flags;
            Length = length;
            StreamId = streamId;
            Status = status;
        }

        public const ushort Type = 3;

        /// <summary>
        /// Flags related to this frame. RST_STREAM does not define any flags. This value must be 0.
        /// </summary>
        protected new byte Flags
        {
            get => base.Flags;
            set
            {
                if (value != 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(Flags),
                        "Flags can only be 0 = none");
                }

                base.Flags = value;
            }
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

        /// <summary>
        /// The 31-bit identifier for this stream.
        /// </summary>
        public UInt31 StreamId { get; }

        /// <summary>
        /// An indicator for why the stream is being terminated.
        /// </summary>
        public StatusCode Status { get; set; }

        internal static async ValueTask<RstStream> ReadAsync(
            byte flags,
            UInt24 length,
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            var streamId = UInt31.From(
                await frameReader.ReadUInt32Async(cancellation)
                    .ConfigureAwait(false) & 0x7FFF);
            var status =
                await frameReader.ReadUInt32Async(cancellation)
                    .ToEnumAsync<StatusCode>()
                    .ConfigureAwait(false);

            return new RstStream(flags, length, streamId, status);
        }

        public enum StatusCode
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
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}