﻿namespace Port.Server.IntegrationTests.SocketTestFramework
{
    internal sealed class ByteArrayMessageClientFactory : IMessageClientFactory<byte[]>
    {
        public IMessageClient<byte[]> Create(
            INetworkClient networkClient)
        {
            return new ByteArrayMessageClient(networkClient);
        }
    }
}