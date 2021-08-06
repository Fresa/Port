using System;
using System.Net.Sockets;
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
                    Forward.Types.ProtocolType.Tcp =>
                        ProtocolType.Tcp,
                    Forward.Types.ProtocolType.Udp =>
                        ProtocolType.Udp,
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