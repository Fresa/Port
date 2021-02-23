using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Test.API.Server.Subscriptions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;

namespace Kubernetes.Test.API.Server
{
    public sealed class TestFramework : IAsyncDisposable
    {
        private readonly IHostBuilder _hostBuilder;
        private IHost? _host;

        private TestServer GetTestServer()
        {
            var testServer = _host.GetTestServer();
            testServer.PreserveExecutionContext = true;
            return testServer;
        }

        private TestFramework(
            IHostBuilder hostBuilder)
            => _hostBuilder = hostBuilder;

        public Uri BaseAddress => GetTestServer()
            .BaseAddress;

        private void Start()
        {
            _host = _hostBuilder.ConfigureServices(
                                    collection => collection.AddSingleton(this))
                                .Build();
            var startSignaler = new ManualResetEventSlim();
            _host.StartAsync()
                 .ContinueWith(_ => startSignaler.Set());
            startSignaler.Wait();
        }

        public static TestFramework Start(
            params string[] args)
        {
            IConfiguration configuration = default!;
            var hostBuilder =
                Program.CreateHostBuilder(args)
                       .ConfigureAppConfiguration(
                           (
                               context,
                               _) =>
                           {
                               configuration =
                                   context.Configuration;
                           })
                       .ConfigureServices(
                           collection =>
                           {
                               collection.AddLogging(
                                   builder =>
                                   {
                                       builder.ClearProviders();
                                       builder.AddNLog(configuration);
                                   });
                               collection.AddControllers(
                                   options =>
                                       options.Filters.Add(
                                           new
                                               HttpResponseExceptionFilter()));
                           })
                       .ConfigureWebHost(builder => builder.UseTestServer())
                       .UseNLog();

            var testFramework = new TestFramework(hostBuilder);
            testFramework.Start();
            return testFramework;
        }

        public HttpMessageHandler CreateHttpMessageHandler()
            => new UpgradeMessageHandler(GetTestServer());
        
        public PodSubscriptions Pod { get; } =
            new();

        public async ValueTask DisposeAsync()
        {
            if (_host != null)
            {
                await _host.StopAsync()
                           .ConfigureAwait(false);
            }

            _host?.Dispose();
        }
    }
}