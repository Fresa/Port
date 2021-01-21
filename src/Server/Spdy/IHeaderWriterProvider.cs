using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy
{
    public interface IHeaderWriterProvider
    {
        Task<IHeaderWriter> RequestWriterAsync(
            PipeWriter stream,
            CancellationToken cancellation = default);
    }
}