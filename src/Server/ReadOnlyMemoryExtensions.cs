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

        internal static bool
            TryGetHttpResponseLength(
                this ReadOnlyMemory<byte> httpResponse, out int headerLength, out int contentLength)
        {
            contentLength = 0;
            headerLength = 0;
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

                contentLength = int.Parse(
                    Encoding.ASCII.GetString(
                        httpResponse[start..(i + 1)]
                            .Span));

                i += httpResponse.Slice(i)
                    .IndexOf(EndOfHeaders);

                headerLength = i + EndOfHeaders.Length;
                break;
            }

            return contentLength != 0;
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