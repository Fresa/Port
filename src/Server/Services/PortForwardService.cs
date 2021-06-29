using System.Threading.Tasks;
using Grpc.Core;

namespace Port.Server.Services
{
    internal sealed class PortForwardService : PortForwarder.PortForwarderBase
    {
        public override Task PortForward(
            IAsyncStreamReader<ForwardRequest> requestStream,
            IServerStreamWriter<ForwardResponse> responseStream,
            ServerCallContext context)
            => base.PortForward(requestStream, responseStream, context);
    }
}