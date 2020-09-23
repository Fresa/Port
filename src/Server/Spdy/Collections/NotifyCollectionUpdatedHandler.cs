namespace Port.Server.Spdy.Collections
{
    public delegate void NotifyCollectionUpdatedHandler<in T>(
        T item);
}