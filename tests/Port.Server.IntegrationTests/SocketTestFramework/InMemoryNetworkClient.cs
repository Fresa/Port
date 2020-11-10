using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.IntegrationTests.SocketTestFramework
{
    internal sealed class InMemoryNetworkClient : INetworkClient
    {
        private readonly Pipe _pipe = new Pipe();

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }

        public async ValueTask<int> SendAsync(
            ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            await _pipe.Writer
                .WriteAsync(buffer, cancellationToken)
                .ConfigureAwait(false);
            return buffer.Length;
        }

        public async ValueTask<int> ReceiveAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            var result = await _pipe.Reader
                .ReadAsync(cancellationToken)
                .ConfigureAwait(false);

            var length = result.Buffer.Length > buffer.Length
                ? buffer.Length
                : (int) result.Buffer.Length;

            var data = result
                .Buffer
                .Slice(0, length);
            data.CopyTo(
                buffer
                    .Span);

            _pipe.Reader.AdvanceTo(data.End, result.Buffer.End);
            return length;
        }
    }
}