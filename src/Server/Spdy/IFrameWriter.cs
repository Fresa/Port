using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    public interface IFrameWriter
    {
        ValueTask WriteBooleanAsync(
            bool value,
            CancellationToken cancellationToken = default);

        ValueTask WriteUInt24Async(
            UInt24 streamId,
            CancellationToken cancellationToken = default);
        
        ValueTask WriteInt32Async(
            int streamId,
            CancellationToken cancellationToken = default);

        ValueTask WriteByteAsync(
            byte value,
            CancellationToken cancellationToken = default);

        ValueTask WriteBytesAsync(
            byte[] value,
            CancellationToken cancellationToken = default);
    }
}