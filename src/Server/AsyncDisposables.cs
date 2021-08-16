using System;
using System.Threading.Tasks;
using Port.Server.Framework;

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
            await _disposables.DisposeAllAsync()
                              .ConfigureAwait(false);
        }
    }
}