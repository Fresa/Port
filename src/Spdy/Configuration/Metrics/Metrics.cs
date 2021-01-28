namespace Spdy.Configuration.Metrics
{
    public sealed class Metrics
    {
        public Metrics(
            PingRoundTripTime pingRoundTripTime)
            => PingRoundTripTime = pingRoundTripTime;

        internal PingRoundTripTime PingRoundTripTime { get; }

        public static Metrics Default => new Metrics(PingRoundTripTime.Default);
    }
}