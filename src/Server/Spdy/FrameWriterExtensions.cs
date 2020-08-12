using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy
{
    internal static class FrameWriterExtensions
    {
        internal static async ValueTask WriteNameValuePairs(
            this IFrameWriter frameWriter,
            IReadOnlyDictionary<string, string> nameValuePairs,
            CancellationToken cancellationToken)
        {
            if (!nameValuePairs.Any())
            {
                return;
            }

            await frameWriter.WriteInt32Async(
                    nameValuePairs.Count, cancellationToken)
                .ConfigureAwait(false);
            foreach (var (name, value) in nameValuePairs)
            {
                await frameWriter.WriteStringAsync(
                        name, Encoding.ASCII, cancellationToken)
                    .ConfigureAwait(false);
                await frameWriter.WriteStringAsync(
                        value, Encoding.ASCII, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}