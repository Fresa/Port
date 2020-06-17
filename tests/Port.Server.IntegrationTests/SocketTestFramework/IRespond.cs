namespace Port.Server.IntegrationTests.SocketTestFramework
{
    public interface IRespond<out TMessage> : ISendingClient
    {
        TMessage Respond();
    }
}