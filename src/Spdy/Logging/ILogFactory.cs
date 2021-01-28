namespace Spdy.Logging
{
    public interface ILogFactory
    {
        ILogger Create(string logger);
        ILogger Create<T>();
    }
}