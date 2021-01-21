using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy;

namespace Port.Server.UnitTests.Spdy.Frames
{
    internal sealed class ExceptionThrowingHeaderReader : IHeaderReader
    {
        public Task<IFrameReader> RequestReaderAsync(
            int bytes,
            CancellationToken cancellationToken = default)
            => throw new System.NotImplementedException();
    }
}