namespace Port.Server.IntegrationTests.SocketTestFramework
{
    public interface IRespond<TMessage> : ISendingClient<TMessage>
    {
        TMessage Respond();
    }
}