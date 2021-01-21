using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy
{
    internal interface IHeaderReader
    {
        Task<IFrameReader> RequestReaderAsync(
            int bytes,
            CancellationToken cancellationToken = default);
    }
}