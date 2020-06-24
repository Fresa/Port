using System;
using System.Threading.Tasks;
using Port.Server.IntegrationTests.SocketTestFramework;
using Port.Server.IntegrationTests.TestFramework;
using Test.It;

namespace Port.Server.IntegrationTests
{
    public partial class Given_a_port_forwarding_endpoint
    {
        public partial class When_requesting_to_port_forward
        {
            internal sealed class Fixture : IAsyncDisposable
            {
                public Fixture(IServiceContainer container)
                {
                    PortforwardingSocketTestFramework =
                        SocketTestFramework.SocketTestFramework.InMemory();
                    container.RegisterSingleton(
                        () => PortforwardingSocketTestFramework
                            .NetworkServerFactory);

                    K8sApiServer =
                        Kubernetes.Test.API.Server.TestFramework.Start();
                    container.RegisterSingleton(
                        () => K8sApiServer.CreateKubernetesConfiguration());
                }

                internal InMemorySocketTestFramework PortforwardingSocketTestFramework
                {
                    get;
                }

                internal Kubernetes.Test.API.Server.TestFramework K8sApiServer
                {
                    get;
                }

                public async ValueTask DisposeAsync()
                {
                    await PortforwardingSocketTestFramework.DisposeAsync();
                    await K8sApiServer.DisposeAsync();
                }
            }
        }
    }
}