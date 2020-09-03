using System.Diagnostics.CodeAnalysis;
using Port.Server.Spdy.Frames;

namespace Port.Server.Spdy
{
    public readonly struct ReadResult<T>
        where T : Frame
    {
        private readonly bool _hasResult;
        private readonly T? _result;
        private readonly RstStream? _error;

        private ReadResult(bool hasResult, [NotNullWhen(true)] T? result, [NotNullWhen(true)] RstStream? error)
        {
            _hasResult = hasResult;
            _result = result;
            _error = error;
        }

        public bool Out([NotNullWhen(true)] out T? result, [NotNullWhen(false)] out RstStream? error)
        {
            result = _result;
            error = _error;
            return _hasResult;
        }

        public T Result
        {
            get
            {
                if (Out(out var result, out var error))
                {
                    return result;
                }

                throw error;
            }
        }

        public static implicit operator ReadResult<Frame>(ReadResult<T> stream) =>
            new ReadResult<Frame>(stream._hasResult, stream._result, stream._error);

        public static ReadResult<T> Error(
            RstStream error)
        {
            return new ReadResult<T>(false, null, error);
        }

        public static ReadResult<T> Ok(
            T result)
        {
            return new ReadResult<T>(true, result, null);
        }
    }

    public static class ReadResult
    {
        public static ReadResult<T> Ok<T>(
            T result) where T : Control
        {
            return ReadResult<T>.Ok(result);
        }

        public static ReadResult<Data> Ok(
            Data result) 
        {
            return ReadResult<Data>.Ok(result);
        }
    }
}