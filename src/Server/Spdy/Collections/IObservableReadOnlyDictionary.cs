using System.Collections.Generic;

namespace Port.Server.Spdy.Collections
{
    internal interface
        IObservableReadOnlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey,
            TValue>
        where TKey : notnull
    {
        event NotifyDictionaryUpdatedHandler<TKey, TValue> Updated;
    }
}