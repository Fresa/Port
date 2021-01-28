using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Spdy.Frames;
using Spdy.Frames.Readers;
using Spdy.Frames.Writers;
using Spdy.Helpers;
using Spdy.IntegrationTests.SocketTestFramework;
using Spdy.Network;

namespace Spdy.IntegrationTests
{
    internal sealed class FrameClient : IMessageClient<Frame>
    {
        private readonly INetworkClient _networkClient;

        private readonly CancellationTokenSource _cancellationTokenSource =
            new CancellationTokenSource();

        private CancellationToken ReceiverCancellationToken
            => _cancellationTokenSource.Token;

        private Task _receiverTask = Task.CompletedTask;
        private readonly Pipe _pipe = new Pipe(new PipeOptions(useSynchronizationContext: false));
        private readonly FrameReader _frameReader;
        private readonly FrameWriter _frameWriter;
        private readonly HeaderWriterProvider _headerWriterProvider;
        private readonly IHeaderReader _headerReader;

        private readonly SemaphoreSlimGate _frameReaderGate =
            SemaphoreSlimGate.OneAtATime;

        public FrameClient(
            INetworkClient networkClient)
        {
            _networkClient = networkClient;
            _frameReader = new FrameReader(_pipe.Reader);
            _frameWriter = new FrameWriter(networkClient);
            _headerWriterProvider = new HeaderWriterProvider();
            _headerReader = new HeaderReader(_frameReader);
            RunReceiver();
        }

        private void RunReceiver()
        {
            _receiverTask = Task.Run(
                async () =>
                {
                    Exception? exception = null;
                    try
                    {
                        FlushResult result;
                        do
                        {
                            var bytes = await _networkClient.ReceiveAsync(
                                    _pipe
                                        .Writer
                                        .GetMemory(),
                                    ReceiverCancellationToken)
                                .ConfigureAwait(false);
                            // End of the stream! 
                            // https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.sockettaskextensions.receiveasync?view=netcore-3.1
                            if (bytes == 0)
                            {
                                _cancellationTokenSource.Cancel(false);
                                return;
                            }
                            _pipe.Writer.Advance(bytes);
                            result = await _pipe
                                           .Writer.FlushAsync(
                                               ReceiverCancellationToken)
                                           .ConfigureAwait(false);
                        } while (ReceiverCancellationToken
                                     .IsCancellationRequested ==
                                 false &&
                                 result.IsCompleted == false &&
                                 result.IsCanceled == false);
                    }
                    catch when (ReceiverCancellationToken
                        .IsCancellationRequested)
                    {
                    }
                    catch (Exception exceptionCaught)
                    {
                        exception = exceptionCaught;
                    }
                    finally
                    {
                        await _pipe.Writer.CompleteAsync(exception)
                                   .ConfigureAwait(false);
                    }
                });
        }

        public async ValueTask<Frame> ReceiveAsync(
            CancellationToken cancellationToken = default)
        {
            var linkedCancellationToken = CatchReceiverCancellation(cancellationToken);
            using (await _frameReaderGate.WaitAsync(linkedCancellationToken)
                                         .ConfigureAwait(false))
            {
                return (await Frame.TryReadAsync(
                                       _frameReader,
                                       _headerReader,
                                       linkedCancellationToken)
                                   .ConfigureAwait(false)).Result;
            }
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
            _frameReaderGate.Dispose();
            await _networkClient.DisposeAsync()
                                .ConfigureAwait(false);
        }

        public ValueTask SendAsync(
            Frame payload,
            CancellationToken cancellationToken = default)
            => payload.WriteAsync(_frameWriter, _headerWriterProvider, CatchReceiverCancellation(cancellationToken));

        private CancellationToken CatchReceiverCancellation(
            CancellationToken cancellationToken)
            => CancellationTokenSource
               .CreateLinkedTokenSource(
                   cancellationToken,
                   ReceiverCancellationToken)
               .Token;
    }
}