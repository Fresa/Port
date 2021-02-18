using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Log.It;

namespace Port.Server
{
    internal sealed class LogErrorStreamNetworkClient : INetworkClient
    {
        private readonly ILogger _logger = LogFactory.Create<LogErrorStreamNetworkClient>();

        public ValueTask DisposeAsync() => new();

        public ValueTask<int> ReceiveAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            _logger.Error(Encoding.UTF8.GetString(buffer.ToArray()));
            return new ValueTask<int>();
        }

        public ValueTask<int> SendAsync(
            ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            _logger.Error(Encoding.UTF8.GetString(buffer.ToArray()));
            return new ValueTask<int>();
        }
    }
}