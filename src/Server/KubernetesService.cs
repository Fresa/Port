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