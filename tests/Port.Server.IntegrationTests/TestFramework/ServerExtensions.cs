using System.Net.Http;
using Test.It.While.Hosting.Your.Web.Application;

namespace Port.Server.IntegrationTests.TestFramework
{
    internal static class ServerExtensions
    {
        internal static HttpClient CreateHttpClient(this IServer server)
        {
            return new(
                new LogItHttpMessageHandlerDecorator(
                    server.CreateHttpMessageHandler()))
            {
                BaseAddress = server.BaseAddress
            };
        }
    }
}