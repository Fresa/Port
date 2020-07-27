using System;

namespace Port.Server.Spdy
{
    public class GoAway : Control
    {
        public override short Type => 7;

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
    }
}