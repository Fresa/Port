namespace Port.Server.Messages
{
    internal partial class Forward
    {
        internal string GetId()
            => $"{Context}-{Namespace}/{Pod} {ProtocolType} {LocalPort}:{PodPort}";
    }
}