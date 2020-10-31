using System;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy.Helpers
{
    internal interface IWaiter : IDisposable
    {
        Task WaitAsync(
            CancellationToken cancellation = default);
    }
}