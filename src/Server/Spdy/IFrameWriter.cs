using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    public interface IFrameWriter
    {
        ValueTask WriteUInt24Async(
            UInt24 value,
            CancellationToken cancellationToken = default);

        ValueTask WriteInt32Async(
            int value,
            CancellationToken cancellationToken = default);
        
        ValueTask WriteUInt32Async(
            uint value,
            CancellationToken cancellationToken = default);

        ValueTask WriteByteAsync(
            byte value,
            CancellationToken cancellationToken = default);

        ValueTask WriteBytesAsync(
            byte[] value,
            CancellationToken cancellationToken = default);

        ValueTask WriteUShortAsync(
            ushort value,
            CancellationToken cancellationToken = default);

        ValueTask WriteStringAsync(
            string value,
            Encoding encoding,
            CancellationToken cancellationToken = default);
    }
}