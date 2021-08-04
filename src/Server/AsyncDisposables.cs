using System;
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
            foreach (var asyncDisposable in _disposables)
            {
                await asyncDisposable.DisposeAsync()
                                     .ConfigureAwait(false);
            }
        }
    }
}