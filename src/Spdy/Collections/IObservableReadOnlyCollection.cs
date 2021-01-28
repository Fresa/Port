using System.Collections.Generic;

namespace Spdy.Collections
{
    public interface
        IObservableReadOnlyCollection<out T> : IReadOnlyCollection<T>
    {
        event NotifyCollectionUpdatedHandler<T> Updated;
    }
}