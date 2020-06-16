using System.Net.Http;

namespace Port.Server.IntegrationTests.TestFramework
{
    internal static class TestFrameworkExtensions
    {
        internal static KubernetesConfiguration CreateKubernetesConfiguration(
            this Kubernetes.Test.API.Server.TestFramework testFramework)
        {
            return new KubernetesConfiguration(
                handlers: new DelegatingHandler[]
                {
                    new LogItHttpMessageHandlerDecorator()
                },
                webSocketBuilder: new WebSocketClientBuilder(
                    testFramework.CreateWebSocketClient()));
        }
    }
}