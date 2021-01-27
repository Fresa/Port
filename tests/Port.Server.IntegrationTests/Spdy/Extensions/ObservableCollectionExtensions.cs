using Port.Server.IntegrationTests.SocketTestFramework;
using Port.Server.IntegrationTests.SocketTestFramework.Collections;
using Port.Server.Spdy.Collections;

namespace Port.Server.IntegrationTests.Spdy.Extensions
{
    internal static class ObservableCollectionExtensions
    {
        internal static ISubscription<T> Subscribe<T>(
            this IObservableReadOnlyCollection<T> collection)
        {
            var buffer = new ConcurrentMessageBroker<T>();
            collection.Updated += item => { buffer.Send(item); };
            return buffer;
        }

        internal static ISubscription<(TKey, TValue)> Subscribe<TKey, TValue>(
            this IObservableReadOnlyDictionary<TKey, TValue> collection)
            where TKey : notnull
        {
            var buffer = new ConcurrentMessageBroker<(TKey, TValue)>();
            collection.Updated += (
                key,
                item) =>
            {
                buffer.Send((key, item));
            };
            return buffer;
        }
    }
}