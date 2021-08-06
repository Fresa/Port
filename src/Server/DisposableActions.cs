using System;

namespace Port.Server
{
    internal class DisposableActions : IDisposable
    {
        private readonly Action[] _dispose;

        internal DisposableActions(
            params Action[] dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            foreach (var dispose in _dispose)
            {
                dispose();
            }
        }
    }
}