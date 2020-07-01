using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Kubernetes.Test.API.Server
{
    public sealed class PortForwardSocket : IAsyncDisposable
    {
        private readonly List<Task> _backgroundTasks = new List<Task>();
        private readonly CancellationTokenSource _cts;
        private readonly Pipe _sendPipe = new Pipe();
        private readonly Pipe _receivePipe = new Pipe();
        private int _closed = No;
        private const int Yes = 1;
        private const int No = 0;
        private readonly WebSocket _webSocket;

        public PortForwardSocket(
            IHostApplicationLifetime lifeTime,
            WebSocket webSocket)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(
                lifeTime.ApplicationStopping);
            _webSocket = webSocket;
            _backgroundTasks.Add(
                ReceiveUntilClosedOrApplicationStoppingAsync());
            _backgroundTasks.Add(
                SendUntilClosedOrApplicationStoppingAsync());
        }

        public PipeReader Reader => _receivePipe.Reader;
        public PipeWriter Writer => _sendPipe.Writer;

        private async Task ReceiveUntilClosedOrApplicationStoppingAsync()
        {
            var writer = _receivePipe.Writer;
            while (_webSocket.State.HasFlag(WebSocketState.Open))
            {
                try
                {
                    var received =
                        await _webSocket.ReceiveAsync(
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
                catch when (_webSocket.State != WebSocketState.Open ||
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

            await CloseAsync()
                .ConfigureAwait(false);
        }

        private async Task SendUntilClosedOrApplicationStoppingAsync()
        {
            var reader = _sendPipe.Reader;
            while (_webSocket.State.HasFlag(WebSocketState.Open))
            {
                try
                {
                    var readResult = await reader.ReadAsync(_cts.Token)
                        .ConfigureAwait(false);
                    if (readResult.IsCompleted || readResult.IsCanceled)
                    {
                        return;
                    }

                    foreach (var buffer in readResult.Buffer)
                    {
                        await _webSocket.SendAsync(
                                buffer, WebSocketMessageType.Binary, true,
                                _cts.Token)
                            .ConfigureAwait(false);
                        reader.AdvanceTo(
                            readResult.Buffer.GetPosition(buffer.Length));
                    }
                }
                catch when (_webSocket.State != WebSocketState.Open ||
                            _cts
                                .IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    await reader.CompleteAsync(ex)
                        .ConfigureAwait(false);
                    throw;
                }
            }

            await reader.CompleteAsync()
                .ConfigureAwait(false);

            await CloseAsync()
                .ConfigureAwait(false);
        }

        public async ValueTask CloseAsync()
        {
            _cts.Cancel(false);
            var closed = Interlocked.CompareExchange(ref _closed, Yes, No);
            if (closed == Yes)
            {
                return;
            }

            if (_webSocket.State == WebSocketState.Closed ||
                _webSocket.State == WebSocketState.Aborted)
            {
                return;
            }

            await _webSocket.CloseAsync(
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