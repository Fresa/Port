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
            var ks = new KubernetesService(factory);
            await ks.PortForwardAsync(
                "kind-argo-demo-ci", new Shared.PortForward
                {
                    To = 2746,
                    From = 2746,
                    ProtocolType = ProtocolType.Tcp,
                    Namespace = "argo",
                    Name = "argo-server-5f5c647dcb-bkcz6"
                }).ConfigureAwait(false);

            await Task.Delay(int.MaxValue);
        }

    }
}
