﻿namespace Port.Server.Messages
{
    internal sealed partial class ForwardResponse
    {
        internal static ForwardResponse CreateStopped() => new()
        {
            Stopped = new Stopped()
        };

        internal static ForwardResponse WasForwarded() => new()
        {
            Forwarded = new Forwarded()
        };
    }
}