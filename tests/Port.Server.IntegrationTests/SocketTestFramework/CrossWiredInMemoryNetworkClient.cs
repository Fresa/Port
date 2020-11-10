using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Log.It;

namespace Port.Server.IntegrationTests.SocketTestFramework
{
    internal sealed class CrossWiredInMemoryNetworkClient : INetworkClient
    {
        private readonly Pipe _reader;
        private readonly Pipe _writer;

        private ILogger _logger = LogFactory.Create<CrossWiredInMemoryNetworkClient>();

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }

        public CrossWiredInMemoryNetworkClient() : this(
            new Pipe(new PipeOptions(useSynchronizationContext: false)),
            new Pipe(new PipeOptions(useSynchronizationContext: false)))
        {
            
        }
        private CrossWiredInMemoryNetworkClient(Pipe reader, Pipe writer)
        {
            _reader = reader;
            _writer = writer;
        }

        internal INetworkClient CreateReverseClient()
        {
            return new CrossWiredInMemoryNetworkClient(_writer, _reader);
        }

        public async ValueTask<int> SendAsync(
            ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            _logger.Trace("Sending to pipe");
            await _writer.Writer
                       .WriteAsync(buffer, cancellationToken)
                       .ConfigureAwait(false);
            _logger.Trace("Sent {length} bytes to pipe", buffer.Length);
            return buffer.Length;
        }

        public async ValueTask<int> ReceiveAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            var result = await _reader.Reader
                                    .ReadAsync(cancellationToken)
                                    .ConfigureAwait(false);

            var length = result.Buffer.Length > buffer.Length
                ? buffer.Length
                : (int)result.Buffer.Length;

            var data = result
                       .Buffer
                       .Slice(0, length);
            data.CopyTo(
                buffer
                    .Span);

            _reader.Reader.AdvanceTo(data.End, result.Buffer.End);
            return length;
        }
    }
}