using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace Spdy.Frames.Writers
{
    public interface IHeaderWriterProvider
    {
        Task<IHeaderWriter> RequestWriterAsync(
            PipeWriter stream,
            CancellationToken cancellation = default);
    }
}