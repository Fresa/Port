using System;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy
{
    public class RstStream : Control
    {
        public const ushort Key = 3;
        protected override ushort Type => Key;

        protected new uint Length
        {
            get => 8;
            set
            {
                if (value != 8)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(Length), "Length can only be 8");
                }
            }
        }

        public int StreamId { get; set; }
        public StatusCode Status { get; set; }

        internal static async ValueTask<RstStream> ReadAsync(
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {

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
    }
}