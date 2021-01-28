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

        internal sealed override async ValueTask WriteAsync(
            IFrameWriter frameWriter,
            IHeaderWriterProvider headerWriterProvider,
            CancellationToken cancellationToken = default)
        {
            await frameWriter.WriteUShortAsync(
                    Version ^ 0x8000, cancellationToken)
                .ConfigureAwait(false);
            await frameWriter.WriteUShortAsync(_type, cancellationToken)
                .ConfigureAwait(false);
            await frameWriter.WriteByteAsync(Flags, cancellationToken)
                .ConfigureAwait(false);

            await WriteControlFrameAsync(frameWriter, headerWriterProvider, cancellationToken)
                .ConfigureAwait(false);
        }

        protected abstract ValueTask WriteControlFrameAsync(
            IFrameWriter frameWriter,
            IHeaderWriterProvider headerWriter,
            CancellationToken cancellationToken = default);
        
        internal new static async ValueTask<ReadResult<Control>> TryReadAsync(
            IFrameReader frameReader,
            IHeaderReader headerReader,
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
                SynStream.Type => (await SynStream.TryReadAsync(
                        flags, length, frameReader, headerReader, cancellation)
                    .ConfigureAwait(false)).AsControl(),
                SynReply.Type => (await SynReply.TryReadAsync(
                        flags, length, frameReader, headerReader, cancellation)
                    .ConfigureAwait(false)).AsControl(),
                RstStream.Type => (await RstStream.TryReadAsync(
                        flags, length, frameReader, cancellation)
                    .ConfigureAwait(false)).AsControl(),
                Settings.Type => (await Settings.TryReadAsync(
                        flags, length, frameReader, cancellation)
                    .ConfigureAwait(false)).AsControl(),
                Ping.Type => (await Ping.TryReadAsync(
                        flags, length, frameReader, cancellation)
                    .ConfigureAwait(false)).AsControl(),
                GoAway.Type => (await GoAway.TryReadAsync(
                        flags, length, frameReader, cancellation)
                    .ConfigureAwait(false)).AsControl(),
                Headers.Type => (await Headers.TryReadAsync(
                        flags, length, frameReader, headerReader, cancellation)
                    .ConfigureAwait(false)).AsControl(),
                WindowUpdate.Type => (await WindowUpdate.TryReadAsync(
                        flags, length, frameReader, cancellation)
                    .ConfigureAwait(false)).AsControl(),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(type), $"Unknown control frame type {type} received")
            };
        }
    }
}