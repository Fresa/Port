using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using Kubernetes.PortForward.Manager.Shared;
using Log.It;

namespace Kubernetes.PortForward.Manager.Server
{
    internal sealed class KubernetesService : IKubernetesService,
        IAsyncDisposable
    {
        private readonly IKubernetesClientFactory _clientFactory;

        private readonly CancellationTokenSource _cancellationSource =
            new CancellationTokenSource();

        private readonly ILogger _logger =
            LogFactory.Create<KubernetesService>();

        private readonly List<IAsyncDisposable> _disposables =
            new List<IAsyncDisposable>();

        public KubernetesService(
            IKubernetesClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<IEnumerable<Deployment>>
            ListDeploymentsInAllNamespacesAsync(
                string context)
        {
            using var client = _clientFactory.Create(context);
            var deployments =
                await client.ListDeploymentForAllNamespacesAsync();
            return deployments.Items.Select(
                pod => new Deployment
                {
                    Namespace = pod.Metadata.NamespaceProperty,
                    Name = pod.Metadata.Name
                });
        }

        public async Task<IEnumerable<Pod>> ListPodsInAllNamespacesAsync(
            string context)
        {
            using var client = _clientFactory.Create(context);
            var pods = await client.ListPodForAllNamespacesAsync();
            return pods.Items.Select(
                pod => new Pod
                {
                    Namespace = pod.Metadata.NamespaceProperty,
                    Name = pod.Metadata.Name
                });
        }

        public async Task<IEnumerable<Service>>
            ListServicesInAllNamespacesAsync(
                string context)
        {
            using var client = _clientFactory.Create(context);
            var services = await client.ListServiceForAllNamespacesAsync();
            return services.Items.Select(
                service => new Service
                {
                    Namespace = service.Metadata.NamespaceProperty,
                    Name = service.Metadata.Name,
                    Ports = service.Spec.Ports.Select(
                        port => new Port
                        {
                            Number = port.Port,
                            ProtocolType =
                                Enum.Parse<ProtocolType>(port.Protocol, true)
                        })
                });
        }

        public async Task PortForwardAsync(
            string context,
            Shared.PortForward portForward)
        {
            if (!portForward.To.HasValue)
            {
                return;
            }

            using var client = _clientFactory.Create(context);

            var webSocket =
                await client.WebSocketNamespacedPodPortForwardAsync(
                        portForward.Name, portForward.Namespace,
                        new[] { portForward.From },
                        "v4.channel.k8s.io")
                    .ConfigureAwait(false);

            //var demux = new StreamDemuxer(webSocket, StreamType.PortForward);
            //demux.Start();

            //var stream = demux.GetStream(
            //    ChannelIndex.StdIn, ChannelIndex.StdIn);

            //IPAddress ipAddress = IPAddress.Loopback;
            //IPEndPoint localEndPoint = new IPEndPoint(ipAddress, (int)portForward.To);
            //Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, portForward.ProtocolType);
            //listener.Bind(localEndPoint);
            //listener.Listen(100);

            //Socket handler = null;

            //// Note this will only accept a single connection
            //var accept = Task.Run(() => {
            //    while (true)
            //    {
            //        handler = listener.Accept();
            //        var bytes = new byte[4096];
            //        while (true)
            //        {
            //            int bytesRec = handler.Receive(bytes);
            //            stream.Write(bytes, 0, bytesRec);
            //            if (bytesRec == 0 || Encoding.ASCII.GetString(bytes, 0, bytesRec).IndexOf("<EOF>") > -1)
            //            {
            //                break;
            //            }
            //        }
            //    }
            //});

            //var copy = Task.Run(() => {
            //    var buff = new byte[4096];
            //    while (true)
            //    {
            //        var read = stream.Read(buff, 0, 4096);
            //        handler.Send(buff, read, 0);
            //    }
            //});

            //await accept;
            //await copy;
            //if (handler != null)
            //{
            //    handler.Close();
            //}
            //listener.Close();

            var socketServer = SocketServer.Start(
                IPAddress.Any,
                (int)portForward.To,
                portForward.ProtocolType);
            _disposables.Add(socketServer);

            var streamForwarder = new StreamForwarder(
                null, socketServer, webSocket);
            streamForwarder.Start();
            _disposables.Add(streamForwarder);
        }

        public async ValueTask DisposeAsync()
        {
            _cancellationSource.Cancel();

            await Task.WhenAll(
                    _disposables
                        .Select(disposable => disposable.DisposeAsync())
                        .Where(valueTask => !valueTask.IsCompletedSuccessfully)
                        .Select(valueTask => valueTask.AsTask()))
                .ConfigureAwait(false);
        }
    }

    internal interface INetworkServer
    {
        Task<INetworkClient> WaitForConnectedClientAsync(
            CancellationToken cancellationToken = default);
    }

    public interface INetworkClient : IAsyncDisposable
    {
        ValueTask<int> ReceiveAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default);

        ValueTask<int> SendAsync(
            ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = default);

        public int Send(
            byte[] buffer,
            int offset,
            int size);

        public int Receive(
            byte[] buffer);
    }

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

        public async ValueTask<int> SendAsync(
            ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            return await _socket
                .SendAsync(buffer, SocketFlags.None, cancellationToken)
                .ConfigureAwait(false);
        }

        public int Send(
            byte[] buffer,
            int offset,
            int size)
        {
            return _socket
                .Send(buffer, offset, size, SocketFlags.None);
        }

        public async ValueTask<int> ReceiveAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            return await _socket
                .ReceiveAsync(buffer, SocketFlags.None, cancellationToken)
                .ConfigureAwait(false);
        }

        public int Receive(
            byte[] buffer)
        {
            return _socket
                .Receive(buffer);
        }
    }

    internal class DataReceiver
    {
        private readonly INetworkClient _networkClient;
        private const int MinimumBufferSize = 512;

        private static readonly ILogger Logger =
            LogFactory.Create<DataReceiver>();

        internal DataReceiver(
            INetworkClient networkClient)
        {
            _networkClient = networkClient;
        }

        internal async Task ReceiveAsync(
            PipeWriter writer,
            CancellationToken cancellationToken)
        {
            try
            {
                FlushResult result;
                do
                {
                    var memory = writer.GetMemory(MinimumBufferSize);
                    var bytesRead = await _networkClient.ReceiveAsync(
                            memory,
                            cancellationToken)
                        .ConfigureAwait(false);

                    if (bytesRead == 0)
                    {
                        return;
                    }

                    Logger.Debug("Received {bytesRead} bytes", bytesRead);
                    writer.Advance(bytesRead);

                    result = await writer
                        .FlushAsync(cancellationToken)
                        .ConfigureAwait(false);
                } while (result.IsCanceled == false &&
                         result.IsCompleted == false);
            }
            catch (Exception ex)
            {
                await writer.CompleteAsync(ex);
                throw;
            }

            await writer.CompleteAsync();
        }
    }
}