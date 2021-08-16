using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server
{
    internal sealed class OrderedDisposableStreamForwarderDecorator : IStreamForwarder
    {
        private readonly IStreamForwarder _streamForwarder;
        private readonly IAsyncDisposable _disposable;

        public OrderedDisposableStreamForwarderDecorator(IStreamForwarder streamForwarder, params IAsyncDisposable[] disposables)
        {
            _streamForwarder = streamForwarder;
            var asyncDisposables = new List<IAsyncDisposable>
            {
                streamForwarder
            };
            asyncDisposables.AddRange(disposables);
            _disposable = new AsyncDisposables(asyncDisposables.ToArray());
        }

        public ValueTask DisposeAsync()
            => _disposable.DisposeAsync();

        public Task WaitUntilStoppedAsync(
            CancellationToken cancellation)
            => _streamForwarder.WaitUntilStoppedAsync(cancellation);
    }
}