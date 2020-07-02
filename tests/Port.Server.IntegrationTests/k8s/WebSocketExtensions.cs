using System;
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

        internal static async Task HandleClosing(
            this WebSocket webSocket,
            CancellationToken cancellationToken,
            Func<Task> action)
        {
            try
            {
                await action().ConfigureAwait(false);
                
                if (webSocket.State != WebSocketState.Closed &&
                    webSocket.State != WebSocketState.Aborted)
                {
                    await webSocket.CloseOutputAsync(
                            WebSocketCloseStatus
                                .NormalClosure,
                            "Close received",
                            cancellationToken)
                        .ConfigureAwait(false);
                }
            }
            catch when (cancellationToken
                .IsCancellationRequested)
            {
                await webSocket.CloseOutputAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Socket closed",
                        CancellationToken.None)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (webSocket.State != WebSocketState.Closed &&
                    webSocket.State != WebSocketState.Aborted)
                {
                    await webSocket.CloseAsync(
                            WebSocketCloseStatus
                                .InternalServerError,
                            $"Error: {ex.Message}",
                            cancellationToken)
                        .ConfigureAwait(false);
                }

                throw;
            }
        }
    }
}