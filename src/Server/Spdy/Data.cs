using System;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Extensions;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    public class Data : Frame
    {
        protected override bool IsControlFrame => false;

        private int _streamId;

        public int StreamId
        {
            get => _streamId;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(StreamId), "Stream id must be 0 or greater");
                }

                _streamId = value;
            }
        }

        public bool IsLastFrame
        {
            get => Flags == 1;
            set => Flags = 1;
        }

        private byte Flags { get; set; }

        public UInt24 Length { get; set; }

        public byte[] Payload { get; set; } = new byte[0];

        public new async ValueTask WriteAsync(
            IFrameWriter frameWriter,
            CancellationToken cancellationToken = default)
        {
            var data = StreamId;
            data.SetBit(0, IsControlFrame);
            await frameWriter.WriteInt32Async(data, cancellationToken)
                .ConfigureAwait(false);

            await frameWriter.WriteByteAsync(Flags, cancellationToken)
                .ConfigureAwait(false);

            await frameWriter.WriteUInt24Async(Length, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}