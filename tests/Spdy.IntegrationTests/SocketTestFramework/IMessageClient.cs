namespace Spdy.IntegrationTests.SocketTestFramework
{
    public interface IMessageClient<T> : IReceivingClient<T>, ISendingClient<T>
    {

    }
}