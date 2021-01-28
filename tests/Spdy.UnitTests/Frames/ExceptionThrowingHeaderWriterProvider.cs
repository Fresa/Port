using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Spdy.Frames.Writers;

namespace Spdy.UnitTests.Frames
{
    internal sealed class ExceptionThrowingHeaderWriterProvider : IHeaderWriterProvider
    {
        public Task<IHeaderWriter> RequestWriterAsync(
            PipeWriter stream,
            CancellationToken cancellation = default)
            => throw new System.NotImplementedException();
    }
}