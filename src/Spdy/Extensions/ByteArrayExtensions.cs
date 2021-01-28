using System.Linq;

namespace Spdy.Extensions
{
    internal static class ByteArrayExtensions
    {
        internal static string ToHexArrayRepresentation(
            this byte[] buffer)
        {
            return buffer
                .Aggregate(
                    "{ ", (
                            @byte,
                            representation)
                        => @byte + $"0x{representation:X2}, ")
                .TrimEnd()
                .TrimEnd(',') + " }";
        }
    }
}