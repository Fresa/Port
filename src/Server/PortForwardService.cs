using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Log.It;

namespace Kubernetes.PortForward.Manager.Server
{
    internal class PortForwardService
    {
        private readonly IKubernetes _k8SClient;
        private readonly ILogger _logger = LogFactory.Create<PortForwardService>();

        public PortForwardService(IKubernetes k8sClient)
        {
            _k8SClient = k8sClient;
        }

        internal async Task Forward()
        {
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            IKubernetes client = new k8s.Kubernetes(config);
            _logger.Info("Starting port forward!");

            var list = client.ListNamespacedPod("default");
            var pod = list.Items[0];
            
            // Note this is single-threaded, it won't handle concurrent requests well...
            var webSocket = await client.WebSocketNamespacedPodPortForwardAsync(pod.Metadata.Name, "default", new int[] { 80 }, "v4.channel.k8s.io");
            var demux = new StreamDemuxer(webSocket, StreamType.PortForward);
            demux.Start();

            var stream = demux.GetStream((byte?)0, (byte?)0);

            IPAddress ipAddress = IPAddress.Loopback;
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8080);
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen(100);

            Socket handler = null;

            // Note this will only accept a single connection
            var accept = Task.Run(() => {
                while (true)
                {
                    handler = listener.Accept();
                    var bytes = new byte[4096];
                    while (true)
                    {
                        int bytesRec = handler.Receive(bytes);
                        stream.Write(bytes, 0, bytesRec);
                        if (bytesRec == 0 || Encoding.ASCII.GetString(bytes, 0, bytesRec).IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                    }
                }
            });

            var copy = Task.Run(() => {
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