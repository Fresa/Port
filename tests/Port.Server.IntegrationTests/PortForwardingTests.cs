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
    public class Given_a_port_forwarding_endpoint
    {
        public partial class
            When_requesting_to_port_forward : XUnit2ServiceSpecificationAsync<
                PortServerHost>
        {
            private Kubernetes.Test.API.Server.TestFramework _k8sApiServer;
            private HttpResponseMessage _response;

            private InMemorySocketTestFramework
                _portforwardingSocketTestFramework;

            private HttpRequest _webSocketRequest;

            public When_requesting_to_port_forward(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override void Given(
                IServiceContainer configurer)
            {
                _portforwardingSocketTestFramework =
                    DisposeAsyncOnTearDown(
                        SocketTestFramework.SocketTestFramework.InMemory());
                _portforwardingSocketTestFramework.On<byte[]>(respond => { });
                configurer.RegisterSingleton(
                    () => _portforwardingSocketTestFramework
                        .NetworkServerFactory);

                _k8sApiServer =
                    DisposeAsyncOnTearDown(
                        Kubernetes.Test.API.Server.TestFramework.Start());
                _k8sApiServer.WebSocketRequestSubscription.OnWebSocketRequest(
                    request => { _webSocketRequest = request; });
                configurer.RegisterSingleton(
                    () => _k8sApiServer.CreateKubernetesConfiguration());
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
                    await _portforwardingSocketTestFramework.ConnectAsync(
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