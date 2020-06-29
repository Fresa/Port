using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Kubernetes.Test.API.Server
{
    public sealed class WebSocketReceiver : IAsyncDisposable
    {
        private readonly List<Task> _backgroundTasks = new List<Task>();
        private readonly CancellationTokenSource _cts;

        public WebSocketReceiver(
            IHostApplicationLifetime lifeTime)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(
                lifeTime.ApplicationStopping);
        }

        public PipeReader Start(
            WebSocket webSocket)
        {
            var pipe = new Pipe();
            _backgroundTasks.Add(
                WaitForCloseOrApplicationStoppingAsync(webSocket, pipe.Writer));
            return pipe.Reader;
        }

        private async Task WaitForCloseOrApplicationStoppingAsync(
            WebSocket webSocket,
            PipeWriter writer)
        {
            while (webSocket.State.HasFlag(WebSocketState.Open))
            {
                try
                {
                    var received =
                        await webSocket.ReceiveAsync(
                                writer.GetMemory(), _cts.Token)
                            .ConfigureAwait(false);
                    writer.Advance(received.Count);

                    var result = await writer
                        .FlushAsync(_cts.Token)
                        .ConfigureAwait(false);

                    if (result.IsCanceled ||
                        result.IsCompleted)
                    {
                        break;
                    }

                    if (received.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                }
                catch when (webSocket.State != WebSocketState.Open ||
                            _cts
                                .IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    await writer.CompleteAsync(ex)
                        .ConfigureAwait(false);
                    throw;
                }
            }

            await writer.CompleteAsync()
                .ConfigureAwait(false);

            if (webSocket.State == WebSocketState.Closed ||
                webSocket.State == WebSocketState.Aborted)
            {
                return;
            }

            await webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Socket closed",
                    _cts.Token)
                .ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel(false);
            await Task.WhenAll(_backgroundTasks);
        }
    }
}