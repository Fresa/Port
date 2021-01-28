using NLog;
using NLog.Targets;

namespace Spdy.UnitTests.Logging
{
    [Target("NLogCapturing")]
    public sealed class NLogCapturingTarget : TargetWithLayout
    {
        public delegate void CapturingSubscriber(string message);

        public static event CapturingSubscriber Subscribe;

        protected override void Write(LogEventInfo logEvent)
        {
            var renderedLogMessage = Layout.Render(logEvent);
            Subscribe?.Invoke(renderedLogMessage);

            base.Write(logEvent);
        }
    }
}