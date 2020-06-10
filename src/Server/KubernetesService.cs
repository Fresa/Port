using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using Port.Shared;

namespace Port.Server
{
    internal sealed class KubernetesService : IKubernetesService,
        IAsyncDisposable
    {
        private readonly IKubernetesClientFactory _clientFactory;

        private readonly CancellationTokenSource _cancellationSource =
            new CancellationTokenSource();

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

        public async Task<IEnumerable<Shared.Pod>> ListPodsInAllNamespacesAsync(
            string context)
        {
            using var client = _clientFactory.Create(context);
            var pods = await client.ListPodForAllNamespacesAsync();
            return pods.Items.Select(
                pod => new Shared.Pod
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
                        port => new Shared.Port
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

            var streamForwarder = StreamForwarder
                .Start(socketServer, webSocket);
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
}