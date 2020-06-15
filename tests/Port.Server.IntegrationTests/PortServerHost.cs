using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Port.Server.IntegrationTests.TestFramework;
using SimpleInjector;
using Test.It.Specifications;
using Test.It.While.Hosting.Your.Web.Application;

namespace Port.Server.IntegrationTests
{
    public class PortServerHost : IWebApplicationHost, IServer
    {
        private IHost _host;

        private TestServer GetTestServer()
        {
            var testServer = _host.GetTestServer();
            testServer.PreserveExecutionContext = true;
            return testServer;
        }

        public async Task<IServer> StartAsync(
            ITestConfigurer testConfigurer,
            CancellationToken cancellationToken = default)
        {
            if (_host == null)
            {
                _host = Program.CreateHostBuilder(new string[0])
                    .ConfigureServices(
                        collection =>
                        {
                            collection.AddControllers(options =>
                                options.Filters.Add(new HttpResponseExceptionFilter()));
                            testConfigurer.Configure(
                                new SimpleInjectorServiceContainer(
                                    collection.BuildServiceProvider()
                                        .GetService<Container>()));

                        })
                    .ConfigureWebHost(builder => builder.UseTestServer())
                    .Build();
                await _host.StartAsync(cancellationToken);
            }

            return this;
        }

        public async Task StopAsync(
            CancellationToken cancellationToken = default)
        {
            if (_host != null)
            {
                await _host.StopAsync(cancellationToken);
            }
        }

        public void Dispose()
        {
            _host?.Dispose();
        }

        public HttpMessageHandler CreateHttpMessageHandler()
        {
            return GetTestServer()
                .CreateHandler();
        }

        public IWebSocketClient CreateWebSocketClient()
        {
            return new TestServerWebSocketClient(
                GetTestServer()
                    .CreateWebSocketClient());
        }

        public Uri BaseAddress => GetTestServer()
            .BaseAddress;
    }
}