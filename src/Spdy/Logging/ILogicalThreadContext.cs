namespace Spdy.Logging
{
    /// <summary>
    /// Transfer context over logical threads i.e. async local
    /// </summary>
    public interface ILogicalThreadContext
    {
        void Set<TValue>(string key, TValue value) where TValue : notnull;
        TValue? Get<TValue>(string key);
        void Remove(string key);
        bool Contains(string key);
    }
}