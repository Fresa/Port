using System.Net.Http;

namespace Port.Server.IntegrationTests.TestFramework
{
    internal static class TestFrameworkExtensions
    {
        internal static KubernetesConfiguration CreateKubernetesConfiguration(
            this global::Kubernetes.Test.API.Server.TestFramework testFramework)
        {
            return new KubernetesConfiguration(
                createClient: () => 
                    new HttpClient(
                        new LogItHttpMessageHandlerDecorator(
                        testFramework.CreateHttpMessageHandler()))
                    {
                        BaseAddress = testFramework.BaseAddress
                    }
                ,
                kubernetesConfigPath: "config",
                createWebSocketBuilder: () => new WebSocketClientBuilder(
                    testFramework.CreateWebSocketClient()));
        }
    }
}