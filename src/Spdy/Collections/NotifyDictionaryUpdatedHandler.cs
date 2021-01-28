namespace Spdy.Collections
{
    public delegate void NotifyDictionaryUpdatedHandler<in TKey, in TValue>(
        TKey key,
        TValue value);
}