using System;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy
{
    public abstract class Control : Frame
    {
        protected sealed override bool IsControlFrame => true;
        public short Version { get; } = 3;
        protected abstract ushort Type { get; }
        protected byte Flags { get; set; }
        protected uint Length { get; set; }
        protected byte[] Data { get; set; } = new byte[0];

        internal new static async ValueTask<Control> ReadAsync(
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            var version =
                (ushort) (await frameReader.ReadUShortAsync(cancellation)
                    .ConfigureAwait(false) & 0x7FFF);
            if (version != 3)
            {
                throw new InvalidOperationException(
                    $"Version {version} is not supported. Supported version is 3.");
            }

            var type = await frameReader.ReadUShortAsync(cancellation)
                .ConfigureAwait(false);
            return type switch
            {
                SynStream.Key => await SynStream.ReadAsync(
                        frameReader, cancellation)
                    .ConfigureAwait(false),
                SynReply.Key => await SynReply.ReadAsync(
                        frameReader, cancellation)
                    .ConfigureAwait(false),
                RstStream.Key => await RstStream.ReadAsync(
                        frameReader, cancellation)
                    .ConfigureAwait(false),
                Settings.Key => await Settings.ReadAsync(
                        frameReader, cancellation)
                    .ConfigureAwait(false),
                Ping.Key => await Ping.ReadAsync(frameReader, cancellation)
                    .ConfigureAwait(false),
                GoAway.Key => await SynStream.ReadAsync(
                        frameReader, cancellation)
                    .ConfigureAwait(false),
                Headers.Key => await Headers.ReadAsync(
                        frameReader, cancellation)
                    .ConfigureAwait(false),
                WindowUpdate.Key => await WindowUpdate.ReadAsync(
                        frameReader, cancellation)
                    .ConfigureAwait(false),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(type), $"Unknown control frame type {type} received")
            };
        }
    }
}