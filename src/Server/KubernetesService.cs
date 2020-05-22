using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Kubernetes.PortForward.Manager.Shared;
using Log.It;

namespace Kubernetes.PortForward.Manager.Server
{
    public class KubernetesService
    {
        private readonly IKubernetes _client;

        private readonly ILogger _logger =
            LogFactory.Create<KubernetesService>();

        public KubernetesService(
            IKubernetes client)
        {
            _client = client;
        }

        internal async Task<IEnumerable<Deployment>>
            ListDeploymentsInAllNamespacesAsync()
        {
            var deployments =
                await _client.ListDeploymentForAllNamespacesAsync();
            return deployments.Items.Select(
                pod => new Deployment
                {
                    Namespace = pod.Metadata.NamespaceProperty,
                    Name = pod.Metadata.Name
                });
        }

        internal async Task<IEnumerable<Pod>> ListPodsInAllNamespacesAsync()
        {
            var pods = await _client.ListPodForAllNamespacesAsync();
            return pods.Items.Select(
                pod => new Pod
                {
                    Namespace = pod.Metadata.NamespaceProperty,
                    Name = pod.Metadata.Name
                });
        }

        internal async Task<IEnumerable<Service>>
            ListServicesInAllNamespacesAsync()
        {
            var services = await _client.ListServiceForAllNamespacesAsync();
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

        internal async Task PortForward(
            string @namespace,
            string podName,
            int fromPort,
            int toPort,
            ProtocolType protocolType)
        {
            _logger.Info("Starting port forward!");

            // Note this is single-threaded, it won't handle concurrent requests well...
            var webSocket =
                await _client.WebSocketNamespacedPodPortForwardAsync(
                    podName, @namespace, new[] { fromPort },
                    "v4.channel.k8s.io");
            var demux = new StreamDemuxer(webSocket, StreamType.PortForward);
            demux.Start();

            var stream = demux.GetStream((byte?)0, (byte?)0);

            var ipAddress = IPAddress.Loopback;
            var localEndPoint = new IPEndPoint(ipAddress, toPort);
            var listener = new Socket(
                ipAddress.AddressFamily, SocketType.Stream, protocolType);
            listener.Bind(localEndPoint);
            listener.Listen(100);

            Socket handler = null;

            // Note this will only accept a single connection
            var accept = Task.Run(
                () =>
                {
                    while (true)
                    {
                        handler = listener.Accept();
                        var bytes = new byte[4096];
                        while (true)
                        {
                            var bytesRec = handler.Receive(bytes);
                            stream.Write(bytes, 0, bytesRec);
                            if (bytesRec == 0 || Encoding.ASCII
                                .GetString(bytes, 0, bytesRec)
                                .IndexOf("<EOF>") > -1)
                            {
                                break;
                            }
                        }
                    }
                });

            var copy = Task.Run(
                () =>
                {
                    var buff = new byte[4096];
                    while (true)
                    {
                        var read = stream.Read(buff, 0, 4096);
                        handler.Send(buff, read, 0);
                    }
                });

            await accept;
            await copy;
            if (handler != null)
            {
                handler.Close();
            }

            listener.Close();
        }
    }
}