using System.Net.Http;
using Grpc.Net.Client;
using NLog.Extensions.Logging;
using Test.It.While.Hosting.Your.Web.Application;

namespace Port.Server.IntegrationTests.TestFramework
{
    internal static class ServerExtensions
    {
        internal static HttpClient CreateHttpClient(this IServer server)
        {
            return new(
                new LogItHttpMessageHandlerDecorator(server.CreateHttpMessageHandler()))
            {
                BaseAddress = server.BaseAddress
            };
        }

        internal static GrpcChannel CreateGrpcWebChannel(this IServer server)
        {
            var httpClient = server.CreateHttpClient();
            var channel = GrpcChannel.ForAddress(httpClient.BaseAddress!, new GrpcChannelOptions
            {
                HttpClient = httpClient,
                LoggerFactory = new NLogLoggerFactory()
            });

            return channel;
        }
    }
    
}