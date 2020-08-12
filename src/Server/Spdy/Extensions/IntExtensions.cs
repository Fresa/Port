using System;

namespace Port.Server.Spdy.Extensions
{
    internal static class IntExtensions
    {
        internal static T ToEnum<T>(
            this int value)
            where T : struct, Enum
        {
            return Enum.Parse<T>(value.ToString());
        }
    }
}