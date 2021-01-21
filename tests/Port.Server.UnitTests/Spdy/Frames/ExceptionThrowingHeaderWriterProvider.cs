using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy;

namespace Port.Server.UnitTests.Spdy.Frames
{
    internal sealed class ExceptionThrowingHeaderWriterProvider : IHeaderWriterProvider
    {
        public Task<IHeaderWriter> RequestWriterAsync(
            PipeWriter stream,
            CancellationToken cancellation = default)
            => throw new System.NotImplementedException();
    }
}