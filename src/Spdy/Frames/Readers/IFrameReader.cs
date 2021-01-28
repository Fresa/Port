using System.Threading;
using System.Threading.Tasks;
using Spdy.Primitives;

namespace Spdy.Frames.Readers
{
    public interface IFrameReader
    {
        ValueTask<UInt24> ReadUInt24Async(
            CancellationToken cancellationToken = default);

        ValueTask<int> ReadInt32Async(
            CancellationToken cancellationToken = default);

        ValueTask<uint> ReadUInt32Async(
            CancellationToken cancellationToken = default);

        ValueTask<byte> ReadByteAsync(
            CancellationToken cancellationToken = default);
        ValueTask<byte[]> ReadBytesAsync(
            int length,
            CancellationToken cancellationToken = default);

        ValueTask<byte> PeekByteAsync(
            CancellationToken cancellationToken = default);

        ValueTask<ushort> ReadUShortAsync(
            CancellationToken cancellationToken = default);

        ValueTask<byte[]> ReadStringAsync(
            CancellationToken cancellationToken = default);
    }
}