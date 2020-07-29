using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    internal class FrameWriter : IFrameWriter
    {
        private readonly Stream _buffer;

        public FrameWriter(
            Stream buffer)
        {
            _buffer = buffer;
        }

        public async ValueTask WriteUInt24Async(
            UInt24 value,
            CancellationToken cancellationToken = default)
        {
            await WriteAsync(
                new[]
                {
                    value.Three,
                    value.Two,
                    value.One
                }, cancellationToken)
                .ConfigureAwait(false);
        }

        public async ValueTask WriteUInt32Async(
            uint value,
            CancellationToken cancellationToken = default)
        {
            await WriteAsBigEndianAsync(
                    BitConverter.GetBytes(value), cancellationToken)
                .ConfigureAwait(false);
        }

        public async ValueTask WriteByteAsync(
            byte value,
            CancellationToken cancellationToken)
        {
            await WriteAsBigEndianAsync(new[] {value}, cancellationToken)
                .ConfigureAwait(false);
        }

        public async ValueTask WriteBytesAsync(
            byte[] value,
            CancellationToken cancellationToken = default)
        {
            await WriteAsLittleEndianAsync(value, cancellationToken)
                .ConfigureAwait(false);
        }

        private async ValueTask WriteAsLittleEndianAsync(
            byte[] value,
            CancellationToken cancellationToken = default)
        {
            if (BitConverter.IsLittleEndian == false)
            {
                Array.Reverse(value);
            }

            await WriteAsync(value, cancellationToken)
                .ConfigureAwait(false);
        }

        private async ValueTask WriteAsBigEndianAsync(
            byte[] value,
            CancellationToken cancellationToken = default)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(value);
            }

            await WriteAsync(value, cancellationToken)
                .ConfigureAwait(false);
        }

        private async ValueTask WriteAsync(
            byte[] value,
            CancellationToken cancellationToken = default)
        {
            if (value.Any() == false)
            {
                return;
            }

            await _buffer.WriteAsync(value.AsMemory(), cancellationToken)
                .ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await _buffer.FlushAsync()
                .ConfigureAwait(false);
        }
    }
}