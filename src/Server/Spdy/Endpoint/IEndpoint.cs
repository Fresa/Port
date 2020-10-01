using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy.Endpoint
{
    public interface IEndpoint
    {
        bool IsOpen { get; }
        bool IsClosed { get; }
        Task WaitForOpenedAsync(CancellationToken cancellationToken = default);
        Task WaitForClosedAsync(CancellationToken cancellationToken = default);
    }
}