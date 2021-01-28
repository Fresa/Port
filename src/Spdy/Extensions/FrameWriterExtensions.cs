using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spdy.Frames.Writers;

namespace Spdy.Extensions
{
    internal static class FrameWriterExtensions
    {
        internal static async ValueTask WriteNameValuePairs(
            this IFrameWriter frameWriter,
            IReadOnlyDictionary<string, string[]> nameValuePairs,
            CancellationToken cancellationToken)
        {
            await frameWriter.WriteInt32Async(
                    nameValuePairs.Count, cancellationToken)
                .ConfigureAwait(false);
            foreach (var (name, values) in nameValuePairs)
            {
                await frameWriter
                      .WriteStringAsync(name, Encoding.ASCII, cancellationToken)
                      .ConfigureAwait(false);
                await frameWriter
                      .WriteStringAsync(
                          string.Join(SpdyConstants.Nul, values),
                          Encoding.ASCII, cancellationToken)
                      .ConfigureAwait(false);
            }
        }
    }
}