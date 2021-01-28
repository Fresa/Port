using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Spdy.Network;

namespace Spdy.IntegrationTests.SocketTestFramework
{
    internal sealed class ByteArrayMessageClient : IMessageClient<byte[]>
    {
        private readonly INetworkClient _networkClient;

        public ByteArrayMessageClient(INetworkClient networkClient)
        {
            _networkClient = networkClient;
        }

        public async ValueTask<byte[]> ReceiveAsync(
            CancellationToken cancellationToken = default)
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(65536);
            var memory = memoryOwner.Memory;

            var length = await _networkClient.ReceiveAsync(memory, cancellationToken)
                .ConfigureAwait(false);
            return memory.Slice(0, length).ToArray();
        }

        public ValueTask DisposeAsync()
        {
            return _networkClient.DisposeAsync();
        }

        public async ValueTask SendAsync(
            byte[] payload,
            CancellationToken cancellationToken = default)
        {
            await _networkClient.SendAsync(payload, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}