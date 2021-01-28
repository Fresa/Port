using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Ionic.Zlib;
using Spdy.Extensions;

namespace Spdy.Frames.Writers
{
    internal sealed class HeaderWriterProvider : IHeaderWriterProvider, IAsyncDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ValueTask _backgroundTask;
        private readonly ZlibCodec _zlibCodec = new ZlibCodec();

        private CancellationToken CancellationToken
            => _cancellationTokenSource.Token;

        private readonly BufferBlock<(PipeReader OutputReader, PipeWriter OutputWriter, bool last)> _requestQueue = new BufferBlock<(PipeReader OutputReader, PipeWriter OutputWriter, bool last)>();

        public HeaderWriterProvider()
        {
            _backgroundTask = RunZlibCompress();
        }

        private async Task<IHeaderWriter> RequestWriterAsync(
            PipeWriter outputWriter,
            bool last = false,
            CancellationToken cancellation = default)
        {
            var pipe = new Pipe();
            await _requestQueue.SendAsync((OutputReader: pipe.Reader, OutputWriter: outputWriter, last), cancellation)
                               .ConfigureAwait(false);
            return new HeaderWriter(pipe.Writer);
        }

        public Task<IHeaderWriter> RequestLastWriterAsync(
            PipeWriter stream,
            CancellationToken cancellation = default)
            => RequestWriterAsync(stream, true, cancellation);

        public Task<IHeaderWriter> RequestWriterAsync(
            PipeWriter stream,
            CancellationToken cancellation = default)
            => RequestWriterAsync(stream, false, cancellation);

        private async ValueTask RunZlibCompress()
        {
            var buffer = new byte[1024];
            _zlibCodec.OutputBuffer = buffer;

            var result = _zlibCodec.InitializeDeflate(CompressionLevel.Default);
            if (result < 0)
            {
                throw new InvalidOperationException(
                    $"Got error code {result} when initializing deflate routine: {_zlibCodec.Message}");
            }
            result = _zlibCodec.SetDictionary(SpdyConstants.HeadersDictionary);
            if (result < 0)
            {
                throw new InvalidOperationException(
                    $"Got error code {result} when setting dictionary: {_zlibCodec.Message}");
            }

            Exception? exception = null;
            try
            {
                while (true)

                {
                    var (inputReader, outputWriter, last) = await _requestQueue
                        .ReceiveAsync(CancellationToken)
                        .ConfigureAwait(false);
                    try
                    {
                        System.IO.Pipelines.ReadResult inputBuffer;
                        do
                        {
                            inputBuffer = await inputReader
                                                .ReadAsync(CancellationToken)
                                                .ConfigureAwait(false);

                            foreach (var input in inputBuffer.Buffer)
                            {
                                _zlibCodec.NextIn = 0;
                                _zlibCodec.InputBuffer = input.ToArray();
                                _zlibCodec.AvailableBytesIn = input.Length;

                                while (_zlibCodec.AvailableBytesIn > 0)
                                {
                                    await WriteToAsync(outputWriter, FlushType.None)
                                        .ConfigureAwait(false);
                                }
                            }

                            inputReader.AdvanceTo(inputBuffer.Buffer.End);

                        } while (inputBuffer.HasMoreData());

                        await WriteToAsync(outputWriter, FlushType.Sync)
                            .ConfigureAwait(false);

                        if (last)
                        {
                            await WriteToAsync(outputWriter, FlushType.Finish)
                                .ConfigureAwait(false);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        return;
                    }
                    finally
                    {
                        await outputWriter.CompleteAsync(exception)
                                          .ConfigureAwait(false);
                        await inputReader.CompleteAsync(exception)
                                         .ConfigureAwait(false);
                    }
                }
            }
            catch when (CancellationToken.IsCancellationRequested)
            {

            }
            finally
            {
                _requestQueue.Complete();
                _zlibCodec.EndDeflate();
            }

            async ValueTask WriteToAsync(PipeWriter writer, FlushType flushType)
            {
                _zlibCodec.NextOut = 0;
                _zlibCodec.AvailableBytesOut = buffer.Length;

                var start = _zlibCodec.NextOut;
                result = _zlibCodec.Deflate(flushType);
                if (result < 0)
                {
                    throw new InvalidOperationException(
                        $"Got error code {result} when deflating the stream: {_zlibCodec.Message}");
                }

                if (flushType == FlushType.Finish && result != ZlibConstants.Z_STREAM_END)
                {
                    throw new InvalidOperationException($"Expected END, got {result}");
                }

                var length = _zlibCodec.NextOut - start;
                var data = buffer[start..(start + length)];
                await writer.WriteAsync(
                                data,
                                CancellationToken)
                            .ConfigureAwait(false);
                await writer.FlushAsync(CancellationToken)
                            .ConfigureAwait(false);
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await _backgroundTask
                .ConfigureAwait(false);

            _cancellationTokenSource.Dispose();
        }
    }
}