using System;

namespace Port.Server.IntegrationTests.SocketTestFramework
{
    public interface IMessageClient : ISendingClient, IReceivingClient,
        IAsyncDisposable
    {
    }
}