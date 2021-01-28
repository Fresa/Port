namespace Spdy.IntegrationTests.SocketTestFramework
{
    public interface IRespond<TMessage> : ISendingClient<TMessage>
    {
        TMessage Respond();
    }
}