using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using Microsoft.FeatureManagement;
using Port.Server.Kubernetes;
using Port.Shared;

namespace Port.Server
{
    internal sealed class KubernetesService : IKubernetesService,
        IAsyncDisposable
    {
        private readonly IKubernetesClientFactory _clientFactory;
        private readonly INetworkServerFactory _networkServerFactory;
        private readonly IFeatureManager _featureManager;

        private readonly CancellationTokenSource _cancellationSource =
            new CancellationTokenSource();

        private readonly List<IAsyncDisposable> _disposables =
            new List<IAsyncDisposable>();

        public KubernetesService(
            IKubernetesClientFactory clientFactory,
            INetworkServerFactory networkServerFactory,
            IFeatureManager featureManager)
        {
            _clientFactory = clientFactory;
            _networkServerFactory = networkServerFactory;
            _featureManager = featureManager;
        }

        public async Task<IEnumerable<Deployment>>
            ListDeploymentsInAllNamespacesAsync(
                string context)
        {
            using var client = _clientFactory.Create(context);
            var deployments =
                await client.ListDeploymentForAllNamespacesAsync()
                            .ConfigureAwait(false);
            return deployments.Items.Select(
                pod => new Deployment(
                    @namespace: pod.Metadata.NamespaceProperty,
                    name: pod.Metadata.Name));
        }

        public async Task<IEnumerable<Pod>> ListPodsInAllNamespacesAsync(
            string context)
        {
            using var client = _clientFactory.Create(context);
            var pods = await client.ListPodForAllNamespacesAsync()
                                   .ConfigureAwait(false);
            return pods.Items.Select(
                pod => new Pod(
                    pod.Metadata.NamespaceProperty,
                    pod.Metadata.Name,
                    pod.Metadata.Labels));
        }

        public async Task<IEnumerable<Service>>
            ListServicesInAllNamespacesAsync(
                string context)
        {
            using var client = _clientFactory.Create(context);
            var services = await client.ListServiceForAllNamespacesAsync()
                                       .ConfigureAwait(false);
            return services.Items.Select(
                service => new Service(
                    service.Metadata.NamespaceProperty,
                    service.Metadata.Name,
                    service.Spec.Ports.Select(
                        port => new Shared.Port(
                            port.Port,
                            Enum.Parse<ProtocolType>(port.Protocol, true))),
                    service.Spec.Selector));
        }

        public async Task PortForwardAsync(
            string context,
            PortForward portForward,
            CancellationToken cancellationToken = default)
        {
            if (!portForward.LocalPort.HasValue)
            {
                return;
            }

            using var client = _clientFactory.Create(context);

            var socketServer = _networkServerFactory.CreateAndStart(
                IPAddress.Any,
                (int) portForward.LocalPort,
                portForward.ProtocolType);
            _disposables.Add(socketServer);

            if (await _featureManager
                      .IsEnabledAsync(nameof(Features.PortForwardingWithSpdy))
                      .ConfigureAwait(false))
            {
                var session = await client.SpdyNamespacedPodPortForwardAsync(
                                              portForward.Pod,
                                              portForward.Namespace,
                                              new[] {portForward.PodPort},
                                              cancellationToken)
                                          .ConfigureAwait(false);
                _disposables.Add(session);
                _disposables.Add(
                    SpdyStreamForwarder.Start(socketServer, session));
            }
            else
            {
                var webSocket =
                    await client.WebSocketNamespacedPodPortForwardAsync(
                                    portForward.Pod, portForward.Namespace,
                                    new[] {portForward.PodPort},
                                    "v4.channel.k8s.io",
                                    cancellationToken: cancellationToken)
                                .ConfigureAwait(false);

                var streamForwarder = StreamForwarder
                    .Start(socketServer, webSocket);
                _disposables.Add(streamForwarder);
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cancellationSource.Cancel();

            await Task.WhenAll(
                          _disposables
                              .Select(disposable => disposable.DisposeAsync())
                              .Where(
                                  valueTask
                                      => !valueTask.IsCompletedSuccessfully)
                              .Select(valueTask => valueTask.AsTask()))
                      .ConfigureAwait(false);
        }
    }
}