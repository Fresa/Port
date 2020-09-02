using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy.Extensions
{
    internal static class FrameWriterExtensions
    {
        internal static async ValueTask WriteNameValuePairs(
            this IFrameWriter frameWriter,
            IReadOnlyDictionary<string, string[]> nameValuePairs,
            CancellationToken cancellationToken)
        {
            if (!nameValuePairs.Any())
            {
                return;
            }

            await frameWriter.WriteInt32Async(
                    nameValuePairs.Count, cancellationToken)
                .ConfigureAwait(false);
            foreach (var (name, values) in nameValuePairs)
            {
                await frameWriter.WriteStringAsync(
                        name, Encoding.ASCII, cancellationToken)
                    .ConfigureAwait(false);
                await frameWriter.WriteStringAsync(
                        string.Join('\0', values), Encoding.ASCII, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}