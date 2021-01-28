using System;
using System.Threading.Tasks;
using Spdy.Primitives;

namespace Spdy.Extensions
{
    internal static class UInt24Extensions
    {
        internal static async ValueTask<T> ToEnumAsync<T>(
            this ValueTask<UInt24> value)
            where T : struct, Enum
        {
            return Enum.Parse<T>(
                (await value.ConfigureAwait(false))
                .ToString());
        }
    }
}