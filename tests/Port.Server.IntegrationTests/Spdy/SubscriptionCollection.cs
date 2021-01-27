﻿using System;
using System.Collections.Generic;
using Port.Server.IntegrationTests.SocketTestFramework.Collections;

namespace Port.Server.IntegrationTests.Spdy
{
    internal sealed class SubscriptionCollection<TBase> where TBase : class
    {
        private readonly Dictionary<Type, object> _items = new Dictionary<Type, object>();

        internal void Add<T>(
            ISubscription<T> item) where T : TBase
        {
            _items.Add(typeof(T), item);
        }

        internal ISubscription<T> Get<T>() where T : TBase
        {
            return (ISubscription<T>)_items[typeof(T)];
        }
    }
}