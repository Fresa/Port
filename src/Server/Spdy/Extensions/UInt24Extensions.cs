using System;
using System.Threading.Tasks;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy.Extensions
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