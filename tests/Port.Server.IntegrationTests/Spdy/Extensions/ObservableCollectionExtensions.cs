using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using Port.Server.Spdy.Collections;

namespace Port.Server.IntegrationTests.Spdy.Extensions
{
    internal static class ObservableCollectionExtensions
    {
        internal static ISourceBlock<T> Subscribe<T>(
            this IObservableReadOnlyCollection<T> collection)
        {
            var buffer = new BufferBlock<T>();
            collection.Updated += item => { buffer.Post(item); };
            return buffer;
        }

        internal static ISourceBlock<(TKey, TValue)> Subscribe<TKey, TValue>(
            this IObservableReadOnlyDictionary<TKey, TValue> collection)
            where TKey : notnull
        {
            var buffer = new BufferBlock<(TKey, TValue)>();
            collection.Updated += (
                key,
                item) =>
            {
                buffer.Post((key, item));
            };
            return buffer;
        }
    }
}