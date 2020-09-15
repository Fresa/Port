using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.IntegrationTests.SocketTestFramework;
using Port.Server.Spdy;
using Port.Server.Spdy.Extensions;
using Port.Server.Spdy.Frames;

namespace Port.Server.IntegrationTests.Spdy
{
    internal sealed class FrameClient : IMessageClient<Frame>
    {
        private readonly INetworkClient _networkClient;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Task _receiverTask = Task.CompletedTask;
        private readonly Pipe _pipe = new Pipe();

        public FrameClient(
            INetworkClient networkClient)
        {
            _networkClient = networkClient;
            RunReceiver();
        }

        private void RunReceiver()
        {
            _receiverTask = Task.Run(
                async () =>
                {
                    try
                    {
                        FlushResult result;
                        do
                        {
                            var bytes = await _networkClient.ReceiveAsync(
                                                                _pipe
                                                                    .Writer
                                                                    .GetMemory(512),
                                                                _cancellationTokenSource
                                                                    .Token)
                                                            .ConfigureAwait(
                                                                false);
                            _pipe.Writer.Advance(bytes);
                            result = await _pipe
                                           .Writer.FlushAsync(
                                               _cancellationTokenSource.Token)
                                           .ConfigureAwait(false);
                        } while (_cancellationTokenSource
                                     .IsCancellationRequested ==
                                 false &&
                                 result.IsCompleted == false &&
                                 result.IsCanceled == false);
                    }
                    catch when (_cancellationTokenSource
                        .IsCancellationRequested)
                    {
                    }
                });
        }

        public async ValueTask<Frame> ReceiveAsync(
            CancellationToken cancellationToken = default)
        {
            return (await Frame.TryReadAsync(
                                   new FrameReader(_pipe.Reader),
                                   cancellationToken)
                               .ConfigureAwait(false)).Result;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                _cancellationTokenSource.Cancel(false);
            }
            catch
            {
                // Graceful shutdown
            }

            await _receiverTask.ConfigureAwait(false);
        }

        public async ValueTask SendAsync(
            Frame payload,
            CancellationToken cancellationToken = default)
        {
            await _networkClient.SendAsync(payload, cancellationToken)
                                .ConfigureAwait(false);
        }
    }
}