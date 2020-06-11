using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
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
            public When_requesting_to_port_forward(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override void Given(
                IServiceContainer configurer)
            {
                configurer.RegisterSingleton(
                    () => new KubernetesConfiguration());
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                await Server.CreateHttpClient()
                    .PostAsJsonAsync(
                        "service/kind-argo-demo-ci/portforward",
                        new PortForward(), cancellationToken)
                    .ConfigureAwait(false);
            }

            [Fact]
            public async Task
                It_should_establish_a_web_socket_connection_to_k8s_api_server()
            {
            }
        }
    }
}