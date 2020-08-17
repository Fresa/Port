﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy.Frames
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