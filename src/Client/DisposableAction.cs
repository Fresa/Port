using System;

namespace Port.Client
{
    internal sealed class DisposableAction : IDisposable
    {
        private readonly Action _dispose;

        public DisposableAction(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            _dispose();
        }
    }
}