using System;
using System.Threading;

namespace Spdy.Configuration
{
    public sealed class Ping
    {
        /// <summary>
        /// Ping configuration
        /// </summary>
        /// <param name="pingInterval">The interval between pings</param>
        /// <param name="maxOutstandingPings">Maximum number of unacknowledged outstanding pings</param>
        public Ping(
            TimeSpan pingInterval,
            int maxOutstandingPings)
        {
            PingInterval = pingInterval;
            MaxOutstandingPings = maxOutstandingPings;
        }

        internal TimeSpan PingInterval { get; }
        internal int MaxOutstandingPings { get; }

        public static Ping Disabled => new Ping(Timeout.InfiniteTimeSpan, 0);
        public static Ping Default => new Ping(TimeSpan.FromSeconds(5), 10);
    }
}