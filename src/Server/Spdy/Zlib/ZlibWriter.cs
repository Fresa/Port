using System;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ionic.Zlib;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy.Zlib
{
    internal sealed class ZlibWriter : IFrameWriter, IAsyncDisposable
    {
        private readonly FrameWriter _frameWriter;
        private readonly Stream _outputStream;
        private readonly byte[] _dictionary;
        private readonly Pipe _pipe = new Pipe();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ValueTask _backgroundTask;

        private CancellationToken CancellationToken
            => _cancellationTokenSource.Token;

        public ZlibWriter(
            Stream outputStream,
            byte[] dictionary)
        {
            _frameWriter = new FrameWriter(_pipe.Writer.AsStream());
            _outputStream = outputStream;
            _dictionary = dictionary;

            _backgroundTask = RunZlibCompress();
        }

        private CancellationToken LinkCancellationTokens(
            CancellationToken cancellationToken)
            => CancellationTokenSource.CreateLinkedTokenSource(
                                          cancellationToken, CancellationToken)
                                      .Token;

        public ValueTask WriteUInt24Async(
            UInt24 value,
            CancellationToken cancellationToken = default)
            => _frameWriter.WriteUInt24Async(
                value, LinkCancellationTokens(cancellationToken));

        public ValueTask WriteInt32Async(
            int value,
            CancellationToken cancellationToken = default)
            => _frameWriter.WriteInt32Async(
                value, LinkCancellationTokens(cancellationToken));

        public ValueTask WriteUInt32Async(
            uint value,
            CancellationToken cancellationToken = default)
            => _frameWriter.WriteUInt32Async(
                value, LinkCancellationTokens(cancellationToken));

        public ValueTask WriteByteAsync(
            byte value,
            CancellationToken cancellationToken = default)
            => _frameWriter.WriteByteAsync(
                value, LinkCancellationTokens(cancellationToken));

        public ValueTask WriteBytesAsync(
            byte[] value,
            CancellationToken cancellationToken = default)
            => _frameWriter.WriteBytesAsync(
                value, LinkCancellationTokens(cancellationToken));

        public ValueTask WriteUShortAsync(
            ushort value,
            CancellationToken cancellationToken = default)
            => _frameWriter.WriteUShortAsync(
                value, LinkCancellationTokens(cancellationToken));

        public ValueTask WriteStringAsync(
            string value,
            Encoding encoding,
            CancellationToken cancellationToken = default)
            => _frameWriter.WriteStringAsync(
                value, encoding, LinkCancellationTokens(cancellationToken));

        private async ValueTask RunZlibCompress()
        {
            var zlibCodec = new ZlibCodec();
            var buffer = new byte[1024];
            zlibCodec.OutputBuffer = buffer;

            var result = zlibCodec.InitializeDeflate();
            if (result < 0)
            {
                throw new InvalidOperationException(
                    $"Got error code {result} when initializing deflate routine: {zlibCodec.Message}");
            }
            result = zlibCodec.SetDictionary(_dictionary);
            if (result < 0)
            {
                throw new InvalidOperationException(
                    $"Got error code {result} when setting dictionary: {zlibCodec.Message}");
            }

            Exception? exception = null;
            try
            {
                System.IO.Pipelines.ReadResult inputBuffer;
                do
                {
                    inputBuffer = await _pipe
                                        .Reader.ReadAsync(CancellationToken)
                                        .ConfigureAwait(false);
                    foreach (var input in inputBuffer.Buffer)
                    {
                        zlibCodec.NextIn = 0;
                        zlibCodec.InputBuffer = input.ToArray();
                        zlibCodec.AvailableBytesIn = input.Length;

                        while (zlibCodec.AvailableBytesIn > 0)
                        {
                            zlibCodec.NextOut = 0;
                            zlibCodec.AvailableBytesOut = buffer.Length;

                            var start = zlibCodec.NextOut;
                            result = zlibCodec.Deflate(FlushType.None);
                            if (result < 0)
                            {
                                throw new InvalidOperationException(
                                    $"Got error code {result} when deflating the stream: {zlibCodec.Message}");
                            }

                            var length = zlibCodec.NextOut - start;

                            await _outputStream.WriteAsync(
                                                   buffer, start, length,
                                                   CancellationToken)
                                               .ConfigureAwait(false);
                        }
                    }
                    _pipe.Reader.AdvanceTo(inputBuffer.Buffer.End);
                } while (inputBuffer.HasMoreData());

                zlibCodec.InputBuffer = Array.Empty<byte>();
                zlibCodec.NextIn = 0;
                zlibCodec.AvailableBytesIn = 0;
                zlibCodec.NextOut = 0;
                zlibCodec.AvailableBytesOut = buffer.Length;
                var bufferStart = zlibCodec.NextOut;

                result = zlibCodec.Deflate(FlushType.Sync);
                if (result != ZlibConstants.Z_OK)
                {
                    throw new InvalidOperationException($"Expected OK, got {result}");
                }
                result = zlibCodec.Deflate(FlushType.Finish);
                if (result != ZlibConstants.Z_STREAM_END)
                {
                    throw new InvalidOperationException($"Expected END, got {result}");
                }
                var bufferLength = zlibCodec.NextOut - bufferStart;

                await _outputStream.WriteAsync(
                                       buffer, bufferStart, bufferLength,
                                       CancellationToken)
                                   .ConfigureAwait(false);
                await _outputStream.FlushAsync(CancellationToken)
                                   .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                zlibCodec.EndDeflate();
                await _pipe.Reader.CompleteAsync(exception)
                           .ConfigureAwait(false);
            }
        }

        public async ValueTask DisposeAsync()
        {
            // flush the pipe writer
            await _frameWriter.DisposeAsync()
                              .ConfigureAwait(false);
            await _pipe.Writer.CompleteAsync()
                       .ConfigureAwait(false);
            await _backgroundTask
                .ConfigureAwait(false);

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}