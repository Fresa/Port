using System;
using System.Collections.Concurrent;

namespace Spdy.Collections
{
    internal sealed class ConcurrentDistinctTypeBag
    {
        private readonly ConcurrentDictionary<Type, Type> _types = new ConcurrentDictionary<Type, Type>();

        internal bool TryAdd<T>()
        {
            return _types.TryAdd(typeof(T), typeof(T));
        }

        internal bool Contains<T>()
        {
            return _types.ContainsKey(typeof(T));
        }
    }
}