using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using k8s;
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
            private readonly List<byte> _webSocketMessageReveived = new List<byte>();
            
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

                _fixture.K8sApiServer.Pod.PortForward
                    .OnConnected(
                        new PortForward("test", "service1", 2001), async (WebSocket socket, CancellationToken cancellationToken) =>
                        {
                            using var memoryOwner = MemoryPool<byte>.Shared.Rent(65536);
                            var memory = memoryOwner.Memory;
                            ValueWebSocketReceiveResult readResult;
                            do
                            {
                                readResult = await socket.ReceiveAsync(
                                        memory,
                                        cancellationToken)
                                    .ConfigureAwait(false);
                                _webSocketMessageReveived.AddRange(memory.Slice(0, readResult.Count).ToArray());
                            } while (readResult.EndOfMessage == false);
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
                    "k8s api server should receive the request message sent")]
            public void TestReceiveRequestMessage()
            {
                _webSocketMessageReveived.Should()
                    .HaveCount(_fixture.Request.Length + 1);
                _webSocketMessageReveived[0].Should()
                    .Be((byte)ChannelIndex.StdIn);
                Encoding.ASCII.GetString(_webSocketMessageReveived.GetRange(1, _fixture.Request.Length).ToArray()).Should()
                    .Be(_fixture.Request);
            }

            [Fact(DisplayName = "It should start port forwarding")]
            public void
                TestStartPortForwarding()
            {
                _response.StatusCode.Should()
                    .Be(HttpStatusCode.OK);
            }

            [Fact(DisplayName = "It should receive a http response")]
            public void TestReceiveResponse()
            {
                _portForwardResponse.Should()
                    .Be(_fixture.Response);
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