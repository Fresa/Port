using System.Net.Http;

namespace Port.Server.IntegrationTests.TestFramework
{
    internal static class TestFrameworkExtensions
    {
        internal static KubernetesConfiguration CreateKubernetesConfiguration(
            this global::Kubernetes.Test.API.Server.TestFramework testFramework)
        {
            return new KubernetesConfiguration(
                createHandlers: () => new DelegatingHandler[]
                {
                    new LogItHttpMessageHandlerDecorator(
                        testFramework.CreateHttpMessageHandler())
                },
                kubernetesConfigPath: "config",
                createWebSocketBuilder: () => new WebSocketClientBuilder(
                    testFramework.CreateWebSocketClient()));
        }
    }
}