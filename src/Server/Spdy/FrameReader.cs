using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    public class FrameReader : IFrameReader
    {
        private readonly PipeReader _reader;

        public FrameReader(
            PipeReader reader)
        {
            _reader = reader;
        }

        public async ValueTask<bool> ReadBooleanAsync(
            CancellationToken cancellationToken = default)
        {
            return
                BitConverter.ToBoolean(
                    await ReadAsLittleEndianAsync(1, cancellationToken)
                        .ConfigureAwait(false),
                    0);
        }

        public async ValueTask<UInt24> ReadUInt24Async(
            CancellationToken cancellationToken = default)
        {
            var bytes = await ReadAsync(3, cancellationToken)
                .ConfigureAwait(false);
            return new UInt24(bytes[2], bytes[1], bytes[0]);
        }

        private async ValueTask<byte[]> ReadAsLittleEndianAsync(
            int length,
            CancellationToken cancellationToken = default)
        {
            var bytes = await ReadAsync(length, cancellationToken)
                .ConfigureAwait(false);
            if (BitConverter.IsLittleEndian == false)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        private async ValueTask<byte[]> ReadAsBigEndianAsync(
            int length,
            CancellationToken cancellationToken = default)
        {
            var bytes = await ReadAsync(length, cancellationToken)
                .ConfigureAwait(false);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        private async ValueTask<byte[]> ReadAsync(
            int length,
            CancellationToken cancellationToken = default)
        {
            if (length <= 0)
            {
                return Array.Empty<byte>();
            }

            ReadResult result;
            do
            {
                result = await _reader.ReadAsync(cancellationToken)
                    .ConfigureAwait(false);
            } while (result.Buffer.Length < length &&
                     result.IsCanceled == false &&
                     result.IsCompleted == false);

            if (result.Buffer.Length < length)
            {
                throw new InvalidOperationException(
                    $"Expected {length} bytes, got {result.Buffer.Length}");
            }

            var bytes = result.Buffer.Slice(0, length).ToArray();
            _reader.AdvanceTo(result.Buffer.GetPosition(length));

            return bytes;
        }
    }
}