using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Log.It;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    internal class FrameWriter : IFrameWriter, IAsyncDisposable
    {
        private readonly Stream _buffer;
        private readonly ILogger _logger = LogFactory.Create<FrameWriter>();

        public FrameWriter(
            Stream buffer)
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

        private ValueTask WriteAsync(
            byte[] value,
            CancellationToken cancellationToken = default)
        {
            if (value.Any() == false)
            {
                return new ValueTask();
            }
            
            _logger.Debug("Writing: {@value}", value);
            return _buffer.WriteAsync(value.AsMemory(), cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await _buffer.FlushAsync()
                .ConfigureAwait(false);
        }
    }
}