namespace Port.Server.IntegrationTests.SocketTestFramework
{
    internal interface IMessageClientFactory
    {
        IMessageClient Create(
            INetworkClient networkClient);
    }
}