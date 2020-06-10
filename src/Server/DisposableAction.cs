using System;

namespace Port.Server
{
    internal class DisposableAction : IDisposable
    {
        private readonly Action _dispose;

        internal DisposableAction(
            Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            _dispose();
        }
    }
}