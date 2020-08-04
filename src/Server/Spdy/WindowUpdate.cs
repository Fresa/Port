using System;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy
{
    public class WindowUpdate : Control
    {
        public WindowUpdate(
            byte flags)
            : base(flags)
        {
        }

        public const ushort Type = 9;
        protected new byte Flags
        {
            get => 0;
            set
            {
                if (value != 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(Flags), "Flags can only be 0");
                }
            }
        }
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

        private int _deltaWindowSize;
        public int DeltaWindowSize
        {
            get => _deltaWindowSize;
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(DeltaWindowSize), "Delta window size must be greater than 0");
                }

                _deltaWindowSize = value;
            }
        }

        internal static async ValueTask<WindowUpdate> ReadAsync(
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }
    }
}