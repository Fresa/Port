using System.Collections.Generic;

namespace Port.Server.Spdy.Collections
{
    public interface
        IObservableReadOnlyCollection<out T> : IReadOnlyCollection<T>
    {
        event NotifyCollectionUpdatedHandler<T> Updated;
    }
}