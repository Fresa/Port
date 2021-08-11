using System;
using System.Linq;
using System.Threading.Tasks;

namespace Port.Server
{
    internal sealed class AsyncDisposables : IAsyncDisposable
    {
        private readonly IAsyncDisposable[] _disposables;

        public AsyncDisposables(params IAsyncDisposable[] disposables)
        {
            _disposables = disposables;
        }

        public async ValueTask DisposeAsync()
        {
            await Task.WhenAll(
                _disposables.Select(forwarder => forwarder.DisposeAsync())
                            .Where(
                                valueTask => !valueTask.IsCompletedSuccessfully)
                            .Select(valueTask => valueTask.AsTask()));
        }
    }
}