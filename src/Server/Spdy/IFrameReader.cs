using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    public interface IFrameReader
    {
        ValueTask<bool> ReadBooleanAsync(
            CancellationToken cancellationToken = default);

        ValueTask<UInt24> ReadUInt24Async(
            CancellationToken cancellationToken = default);
    }
}