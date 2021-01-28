using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Spdy.Logging;
using Spdy.Network;

namespace Spdy.IntegrationTests.SocketTestFramework
{
    internal sealed class CrossWiredInMemoryNetworkClient : INetworkClient
    {
        private readonly Pipe _reader;
        private readonly Pipe _writer;

        private readonly ILogger _logger = 
            LogFactory.Create<CrossWiredInMemoryNetworkClient>();

        private bool _disconnected;

        public async ValueTask DisposeAsync()
        {
            _disconnected = true;
            await _reader.Reader.CompleteAsync()
                         .ConfigureAwait(false);
            await _writer.Writer.CompleteAsync()
                         .ConfigureAwait(false);
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

        internal INetworkClient CreateReversed()
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
            var length = 0;
            try
            {
                var result = await _reader.Reader
                                          .ReadAsync(cancellationToken)
                                          .ConfigureAwait(false);

                length = result.Buffer.Length > buffer.Length
                    ? buffer.Length
                    : (int)result.Buffer.Length;

                var data = result
                           .Buffer
                           .Slice(0, length);
                data.CopyTo(
                    buffer
                        .Span);

                _reader.Reader.AdvanceTo(data.End, result.Buffer.End);
            }
            // The other cross-wired client might have sent us data and then "disconnected".
            // We want to inform about potential data we wrote to the buffer
            catch when (_disconnected)
            {
            }

            return length;
        }
    }
}