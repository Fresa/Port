using System;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    public class WindowUpdate : Control
    {
        public WindowUpdate(
            byte flags,
            UInt24 length,
            UInt31 streamId,
            UInt31 deltaWindowSize)
        {
            Flags = flags;
            Length = length;
            StreamId = streamId;
            DeltaWindowSize = deltaWindowSize;
        }

        public const ushort Type = 9;

        /// <summary>
        /// The flags field is always zero.
        /// </summary>
        private new byte Flags
        {
            get => 0;
            set
            {
                if (value != 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(Flags), "Flags can only be 0");
                }

                base.Flags = value;
            }
        }
        private new UInt24 Length
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
        /// The stream ID for which this WINDOW_UPDATE control frame applies to, or 0 if applied to connection-level flow control.
        /// </summary>
        public UInt31 StreamId { get; }

        public bool IsConnectionLevelFlowControl => StreamId == UInt31.From(0);
        
        private UInt31 _deltaWindowSize;
        /// <summary>
        /// The additional number of bytes that the sender can transmit in addition to existing remaining window size. The legal range for this field is 1 to 2^31 - 1 (0x7fffffff) bytes.
        /// </summary>
        public UInt31 DeltaWindowSize
        {
            get => _deltaWindowSize;
            private set
            {
                if (value.Value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(DeltaWindowSize), "Delta window size must be greater than 0");
                }

                _deltaWindowSize = value;
            }
        }

        internal static async ValueTask<WindowUpdate> ReadAsync(
            byte flags,
            UInt24 length,
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            var streamId = UInt31.From(
                await frameReader.ReadUInt32Async(cancellation)
                    .ConfigureAwait(false) & 0x7FFF);
            var deltaWindowSize = UInt31.From(
                await frameReader.ReadUInt32Async(cancellation)
                    .ConfigureAwait(false) & 0x7FFF);

            return new WindowUpdate(flags, length, streamId, deltaWindowSize);
        }

        protected override async ValueTask WriteFrameAsync(
            IFrameWriter frameWriter,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
