using System;
using Spdy.Helpers;

namespace Spdy.Logging
{
    internal static class LogContextExtensions
    {
        public static IDisposable Capture<T>(this ILogicalThreadContext logContext, string key, T value) where T : notnull
        {
            logContext.Set(key, value);
            return new DisposableAction(() => logContext.Remove(key));
        }
    }
}