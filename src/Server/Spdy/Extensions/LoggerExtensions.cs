using Log.It;
using Port.Server.Spdy.Frames;

namespace Port.Server.Spdy.Extensions
{
    internal static class LoggerExtensions
    {
        internal static void LogDataFrameReceived(
            this ILogger logger,
            Data data)
        {
            logger.Debug(
                $"[{data.StreamId}]: " +
                "Received {type}: {{\"StreamId\":{streamId}, \"IsLastFrame\":{isLastFrame}, \"PayloadSize\":{size}}}",
                data.GetType()
                    .Name,
                data.StreamId,
                data.IsLastFrame,
                data.Payload.Length);
        }

        internal static void LogControlFrameReceived(
            this ILogger logger,
            Control control)
        {
            logger.Debug(
                (control.TryGetStreamId(out var streamId)
                    ? $"[{streamId}]: "
                    : "") +
                "Received {type}: {@frame}",
                control.GetType()
                       .Name,
                control);
        }
    }
}