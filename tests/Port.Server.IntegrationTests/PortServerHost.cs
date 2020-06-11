using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Port.Server.IntegrationTests.TestFramework;
using Test.It.Specifications;
using Test.It.While.Hosting.Your.Web.Application;

namespace Port.Server.IntegrationTests
{
    public class PortServerHost : IWebApplicationHost, IServer
    {
        private IHost _host;

        public async Task<IServer> StartAsync(
            ITestConfigurer testConfigurer,
            CancellationToken cancellationToken = default)
        {
            if (_host == null)
            {
                _host = Program.CreateHostBuilder(new string[0])
                    .ConfigureWebHost(builder => builder.UseTestServer())
                    .Build();
                await _host.StartAsync(cancellationToken);
            }

            return this;
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
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
            return _host.GetTestServer().CreateHandler();
        }

        public IWebSocketClient CreateWebSocketClient()
        {
            return new TestServerWebSocketClient(_host.GetTestServer().CreateWebSocketClient());
        }

        public Uri BaseAddress => _host.GetTestServer().BaseAddress;
    }
}