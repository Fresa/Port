using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server
{
    internal sealed class SocketNetworkClient : INetworkClient
    {
        private readonly Socket _socket;

        public SocketNetworkClient(
            Socket socket)
        {
            _socket = socket;
        }

        public ValueTask DisposeAsync()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _socket.Dispose();
            return new ValueTask();
        }

        public ValueTask<int> SendAsync(
            ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            return _socket
                .SendAsync(buffer, SocketFlags.None, cancellationToken);
        }

        public ValueTask<int> ReceiveAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            return _socket
                .ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
        }
    }
}