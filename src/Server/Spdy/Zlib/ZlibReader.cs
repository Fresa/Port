using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Ionic.Zlib;
using Log.It;
using Port.Server.Spdy.Extensions;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy.Zlib
{
    internal sealed class ZlibReader : IFrameReader, IAsyncDisposable
    {
        private readonly IFrameReader _frameReader;
        private readonly byte[] _dictionary;
        private readonly int _length;
        private readonly Pipe _pipe = new Pipe();
        private readonly FrameReader _headerReader;
        private readonly ZlibCodec _zlibCodec = new ZlibCodec();
        private readonly ILogger _logger = LogFactory.Create<ZlibReader>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ValueTask _backgroundTask;

        private CancellationToken CancellationToken
            => _cancellationTokenSource.Token;

        public ZlibReader(
            IFrameReader frameReader,
            byte[] dictionary,
            int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), $"{length} is an invalid length of a Zlib stream");
            }

            _frameReader = frameReader;
            _headerReader = new FrameReader(_pipe.Reader);
            _dictionary = dictionary;
            _length = length;

            _backgroundTask = RunZlibDecompress();
        }

        private CancellationToken LinkCancellationTokens(
            CancellationToken cancellationToken)
            => CancellationTokenSource.CreateLinkedTokenSource(
                                          cancellationToken, CancellationToken)
                                      .Token;

        private void SetResponse<T>(
            ValueTask<T> valueTask, TaskCompletionSource<T> task)
        {
            try
            {
                if (_backgroundTask.IsFaulted)
                {
                    task.SetException(
                        _backgroundTask.AsTask()
                                       .Exception ?? new Exception("Unknown error"));
                    return;
                }

                if (valueTask.IsCanceled)
                {
                    task.SetCanceled();
                    return;
                }

                if (valueTask.IsFaulted)
                {
                    task.SetException(
                        valueTask.AsTask()
                                 .Exception ?? new Exception("Unknown error"));
                    return;
                }

                if (valueTask.IsCompleted)
                {
                    task.SetResult(valueTask.Result);
                    return;
                }
                
                task.SetCanceled();
            }
            catch (Exception e)
            {
                task.SetException(e);
            }
        }

        public ValueTask<UInt24> ReadUInt24Async(
            CancellationToken cancellationToken = default)
        {
            var taskCompletionSource = new TaskCompletionSource<UInt24>();
            _ = _headerReader.ReadUInt24Async(
                                 LinkCancellationTokens(cancellationToken))
                             .ContinueWith(
                                 valueTask => SetResponse(valueTask, taskCompletionSource));

            return new ValueTask<UInt24>(taskCompletionSource.Task);
        }

        public ValueTask<int> ReadInt32Async(
            CancellationToken cancellationToken = default)
        {
            var taskCompletionSource = new TaskCompletionSource<int>();
            _ = _headerReader.ReadInt32Async(
                                 LinkCancellationTokens(cancellationToken))
                             .ContinueWith(
                                 valueTask => SetResponse(valueTask, taskCompletionSource));
            return new ValueTask<int>(taskCompletionSource.Task);
        }

        public ValueTask<uint> ReadUInt32Async(
            CancellationToken cancellationToken = default)
        {
            var taskCompletionSource = new TaskCompletionSource<uint>();
            _ = _headerReader.ReadUInt32Async(
                                 LinkCancellationTokens(cancellationToken))
                             .ContinueWith(
                                 valueTask => SetResponse(valueTask, taskCompletionSource));
            return new ValueTask<uint>(taskCompletionSource.Task);
        }

        public ValueTask<byte> ReadByteAsync(
            CancellationToken cancellationToken = default)
        {
            var taskCompletionSource = new TaskCompletionSource<byte>();
            _ = _headerReader.ReadByteAsync(
                                 LinkCancellationTokens(cancellationToken))
                             .ContinueWith(
                                 valueTask => SetResponse(valueTask, taskCompletionSource));
            return new ValueTask<byte>(taskCompletionSource.Task);
        }

        public ValueTask<byte[]> ReadBytesAsync(
            int length,
            CancellationToken cancellationToken = default)
        {
            var taskCompletionSource = new TaskCompletionSource<byte[]>();
            _ = _headerReader.ReadBytesAsync(length,
                                 LinkCancellationTokens(cancellationToken))
                             .ContinueWith(
                                 valueTask => SetResponse(valueTask, taskCompletionSource));
            return new ValueTask<byte[]>(taskCompletionSource.Task);
        }

        public ValueTask<byte> PeekByteAsync(
            CancellationToken cancellationToken = default)
        {
            var taskCompletionSource = new TaskCompletionSource<byte>();
            _ = _headerReader.PeekByteAsync(
                                 LinkCancellationTokens(cancellationToken))
                             .ContinueWith(
                                 valueTask => SetResponse(valueTask, taskCompletionSource));
            return new ValueTask<byte>(taskCompletionSource.Task);
        }

        public ValueTask<ushort> ReadUShortAsync(
            CancellationToken cancellationToken = default)
        {
            var taskCompletionSource = new TaskCompletionSource<ushort>();
            _ = _headerReader.ReadUShortAsync(
                                 LinkCancellationTokens(cancellationToken))
                             .ContinueWith(
                                 valueTask => SetResponse(valueTask, taskCompletionSource));
            return new ValueTask<ushort>(taskCompletionSource.Task);
        }

        public async ValueTask<byte[]> ReadStringAsync(
            CancellationToken cancellationToken = default)
        {
            var internalCancellationToken =
                LinkCancellationTokens(cancellationToken);

            var length = await ReadInt32Async(internalCancellationToken)
                .ConfigureAwait(false);
            return await ReadBytesAsync(length, internalCancellationToken)
                .ConfigureAwait(false);
        }

        private async ValueTask RunZlibDecompress()
        {
            var result = _zlibCodec.InitializeInflate();
            if (result < 0)
            {
                throw new InvalidOperationException(
                    $"Got error code {result} when initializing inflate routine: {_zlibCodec.Message}");
            }

            var buffer = new byte[Math.Min(1024, _length)];
            _zlibCodec.OutputBuffer = buffer;

            var left = _length;
            Exception? exception = null;
            try
            {
                while (left > 0)
                {
                    var readLength =
                        left > buffer.Length ? buffer.Length : left;
                    left -= readLength;
                    var inputBuffer = await _frameReader.ReadBytesAsync(
                            readLength, CancellationToken)
                        .ConfigureAwait(false);

                    _zlibCodec.NextIn = 0;
                    _zlibCodec.InputBuffer = inputBuffer;
                    _zlibCodec.AvailableBytesIn = inputBuffer.Length;

                    while (_zlibCodec.AvailableBytesIn > 0)
                    {
                        _zlibCodec.NextOut = 0;
                        _zlibCodec.AvailableBytesOut = buffer.Length;

                        var start = _zlibCodec.NextOut;
                        result = _zlibCodec.Inflate(
                            left == 0 ? FlushType.Finish : FlushType.Sync);
                        var end = _zlibCodec.NextOut;
                        var length = _zlibCodec.NextOut - start;
                        
                        buffer[start..end]
                            .CopyTo(_pipe.Writer.GetMemory());
                        _pipe.Writer.Advance(length);
                        await _pipe.Writer.FlushAsync(CancellationToken)
                                   .ConfigureAwait(false);

                        switch (result)
                        {
                            case ZlibConstants.Z_STREAM_END:
                                if (left > 0)
                                {
                                    throw new InvalidOperationException(
                                        $"Expected {_length} bytes, but received end of stream when there were {left} bytes left");
                                }

                                return;
                            case ZlibConstants.Z_NEED_DICT:
                                result = _zlibCodec.SetDictionary(_dictionary);
                                if (result < 0)
                                {
                                    throw new InvalidOperationException(
                                        $"Got error code {result} when setting dictionary: {_zlibCodec.Message}");
                                }

                                break;
                            case var _ when result < 0:
                                throw new InvalidOperationException(
                                    $"Got error code {result} when deflating the stream: {_zlibCodec.Message}");
                        }
                    }
                }

                _logger.Warning(
                    $"Could not verify checksum {_zlibCodec.Adler32}. Was the checksum present in the stream?");
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                _zlibCodec.EndInflate();
                await _pipe.Writer.CompleteAsync(exception)
                           .ConfigureAwait(false);
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                _cancellationTokenSource.Cancel();
            }
            finally
            {
                await _backgroundTask
                    .ConfigureAwait(false);
            }
            _cancellationTokenSource.Dispose();
        }
    }
}