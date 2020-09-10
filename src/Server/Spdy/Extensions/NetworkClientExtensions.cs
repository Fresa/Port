using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Frames;

namespace Port.Server.Spdy.Extensions
{
    internal static class NetworkClientExtensions
    {
        internal static async Task SendAsync(
            this INetworkClient networkClient,
            Frame frame,
            CancellationToken cancellationToken = default)
        {
            await foreach (var bufferSequence in frame
                                                 .WriteAsync(cancellationToken)
                                                 .ConfigureAwait(false))
            {
                foreach (var buffer in bufferSequence)
                {
                    await networkClient.SendAsync(
                                           buffer,
                                           cancellationToken)
                                       .ConfigureAwait(false);
                }
            }
        }
    }
}