using System;
using System.Text;

namespace Port.Server
{
    internal static class ReadOnlyMemoryExtensions
    {
        private const string ContentLengthKey = "Content-Length: ";

        private static readonly byte[] ContentLengthKeyAsBytes =
            Encoding.ASCII.GetBytes(ContentLengthKey);

        private static readonly int ContentLengthKeyLength =
            ContentLengthKey.Length;

        private const byte CR = 13;
        private const byte LF = 10;
        private static readonly byte[] EndOfHeaders = { CR, LF, CR, LF };

        internal static (int HeaderLength, int ContentLength)
            GetHttpResponseLength(
                this ReadOnlyMemory<byte> httpResponse)
        {
            var httpResponseContentLength = 0;
            var httpResponseHeaderLength = 0;
            for (var i = 0;
                i < httpResponse.Length - ContentLengthKeyLength;
                i++)
            {
                if (httpResponse.Slice(i, ContentLengthKeyLength)
                    .Span.SequenceCompareTo(ContentLengthKeyAsBytes) != 0)
                {
                    continue;
                }

                i += ContentLengthKeyLength;
                var start = i;
                while (httpResponse.Span[i]
                    .IsNumber())
                {
                    i++;
                }

                httpResponseContentLength = int.Parse(
                    Encoding.ASCII.GetString(
                        httpResponse[start..(i + 1)]
                            .Span));

                i += httpResponse.Slice(i)
                    .IndexOf(EndOfHeaders);

                httpResponseHeaderLength = i + EndOfHeaders.Length;
                break;
            }

            if (httpResponseContentLength == 0)
            {
                throw new InvalidOperationException(
                    $"Expected to find '{ContentLengthKey}'");
            }

            return (httpResponseHeaderLength, httpResponseContentLength);
        }

        internal static int IndexOf(
            this ReadOnlyMemory<byte> memory,
            byte[] bytes)
        {
            var i = 0;
            while (memory.Slice(i, bytes.Length)
                .Span.SequenceCompareTo(bytes) != 0)
            {
                i++;
            }

            return i;
        }
    }
}