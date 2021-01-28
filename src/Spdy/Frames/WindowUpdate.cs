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
    /// The WINDOW_UPDATE frame is used to implement flow control.
    /// 
    /// Flow control operates at two levels: on each individual stream and on the entire connection.
    /// 
    /// Both types of flow control are hop by hop; that is, only between the two endpoints. Intermediaries do not forward WINDOW_UPDATE frames between dependent connections. However, throttling of data transfer by any receiver can indirectly cause the propagation of flow control information toward the original sender.
    /// 
    /// Flow control only applies to frames that are identified as being subject to flow control. Of the frame types defined in this document, this includes only DATA frame. Frames that are exempt from flow control MUST be accepted and processed, unless the receiver is unable to assign resources to handling the frame. A receiver MAY respond with a stream error (Section 2.4.2) or session error (Section 2.4.1) of type FLOW_CONTROL_ERROR if it is unable accept a frame.
    /// 
    /// +----------------------------------+
    /// |1|   version    |         9       |
    /// +----------------------------------+
    /// | 0 (flags) |     8 (length)       |
    /// +----------------------------------+
    /// |X|     Stream-ID (31-bits)        |
    /// +----------------------------------+
    /// |X|  Delta-Window-Size (31-bits)   |
    /// +----------------------------------+
    /// </summary>
    public class WindowUpdate : Control
    {
        private WindowUpdate(
            Options flags,
            UInt24 length,
            UInt31 streamId,
            UInt31 deltaWindowSize)
            : this(streamId, deltaWindowSize)
        {
            Flags = flags;
            Length = length;
        }

        public WindowUpdate(
            UInt31 streamId,
            UInt31 deltaWindowSize)
            : base(Type)
        {
            StreamId = streamId;
            DeltaWindowSize = deltaWindowSize;
        }

        private static readonly UInt31 ConnectionFlowId = UInt31.From(0);

        public static WindowUpdate ConnectionFlowControl(
            UInt31 deltaWindowSize)
        {
            return new WindowUpdate(ConnectionFlowId, deltaWindowSize);
        }

        public const ushort Type = 9;

        /// <summary>
        /// The flags field is always zero.
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

        private UInt24 Length
        {
            get => UInt24.From(8);
            set
            {
                if (value.Value != 8)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(Length), $"Length can only be 8, was {value}");
                }
            }
        }

        /// <summary>
        /// The stream ID for which this WINDOW_UPDATE control frame applies to, or 0 if applied to connection-level flow control.
        /// </summary>
        public UInt31 StreamId { get; }

        public bool IsStreamFlowControl => !IsConnectionFlowControl;
        public bool IsConnectionFlowControl => StreamId == ConnectionFlowId;

        private UInt31 _deltaWindowSize;

        /// <summary>
        /// The additional number of bytes that the sender can transmit in addition to existing remaining window size. The legal range for this field is 1 to 2^31 - 1 (0x7fffffff) bytes.
        /// </summary>
        public UInt31 DeltaWindowSize
        {
            get => _deltaWindowSize;
            private set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(DeltaWindowSize),
                        "Delta window size must be greater than 0");
                }

                _deltaWindowSize = value;
            }
        }

        internal static async ValueTask<ReadResult<WindowUpdate>> TryReadAsync(
            byte flags,
            UInt24 length,
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            var streamId =
                await frameReader.ReadUInt32Async(cancellation)
                                 .AsUInt31Async()
                                 .ConfigureAwait(false);
            var deltaWindowSize =
                await frameReader.ReadUInt32Async(cancellation)
                                 .AsUInt31Async()
                                 .ConfigureAwait(false);

            return ReadResult.Ok(new WindowUpdate(
                flags.ToEnum<Options>(), length, streamId, deltaWindowSize));
        }

        protected override async ValueTask WriteControlFrameAsync(
            IFrameWriter frameWriter,
            IHeaderWriterProvider _,
            CancellationToken cancellationToken = default)
        {
            await frameWriter.WriteUInt24Async(Length, cancellationToken)
                             .ConfigureAwait(false);
            await frameWriter.WriteUInt32Async(
                                 StreamId, cancellationToken)
                             .ConfigureAwait(false);
            await frameWriter.WriteUInt32Async(
                                 DeltaWindowSize, cancellationToken)
                             .ConfigureAwait(false);
        }
    }
}