using System;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy
{
    public class GoAway : Control
    {
        public GoAway(
            byte flags)
        {
        }

        public const ushort Type = 7;

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

        public int LastGoodStreamId { get; set; }
        public StatusCode Status { get; set; }

        public enum StatusCode
        {
            Ok = 0,
            ProtocolError = 1,
            InternalError = 2
        }

        internal static async ValueTask<GoAway> ReadAsync(
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }
    }
}