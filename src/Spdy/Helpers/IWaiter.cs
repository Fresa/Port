using System;
using System.Threading;
using System.Threading.Tasks;

namespace Spdy.Helpers
{
    internal interface IWaiter : IDisposable
    {
        Task WaitAsync(
            CancellationToken cancellation = default);
    }
}