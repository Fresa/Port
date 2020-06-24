using System;

namespace Port.Server.IntegrationTests.SocketTestFramework
{
    public interface IMessageClient<T> : IReceivingClient<T>, ISendingClient<T>
    {

    }
}