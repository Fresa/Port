using System;
using Log.It;

namespace Port.Server
{
    internal static class LogContextExtensions
    {
        internal static IDisposable With<T>(
            this ILogContext context,
            string key,
            T value)
        {
            context.Set(key, value);

            return new DisposableAction(() => context.Remove(key));
        }
    }
}