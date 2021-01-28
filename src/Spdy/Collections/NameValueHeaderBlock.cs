using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Spdy.Collections
{
    public sealed class NameValueHeaderBlock : ReadOnlyDictionary<string, string[]>
    {
        public NameValueHeaderBlock(
            params (string Name, string[] Values)[] headers)
            : base(headers.ToDictionary(header => header.Name, pair => pair.Values))
        {
            foreach (var (name, values) in headers)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentException($"Header name '{name}' cannot be empty");
                }

                if (name.Any(char.IsUpper))
                {
                    throw new ArgumentException($"Header name '{name}' must be lower case");
                }

                foreach (var value in values)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        throw new ArgumentException($"Header value '{value}' cannot be empty");
                    }

                    if (value.StartsWith(SpdyConstants.Nul))
                    {
                        throw new ArgumentException(
                            $"Header value '{value}' cannot start with NUL ('{SpdyConstants.Nul}')");
                    }

                    if (value.EndsWith(SpdyConstants.Nul))
                    {
                        throw new ArgumentException(
                            $"Header value '{value}' cannot end with NUL ('{SpdyConstants.Nul}')");
                    }
                }
            }
        }
    }
}