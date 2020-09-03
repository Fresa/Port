using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy.Frames
{
    /// <summary>
    /// +----------------------------------+
    /// |C| Version(15bits) | Type(16bits) |
    /// +----------------------------------+
    /// | Flags (8)  |  Length (24 bits)   |
    /// +----------------------------------+
    /// |               Data               |
    /// +----------------------------------+
    /// </summary>
    public abstract class Control : Frame
    {
        private readonly ushort _type;

        protected Control(ushort type)
        {
            _type = type;
        }

        public const ushort Version = 3;

        /// <summary>
        /// Flags related to this frame. Flags for control frames and data frames are different.
        /// </summary>
        protected byte Flags { get; set; }

        protected sealed override async ValueTask WriteFrameAsync(
            IFrameWriter frameWriter,
            CancellationToken cancellationToken = default)
        {
            await frameWriter.WriteUShortAsync(
                    Version ^ 0x8000, cancellationToken)
                .ConfigureAwait(false);
            await frameWriter.WriteUShortAsync(_type, cancellationToken)
                .ConfigureAwait(false);
            await frameWriter.WriteByteAsync(Flags, cancellationToken)
                .ConfigureAwait(false);

            await WriteControlFrameAsync(frameWriter, cancellationToken)
                .ConfigureAwait(false);
        }

        protected abstract ValueTask WriteControlFrameAsync(
            IFrameWriter frameWriter,
            CancellationToken cancellationToken = default);
        
        internal new static async ValueTask<ReadResult<Frame>> TryReadAsync(
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            var version =
                (ushort) (await frameReader.ReadUShortAsync(cancellation)
                    .ConfigureAwait(false) & 0x7FFF);
            if (version != Version)
            {
                // todo: What stream id should be specified here?
                return ReadResult<Control>.Error(RstStream.UnsupportedVersion(UInt31.From(0)));
            }

            var type = await frameReader.ReadUShortAsync(cancellation)
                .ConfigureAwait(false);
            var flags = await frameReader.ReadByteAsync(cancellation)
                .ConfigureAwait(false);
            var length = await frameReader.ReadUInt24Async(cancellation)
                .ConfigureAwait(false);
            
            return type switch
            {
                SynStream.Type => await SynStream.TryReadAsync(
                        flags, length, frameReader, cancellation)
                    .ConfigureAwait(false),
                SynReply.Type => await SynReply.TryReadAsync(
                        flags, length, frameReader, cancellation)
                    .ConfigureAwait(false),
                RstStream.Type => await RstStream.TryReadAsync(
                        flags, length, frameReader, cancellation)
                    .ConfigureAwait(false),
                Settings.Type => await Settings.TryReadAsync(
                        flags, length, frameReader, cancellation)
                    .ConfigureAwait(false),
                Ping.Type => await Ping.TryReadAsync(
                        flags, length, frameReader, cancellation)
                    .ConfigureAwait(false),
                GoAway.Type => await GoAway.TryReadAsync(
                        flags, length, frameReader, cancellation)
                    .ConfigureAwait(false),
                Headers.Type => await Headers.TryReadAsync(
                        flags, length, frameReader, cancellation)
                    .ConfigureAwait(false),
                WindowUpdate.Type => await WindowUpdate.TryReadAsync(
                        flags, length, frameReader, cancellation)
                    .ConfigureAwait(false),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(type), $"Unknown control frame type {type} received")
            };
        }
    }
}