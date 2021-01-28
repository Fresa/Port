using System.Collections.Generic;

namespace Spdy.Logging
{
    internal static class EnumerableExtensions
    {
        internal static string Join(this IEnumerable<string> strings, string separator)
        {
            return string.Join(separator, strings);
        }
    }
}