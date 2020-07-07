using System.Net.Sockets;
using System.Threading.Tasks;
using Port.Server.IntegrationTests.TestFramework;
using Xunit;
using Xunit.Abstractions;

namespace Port.Server.IntegrationTests
{
    public class UnitTest1 : TestSpecificationAsync
    {

        public UnitTest1(
            ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Fact(Skip = "Dependent on a real k8s cluster and services")]
        public async Task Test1()
        {
            var factory = new KubernetesClientFactory(new KubernetesConfiguration());
            var ks = new KubernetesService(factory, new SocketNetworkServerFactory());
            await ks.PortForwardAsync(
                "kind-argo-demo-ci", new Shared.PortForward
                (
                    podPort: 2746,
                    protocolType: ProtocolType.Tcp,
                    @namespace: "argo",
                    name: "argo-server-5f5c647dcb-bkcz6"
                )
                { LocalPort = 2746 }).ConfigureAwait(false);

            await Task.Delay(int.MaxValue);
        }

    }
}
