using System;
using Port.Shared;

namespace Port.Server.Messages
{
    internal static class ForwardExtensions
    {
        internal static PortForward ToPortForward(
            this Forward request)
            => new(
                request.Namespace,
                request.Pod,
                "",
                request.ProtocolType switch
                {
                    ProtocolType.Tcp =>
                        System.Net.Sockets.ProtocolType.Tcp,
                    ProtocolType.Udp =>
                        System.Net.Sockets.ProtocolType.Udp,
                    _ => throw new
                        ArgumentOutOfRangeException(
                            nameof(request
                                .ProtocolType))
                },
                (int) request.PodPort)
            {
                LocalPort =
                    (int) request.LocalPort
            };
    }
}