using System;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    public class Ping : Control
    {
        public Ping(
            Options flags,
            UInt24 length,
            uint id) : base(Type)
        {
            Flags = flags;
            Length = length;
            Id = id;
        }

        public const ushort Type = 6;

        /// <summary>
        /// Flags related to this frame. 
        /// </summary>
        protected new Options Flags
        {
            get => (Options)base.Flags;
            private set => base.Flags = (byte)value;
        }

        [Flags]
        public enum Options : byte
        {
            None = 0
        }

        /// <summary>
        /// This frame is always 4 bytes long.
        /// </summary>
        public UInt24 Length
        {
            get => UInt24.From(8);
            private set
            {
                if (value.Value != 4)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(Length), "Length can only be 4");
                }
            }
        }

        /// <summary>
        /// A unique ID for this ping, represented as an unsigned 32 bit value.
        /// </summary>
        public uint Id { get; }

        internal static async ValueTask<Ping> ReadAsync(
            byte flags,
            UInt24 length,
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            var id = await frameReader.ReadUInt32Async(cancellation)
                .ConfigureAwait(false);

            return new Ping(flags.ToEnum<Options>(), length, id);
        }

        protected override async ValueTask WriteControlFrameAsync(
            IFrameWriter frameWriter,
            CancellationToken cancellationToken = default)
        {
            await frameWriter.WriteUInt24Async(
                    Length, cancellationToken)
                .ConfigureAwait(false);
            await frameWriter.WriteUInt32Async(
                    Id, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}