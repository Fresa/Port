using System;
using System.Threading.Tasks;

namespace Port.Server
{
    internal static class ByteExtensions
    {
        internal static bool IsNumber(
            this byte @byte)
        {
            return @byte >= 48 &&
                   @byte <= 57;
        }

        internal static async ValueTask<T> ToEnumAsync<T>(
            this ValueTask<byte> value)
            where T : struct, Enum
        {
            return Enum.Parse<T>(
                (await value.ConfigureAwait(false))
                .ToString());
        }
    }
}