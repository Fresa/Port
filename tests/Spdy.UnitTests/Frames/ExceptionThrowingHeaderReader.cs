using System.Threading;
using System.Threading.Tasks;
using Spdy.Frames.Readers;

namespace Spdy.UnitTests.Frames
{
    internal sealed class ExceptionThrowingHeaderReader : IHeaderReader
    {
        public Task<IFrameReader> RequestReaderAsync(
            int bytes,
            CancellationToken cancellationToken = default)
            => throw new System.NotImplementedException();
    }
}