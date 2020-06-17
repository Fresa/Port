using System;

namespace Port.Server.IntegrationTests.SocketTestFramework
{
    public interface IMessage
    {
        Type Type { get; }
    }
}