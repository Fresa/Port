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
            collection.Updated += item =>
            {
                buffer.Post(item);
            };
            return buffer;
        }
    }
}