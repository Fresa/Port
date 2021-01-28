using System;
using System.Threading.Tasks;

namespace Spdy.Extensions
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
            return 
                (await value.ConfigureAwait(false))
                .ToEnum<T>();
        }

        internal static T ToEnum<T>(
            this byte value)
            where T : struct, Enum
        {
            return Enum.Parse<T>(value.ToString());
        }
    }
}