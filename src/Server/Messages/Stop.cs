namespace Port.Server.Messages
{
    internal partial class Stop
    {
        internal string GetId()
            => $"{Context}-{Namespace}/{Pod} {ProtocolType} {LocalPort}:{PodPort}";
    }
}