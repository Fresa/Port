using System;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server
{
    public interface IStreamForwarder : IAsyncDisposable
    {
        Task WaitUntilStoppedAsync(
            CancellationToken cancellation);
    }
}