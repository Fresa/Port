using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy;

namespace Port.Server.Kubernetes
{
    internal static class KubernetesExtensions
    {
        internal static async Task<SpdySession> SpdyNamespacedPodPortForwardAsync(
            this k8s.Kubernetes kubernetes,
            string name,
            string @namespace,
            int[] ports,
            CancellationToken cancellationToken = default)
        {
            if (ports.Any() == false)
            {
                throw new ArgumentOutOfRangeException(nameof(ports), "At least one port needs to be specified");
            }
            var uri = new Uri(kubernetes.BaseUri, $"api/v1/namespaces/{@namespace}/pods/{name}/portforward?{string.Join('&', ports.Select(port => $"ports={port}"))}");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.TryAddWithoutValidation(
                Microsoft.Net.Http.Headers.HeaderNames.Connection, "Upgrade");
            request.Headers.TryAddWithoutValidation(
                Microsoft.Net.Http.Headers.HeaderNames.Upgrade, "SPDY/3.1");
            if (kubernetes.Credentials != null)
            {
                await kubernetes.Credentials.ProcessHttpRequestAsync(
                              request, cancellationToken)
                          .ConfigureAwait(false);
            }

            var response = await kubernetes.HttpClient.SendAsync(request, cancellationToken)
                                           .ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.SwitchingProtocols)
            {
                throw new InvalidOperationException(
                    $"Expected switching protocol, but got {response.StatusCode}");
            }

            var stream = await response.Content.ReadAsStreamAsync()
                                       .ConfigureAwait(false);
            return new SpdySession(new StreamingNetworkClient(stream));
        }
    }
}