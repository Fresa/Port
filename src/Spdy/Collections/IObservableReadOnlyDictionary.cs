using System.Collections.Generic;

namespace Spdy.Collections
{
    public interface
        IObservableReadOnlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey,
            TValue>
        where TKey : notnull
    {
        event NotifyDictionaryUpdatedHandler<TKey, TValue> Updated;
    }
}