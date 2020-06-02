using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Kubernetes.PortForward.Manager.Server.Tests
{
    public class UnitTest1 : TestSpecificationAsync
    {

        public UnitTest1(
            ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task Test1()
        {
            var factory = new KubernetesClientFactory();
            var ks = new KubernetesService(factory);
            await ks.PortForwardAsync(
                "kind-argo-demo-ci", new Shared.PortForward
                {
                    To = 2746,
                    From = 2746,
                    ProtocolType = ProtocolType.Tcp,
                    Namespace = "argo",
                    Name = "argo-server-7495b6b74b-4rqrg"
                }).ConfigureAwait(false);

            await Task.Delay(int.MaxValue);
        }

    }
}
