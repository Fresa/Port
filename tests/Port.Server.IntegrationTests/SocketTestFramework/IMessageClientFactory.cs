namespace Port.Server.IntegrationTests.SocketTestFramework
{
    internal interface IMessageClientFactory<T>
    {
        IMessageClient<T> Create(
            INetworkClient networkClient);
    }
}