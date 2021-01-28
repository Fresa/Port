using System.Threading;
using System.Threading.Tasks;

namespace Spdy.Frames.Readers
{
    internal interface IHeaderReader
    {
        Task<IFrameReader> RequestReaderAsync(
            int bytes,
            CancellationToken cancellationToken = default);
    }
}