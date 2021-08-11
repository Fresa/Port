using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Spdy;

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
            if (ports.Any() is false)
            {
                throw new ArgumentOutOfRangeException(nameof(ports), "At least one port needs to be specified");
            }
            var uri = new Uri(kubernetes.BaseUri, $"api/v1/namespaces/{@namespace}/pods/{name}/portforward?{string.Join('&', ports.Select(port => $"ports={port}"))}");

            var request = new HttpRequestMessage(HttpMethod.Get, uri)
            {
                Headers =
                {
                    { Microsoft.Net.Http.Headers.HeaderNames.Connection, "upgrade" },
                    { Microsoft.Net.Http.Headers.HeaderNames.Upgrade, "SPDY/3.1" },
                    { "X-Stream-Protocol-Version", "portforward.k8s.io" },
                }
            };

            if (kubernetes.Credentials is not null)
            {
                await kubernetes.Credentials.ProcessHttpRequestAsync(
                              request, cancellationToken)
                          .ConfigureAwait(false);
            }

            var response =
                await kubernetes
                      .HttpClient.SendAsync(
                          request,
                          // This prevents the http client to buffer the response
                          HttpCompletionOption
                              .ResponseHeadersRead,
                          cancellationToken)
                      .ConfigureAwait(false);

            if (response.StatusCode is not HttpStatusCode.SwitchingProtocols)
            {
                string contentResponse;
                try
                {
                    contentResponse = await response.Content.ReadAsStringAsync(cancellationToken)
                                                    .ConfigureAwait(false);
                }
                catch
                {
                    contentResponse = "";
                }

                throw new InvalidOperationException(
                    $"Expected switching protocol, but got {response.StatusCode} with content: {contentResponse}");
            }

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
                                       .ConfigureAwait(false);
            return SpdySession.CreateClient(new StreamingNetworkClient(stream));
        }
    }
}