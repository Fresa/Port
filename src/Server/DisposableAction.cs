using System;

namespace Kubernetes.PortForward.Manager.Server
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