namespace Port.Server.Kubernetes
{
    internal static class Headers
    {
        internal static class PortForward
        {
            internal static class StreamType
            {
                internal const string Key = "streamtype";

                internal const string Data = "data";
                internal const string Error = "error";
            }

            internal const string RequestId = "requestid";
            internal const string Port = "port";
        }
    }
}