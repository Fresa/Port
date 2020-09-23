using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Port.Server.Spdy.Collections
{
    internal class
        ObservableConcurrentDictionary<TKey, TValue> :
            ConcurrentDictionary<TKey, TValue>, IObservableReadOnlyDictionary<TKey,
                TValue>, IObservableReadOnlyCollection<TValue>
        where TKey : notnull
    {
        public event NotifyDictionaryUpdatedHandler<TKey, TValue> Updated = (
            key,
            value) =>
        {
        };

        public new TValue this[TKey key]
        {
            get => base[key];
            set
            {
                base[key] = value;
                Updated.Invoke(key, value);
                UpdatedValue.Invoke(value);
            }
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => Values.GetEnumerator();

        private event NotifyCollectionUpdatedHandler<TValue> UpdatedValue = item => {};
        event NotifyCollectionUpdatedHandler<TValue>
            IObservableReadOnlyCollection<TValue>.Updated
            {
                add => UpdatedValue += value;
                remove => UpdatedValue -= value;
            }
    }
}