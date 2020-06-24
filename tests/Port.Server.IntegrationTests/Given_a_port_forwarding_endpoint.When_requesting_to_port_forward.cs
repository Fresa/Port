using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
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
            private HttpRequest _webSocketRequest;
            private Fixture _fixture;

            public When_requesting_to_port_forward(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override void Given(
                IServiceContainer configurer)
            {
                _fixture = DisposeAsyncOnTearDown(new Fixture(configurer));
                _fixture.PortforwardingSocketTestFramework.On<byte[]>(
                    respond => { });

                _fixture.K8sApiServer.WebSocketRequestSubscription
                    .OnWebSocketRequest(
                        request => { _webSocketRequest = request; });
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
                            From = 2001,
                            To = 1000
                        }, cancellationToken)
                    .ConfigureAwait(false);

                var client =
                    await _fixture.PortforwardingSocketTestFramework
                        .ConnectAsync(
                            new ByteArrayMessageClientFactory(), IPAddress.Any,
                            1000, ProtocolType.Tcp, cancellationToken);
            }

            [Fact]
            public void
                It_should_request_a_web_socket_connection_to_k8s_api_server_at_a_port()
            {
                _webSocketRequest.Path.Value.Should()
                    .Be("/api/v1/namespaces/test/pods/service1/portforward");
                _webSocketRequest.Query.Should()
                    .HaveCount(1)
                    .And
                    .Contain(
                        pair => pair.Key == "ports" &&
                                pair.Value == "2001");
            }

            [Fact]
            public void
                It_should_start_port_forwarding()
            {
                _response.StatusCode.Should()
                    .Be(HttpStatusCode.OK);
            }
        }
    }
}