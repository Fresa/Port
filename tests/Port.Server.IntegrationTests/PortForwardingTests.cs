using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Port.Server.IntegrationTests.TestFramework;
using Port.Shared;
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

            public When_requesting_to_port_forward(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override void Given(
                IServiceContainer configurer)
            {
                _k8sApiServer =
                    DisposeAsyncOnTearDown(
                        Kubernetes.Test.API.Server.TestFramework.Start());
                configurer.RegisterSingleton(
                    () => _k8sApiServer.CreateKubernetesConfiguration());
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                _response = await Server.CreateHttpClient()
                    .PostAsJsonAsync(
                        "service/kind-argo-demo-ci/portforward",
                        new PortForward
                        {
                            Namespace = "test",
                            Name = "service1",
                            ProtocolType = ProtocolType.Tcp,
                            From = 2001,
                            To = 5040
                        }, cancellationToken)
                    .ConfigureAwait(false);
            }

            [Fact]
            public async Task
                It_should_establish_a_web_socket_connection_to_k8s_api_server()
            {
                _response.StatusCode.Should()
                    .Be(HttpStatusCode.OK);
            }
        }
    }
}