using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    public interface IFrameReader
    {
        ValueTask<UInt24> ReadUInt24Async(
            CancellationToken cancellationToken = default);

        ValueTask<uint> ReadUInt32Async(
            CancellationToken cancellationToken = default);

        ValueTask<byte> ReadByteAsync(
            CancellationToken cancellationToken = default);

        ValueTask<byte> PeekByteAsync(
            CancellationToken cancellationToken = default);

        ValueTask<ushort> ReadUShortAsync(
            CancellationToken cancellationToken = default);
    }
}