using System;

namespace Spdy.Configuration.Metrics
{
    public sealed class PingRoundTripTime
    {
        public PingRoundTripTime(
            Action<TimeSpan> observe)
        {
            Observe = observe;
        }

        internal Action<TimeSpan> Observe { get; }

        public static PingRoundTripTime Default => new PingRoundTripTime(
            observedLatency => { });
    }
}