using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using Port.Server.Kubernetes;
using Port.Shared;

namespace Port.Server
{
    internal sealed class KubernetesService : IKubernetesService
    {
        private readonly IKubernetesClientFactory _clientFactory;
        private readonly INetworkServerFactory _networkServerFactory;
        
        public KubernetesService(
            IKubernetesClientFactory clientFactory,
            INetworkServerFactory networkServerFactory)
        {
            _clientFactory = clientFactory;
            _networkServerFactory = networkServerFactory;
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
                            int.TryParse(
                                port.TargetPort.Value, out var targetPort)
                                ? targetPort
                                : port.NodePort ?? port.Port,
                            Enum.Parse<ProtocolType>(port.Protocol, true))),
                    service.Spec.Selector));
        }

        public async Task<IAsyncDisposable> PortForwardAsync(
            string context,
            PortForward portForward,
            CancellationToken cancellationToken = default)
        {
            if (!portForward.LocalPort.HasValue)
            {
                throw new ArgumentNullException(
                    nameof(portForward.LocalPort), "Must specify local port");
            }

            var socketServer = _networkServerFactory.CreateAndStart(
                IPAddress.IPv6Any,
                (int)portForward.LocalPort,
                portForward.ProtocolType);

            using var client = _clientFactory.Create(context);
            var session = await client.SpdyNamespacedPodPortForwardAsync(
                                          portForward.Pod,
                                          portForward.Namespace,
                                          new[] { portForward.PodPort },
                                          cancellationToken)
                                      .ConfigureAwait(false);

            // The disposables have order dependencies so they need to be
            // disposed in the reversed order they where created
            return new AsyncDisposables(
                SpdyStreamForwarder.Start(socketServer, session, portForward),
                session,
                socketServer);
        }
    }
}