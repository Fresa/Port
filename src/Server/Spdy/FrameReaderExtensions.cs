using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy
{
    internal static class FrameReaderExtensions
    {
        internal static async ValueTask<IReadOnlyDictionary<string, string>>
            ReadNameValuePairs(
                this IFrameReader frameReader,
                CancellationToken cancellationToken)
        {
            var nameValuePairs = new Dictionary<string, string>();
            var length = await frameReader.ReadInt32Async(cancellationToken)
                .ConfigureAwait(false);
            for (var i = 0; i < length; i++)
            {
                var name = Encoding.ASCII.GetString(
                    await frameReader.ReadStringAsync(cancellationToken)
                        .ConfigureAwait(false));
                var value = Encoding.ASCII.GetString(
                    await frameReader.ReadStringAsync(cancellationToken)
                        .ConfigureAwait(false));
                nameValuePairs.Add(name, value);
            }

            return nameValuePairs;
        }
    }
}