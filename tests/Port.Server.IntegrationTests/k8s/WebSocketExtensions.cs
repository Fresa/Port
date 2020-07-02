using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using k8s;

namespace Port.Server.IntegrationTests.k8s
{
    internal static class WebSocketExtensions
    {
        private static readonly ChannelIndex[] Channels =
            {ChannelIndex.StdIn, ChannelIndex.StdOut};

        internal static async Task SendPortAsync(
            this WebSocket webSocket,
            ushort port,
            CancellationToken cancellationToken = default)
        {
            // Port message: [channel][port high byte][port low byte]
            var high = (byte)(port >> 8);
            var low = (byte)(port & 0xff);
            foreach (var channel in Channels)
            {
                await webSocket.SendAsync(
                        new[] { (byte)channel, high, low },
                        WebSocketMessageType.Binary, true,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}