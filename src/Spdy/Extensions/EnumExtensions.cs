using System;

namespace Spdy.Extensions
{
    internal static class EnumExtensions
    {
        internal static string GetName(
            this Enum @enum)
            => Enum.GetName(@enum.GetType(), @enum) ??
               throw new InvalidOperationException(
                   $"{@enum} of type {@enum.GetType()} does not have a name");
    }
}