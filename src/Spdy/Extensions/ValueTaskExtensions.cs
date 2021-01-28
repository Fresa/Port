using System;
using System.Threading.Tasks;

namespace Spdy.Extensions
{
    internal static class ValueTaskExtensions
    {
        public static async ValueTask<T> ContinueWith<T>(this ValueTask<T> source,
            Action<ValueTask<T>> continuationAction)
        {
            try
            {
                return await source.ConfigureAwait(false);
            }
            finally
            {
                continuationAction(source);
            }
        }
    }
}