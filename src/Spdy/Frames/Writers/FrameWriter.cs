using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spdy.Network;
using Spdy.Primitives;

namespace Spdy.Frames.Writers
{
    internal class FrameWriter : IFrameWriter
    {
        private readonly INetworkClient _buffer;

        public FrameWriter(
            INetworkClient buffer)
        {
            _buffer = buffer;
        }

        public ValueTask WriteUInt24Async(
            UInt24 value,
            CancellationToken cancellationToken = default)
            => WriteAsync(
                new[]
                {
                    value.Three,
                    value.Two,
                    value.One
                }, cancellationToken);

        public ValueTask WriteInt32Async(
            int value,
            CancellationToken cancellationToken = default)
            => WriteAsBigEndianAsync(
                BitConverter.GetBytes(value), cancellationToken);

        public ValueTask WriteUInt32Async(
            uint value,
            CancellationToken cancellationToken = default)
            => WriteAsBigEndianAsync(
                BitConverter.GetBytes(value), cancellationToken);

        public ValueTask WriteByteAsync(
            byte value,
            CancellationToken cancellationToken)
            => WriteAsync(new[] {value}, cancellationToken);

        public ValueTask WriteBytesAsync(
            byte[] value,
            CancellationToken cancellationToken = default)
            => WriteAsync(value, cancellationToken);

        public ValueTask WriteUShortAsync(
            ushort value,
            CancellationToken cancellationToken = default)
            => WriteAsBigEndianAsync(
                BitConverter.GetBytes(value), cancellationToken);

        public async ValueTask WriteStringAsync(
            string value,
            Encoding encoding,
            CancellationToken cancellationToken = default)
        {
            var bytes = encoding.GetBytes(value);
            await WriteInt32Async(bytes.Length, cancellationToken)
                .ConfigureAwait(false);
            await WriteBytesAsync(bytes, cancellationToken)
                .ConfigureAwait(false);
        }
        
        private ValueTask WriteAsBigEndianAsync(
            byte[] value,
            CancellationToken cancellationToken = default)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(value);
            }

            return WriteAsync(value, cancellationToken);
        }

        private async ValueTask WriteAsync(
            byte[] value,
            CancellationToken cancellationToken = default)
        {
            if (value.Any() == false)
            {
                return;
            }
            
            await _buffer.SendAsync(value.AsMemory(), cancellationToken)
                         .ConfigureAwait(false);
        }
    }
}