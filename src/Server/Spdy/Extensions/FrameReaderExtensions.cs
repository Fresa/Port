using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Collections;
using Port.Server.Spdy.Frames;

namespace Port.Server.Spdy.Extensions
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
                this IFrameReader frameReader,
                int length,
                CancellationToken cancellationToken)
        {
            return await
                (await frameReader
                       .ReadBytesAsync(length, cancellationToken)
                       .ConfigureAwait(false))
                .ZlibDecompress(SpdyConstants.HeadersDictionary)
                .ToFrameReader()
                .ReadNameValuePairsAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}