using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spdy.Collections;
using Spdy.Frames.Readers;

namespace Spdy.Extensions
{
    internal static class FrameReaderExtensions
    {
        private static async ValueTask<NameValueHeaderBlock>
            ReadNameValuePairsAsync(
                this IFrameReader frameReader,
                CancellationToken cancellationToken)
        {
            var length = await frameReader.ReadInt32Async(cancellationToken)
                .ConfigureAwait(false);
            var nameValuePairs = new (string Name, string[] Values)[length];
            for (var i = 0; i < length; i++)
            {
                var name = Encoding.ASCII.GetString(
                    await frameReader.ReadStringAsync(cancellationToken)
                        .ConfigureAwait(false));
                var value = Encoding.ASCII.GetString(
                    await frameReader.ReadStringAsync(cancellationToken)
                        .ConfigureAwait(false));
                var values = value.Split(SpdyConstants.Nul);
                nameValuePairs[i] = (name, values);
            }

            return new NameValueHeaderBlock(nameValuePairs);
        }

        internal static async ValueTask<NameValueHeaderBlock> ReadNameValuePairsAsync(
                this IHeaderReader headerReader,
                int length,
                CancellationToken cancellationToken)
        {
            var reader = await headerReader.RequestReaderAsync(length, cancellationToken)
                       .ConfigureAwait(false);
            return await reader.ReadNameValuePairsAsync(cancellationToken)
                               .ConfigureAwait(false);
        }
    }
}