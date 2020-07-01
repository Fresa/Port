using System.Net.WebSockets;
using Microsoft.Extensions.Hosting;

namespace Kubernetes.Test.API.Server
{
    public sealed class PortForwardSocketFactory
    {
        private readonly IHostApplicationLifetime _lifeTime;

        public PortForwardSocketFactory(
            IHostApplicationLifetime lifeTime)
        {
            _lifeTime = lifeTime;
        }

        internal PortForwardSocket Create(
            WebSocket webSocket)
        {
            return new PortForwardSocket(_lifeTime, webSocket);
        }
    }
}