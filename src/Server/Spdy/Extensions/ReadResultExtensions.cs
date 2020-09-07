using Port.Server.Spdy.Frames;

namespace Port.Server.Spdy.Extensions
{
    internal static class ReadResultExtensions
    {
        internal static ReadResult<Control> AsControl<T>(
            this ReadResult<T> readResult)
            where T : Control
            => readResult.Out(out var result, out var error)
                ? ReadResult<Control>.Ok(result)
                : ReadResult<Control>.Error(error);
    }
}