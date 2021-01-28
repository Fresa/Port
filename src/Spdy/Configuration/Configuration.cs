namespace Spdy.Configuration
{
    public sealed class Configuration
    {
        public Configuration(
            Ping ping,
            Metrics.Metrics metrics)
        {
            Ping = ping;
            Metrics = metrics;
        }

        internal Ping Ping { get; }

        internal Metrics.Metrics Metrics { get; }

        public static Configuration Default
            => new Configuration(Ping.Default, Spdy.Configuration.Metrics.Metrics.Default);
    }
}