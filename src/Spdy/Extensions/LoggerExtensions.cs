using Spdy.Frames;
using Spdy.Logging;

namespace Spdy.Extensions
{
    internal static class LoggerExtensions
    {
        internal static void LogFrameReceived(
            this ILogger logger,
            int sessionId,
            Frame frame)
        {
            if (frame.TryGetStreamId(out var streamId))
            {
                logger.Debug(
                    "[{SessionId}:{StreamId}]: Received {FrameType}: {@Frame}",
                    sessionId,
                    streamId,
                    frame.GetType()
                           .Name,
                    frame.ToStructuredLogging());
            }
            else
            {
                logger.Debug(
                    "[{SessionId}]: Received {FrameType}: {@Frame}",
                    sessionId,
                    frame.GetType()
                           .Name,
                    frame.ToStructuredLogging());
            }
        }

        internal static void LogSendingFrame(
            this ILogger logger,
            int sessionId,
            Frame frame)
        {
            if (frame.TryGetStreamId(out var streamId))
            {
                logger.Debug(
                    "[{SessionId}:{StreamId}]: Sending {FrameType}: {@Frame}",
                    sessionId,
                    streamId,
                    frame.GetType()
                         .Name, 
                    frame.ToStructuredLogging());
            }
            else
            {
                logger.Debug(
                    "[{SessionId}]: Sending {FrameType}: {@Frame}",
                    sessionId,
                    frame.GetType()
                         .Name, 
                    frame.ToStructuredLogging());
            }
        }
    }
}