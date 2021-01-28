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
    /// The PING control frame is a mechanism for measuring a minimal round-trip time from the sender. It can be sent from the client or the server. Recipients of a PING frame should send an identical frame to the sender as soon as possible (if there is other pending data waiting to be sent, PING should take highest priority). Each ping sent by a sender should use a unique ID.
    /// 
    /// +----------------------------------+
    /// |1|   version    |         6       |
    /// +----------------------------------+
    /// | 0 (flags) |     4 (length)       |
    /// +----------------------------------|
    /// |            32-bit ID             |
    /// +----------------------------------+
    /// </summary>
    public class Ping : Control
    {
        private Ping(
            Options flags,
            UInt24 length,
            uint id) : this(id)
        {
            Flags = flags;
            Length = length;
        }

        public Ping(
            uint id) : base(Type)
        {
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
            get => UInt24.From(4);
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

        internal static async ValueTask<ReadResult<Ping>> TryReadAsync(
            byte flags,
            UInt24 length,
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            var id = await frameReader.ReadUInt32Async(cancellation)
                .ConfigureAwait(false);

            return ReadResult.Ok(new Ping(flags.ToEnum<Options>(), length, id));
        }

        protected override async ValueTask WriteControlFrameAsync(
            IFrameWriter frameWriter,
            IHeaderWriterProvider _,
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