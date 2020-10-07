using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Linq;
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

        public async ValueTask<UInt24> ReadUInt24Async(
            CancellationToken cancellationToken = default)
        {
            var bytes = await ReadAsBigEndianAsync(3, cancellationToken)
                .ConfigureAwait(false);
            return new UInt24(bytes[0], bytes[1], bytes[2]);
        }

        public async ValueTask<int> ReadInt32Async(
            CancellationToken cancellationToken = default)
        {
            return BitConverter.ToInt32(
                await ReadAsBigEndianAsync(4, cancellationToken)
                    .ConfigureAwait(false));
        }

        public async ValueTask<uint> ReadUInt32Async(
            CancellationToken cancellationToken = default)
        {
            return
                BitConverter.ToUInt32(
                    await ReadAsBigEndianAsync(4, cancellationToken)
                        .ConfigureAwait(false));
        }

        public async ValueTask<byte> ReadByteAsync(
            CancellationToken cancellationToken = default)
        {
            var value = await ReadAsync(1, cancellationToken)
                .ConfigureAwait(false);
            return value.First();
        }

        public ValueTask<byte[]> ReadBytesAsync(int length,
            CancellationToken cancellationToken = default)
        {
            return ReadAsLittleEndianAsync(length, cancellationToken);
        }

        public async ValueTask<byte> PeekByteAsync(
            CancellationToken cancellationToken = default)
        {
            var value = await PeakAsync(1, cancellationToken)
                .ConfigureAwait(false);
            return value.First();
        }

        public async ValueTask<ushort> ReadUShortAsync(
            CancellationToken cancellationToken = default)
        {
            return
                BitConverter.ToUInt16(
                    await ReadAsBigEndianAsync(2, cancellationToken)
                        .ConfigureAwait(false));
        }

        public async ValueTask<byte[]> ReadStringAsync(
            CancellationToken cancellationToken = default)
        {
            var length = await ReadInt32Async(cancellationToken)
                .ConfigureAwait(false);
            return await ReadAsLittleEndianAsync(length, cancellationToken)
                .ConfigureAwait(false);
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
            if (length == 0)
            {
                return Array.Empty<byte>();
            }
            var sequence = await GetAsync(length, cancellationToken)
                .ConfigureAwait(false);
            var bytes = sequence.ToArray();
            _reader.AdvanceTo(sequence.GetPosition(length));
            
            return bytes;
        }

        private async ValueTask<byte[]> PeakAsync(
            int length,
            CancellationToken cancellationToken = default)
        {
            if (length == 0)
            {
                return Array.Empty<byte>();
            }
            var sequence = await GetAsync(length, cancellationToken)
                .ConfigureAwait(false);
            _reader.AdvanceTo(sequence.GetPosition(0));
            return sequence.ToArray();
        }

        private async ValueTask<ReadOnlySequence<byte>> GetAsync(
            int length,
            CancellationToken cancellationToken = default)
        {
            if (length <= 0)
            {
                return ReadOnlySequence<byte>.Empty;
            }

            System.IO.Pipelines.ReadResult result;
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

            return result.Buffer.Slice(0, length);
        }
    }
}