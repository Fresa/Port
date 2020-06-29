using System;
using System.Buffers;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kubernetes.Test.API.Server.Subscriptions.Models;
using Port.Server.IntegrationTests.SocketTestFramework;
using Port.Server.IntegrationTests.TestFramework;
using Test.It;
using Xunit;
using Xunit.Abstractions;

namespace Port.Server.IntegrationTests
{
    public partial class Given_a_port_forwarding_endpoint
    {
        public partial class
            When_requesting_to_port_forward : XUnit2ServiceSpecificationAsync<
                PortServerHost>
        {
            private HttpResponseMessage _response;
            private Fixture _fixture;
            private string _portForwardResponse;
            private ReadOnlySequence<byte> _webSocketMessageReveived;

            public When_requesting_to_port_forward(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override void Given(
                IServiceContainer configurer)
            {
                _fixture = DisposeAsyncOnTearDown(new Fixture(configurer));
                _fixture.PortforwardingSocket.On<byte[]>(
                    bytes => _portForwardResponse =
                        Encoding.ASCII.GetString(bytes));

                _fixture.K8sApiServer.WebSocketRequestSubscription
                    .OnWebSocketMessage(
                        new PortForward("test", "service1", 2001), memory =>
                        {
                            _webSocketMessageReveived = memory;
                            return new ValueTask<byte[]>();
                        });
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                _response = await Server.CreateHttpClient()
                    .PostAsJsonAsync(
                        "service/test/portforward",
                        new Shared.PortForward
                        {
                            Namespace = "test",
                            Name = "service1",
                            ProtocolType = ProtocolType.Tcp,
                            PodPort = 2001,
                            LocalPort = 1000
                        }, cancellationToken)
                    .ConfigureAwait(false);

                var client =
                    await _fixture.PortforwardingSocket
                        .ConnectAsync(
                            new ByteArrayMessageClientFactory(), IPAddress.Any,
                            1000, ProtocolType.Tcp, cancellationToken);
                await client.SendAsync(
                    Encoding.ASCII.GetBytes(_fixture.Request),
                    cancellationToken);
            }

            [Fact(
                DisplayName =
                    @"It should request a web socket connection to k8s api server at a port")]
            public void
                It_should_request_a_web_socket_connection_to_k8s_api_server_at_a_port()
            {
                _webSocketMessageReveived.Length.Should()
                    .BeGreaterThan(0);
            }

            [Fact(DisplayName = "It should start port forwarding")]
            public void
                It_should_start_port_forwarding()
            {
                _response.StatusCode.Should()
                    .Be(HttpStatusCode.OK);
            }

            internal sealed class Fixture : IAsyncDisposable
            {
                public Fixture(
                    IServiceContainer container)
                {
                    PortforwardingSocket =
                        SocketTestFramework.SocketTestFramework.InMemory();
                    container.RegisterSingleton(
                        () => PortforwardingSocket
                            .NetworkServerFactory);

                    K8sApiServer =
                        Kubernetes.Test.API.Server.TestFramework.Start();
                    container.RegisterSingleton(
                        () => K8sApiServer.CreateKubernetesConfiguration());
                }

                internal InMemorySocketTestFramework PortforwardingSocket
                {
                    get;
                }

                internal Kubernetes.Test.API.Server.TestFramework K8sApiServer
                {
                    get;
                }

                internal string Request => @"
POST /cgi-bin/process.cgi HTTP/1.1
User-Agent: Mozilla/4.0 (compatible; MSIE5.01; Windows NT)
Host: www.tutorialspoint.com
Content-Type: text/xml; charset=utf-8
Content-Length: length
Accept-Language: en-us
Accept-Encoding: gzip, deflate
Connection: Keep-Alive

<?xml version=""1.0"" encoding=""utf-8""?>
<string xmlns = ""http://clearforest.com/"">string</string>";

                internal string Response => @"
HTTP/1.1 400 Bad Request
Date: Sun, 18 Oct 2012 10:36:20 GMT
Server: Apache/2.2.14 (Win32)
Content-Length: 327
Content-Type: text/html; charset=iso-8859-1
Connection: Closed

<!DOCTYPE HTML PUBLIC ""-//IETF//DTD HTML 2.0//EN"">
<html>
<head>
   <title>400 Bad Request</title>
</head>
<body>
   <h1>Bad Request</h1>
   <p>Your browser sent a request that this server could not understand.</p>
   <p>The request line contained invalid characters following the protocol string.</p>
</body>
</html>";

                public async ValueTask DisposeAsync()
                {
                    await PortforwardingSocket.DisposeAsync();
                    await K8sApiServer.DisposeAsync();
                }
            }
        }
    }
}