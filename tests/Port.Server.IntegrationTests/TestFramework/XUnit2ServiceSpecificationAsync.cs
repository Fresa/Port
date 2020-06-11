using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Log.It.With.NLog;
using Test.It.While.Hosting.Your.Web.Application;
using Test.It.With.XUnit;
using Xunit;
using Xunit.Abstractions;

namespace Port.Server.IntegrationTests.TestFramework
{
    public abstract class XUnit2ServiceSpecificationAsync<TWebApplicationHost>
        : WebApplicationSpecification<TWebApplicationHost>, IAsyncLifetime
        where TWebApplicationHost : IWebApplicationHost, new()
    {
        private TWebApplicationHost _webApplicationHost;
        private readonly IDisposable _output;
        private bool _stopped;

        protected XUnit2ServiceSpecificationAsync(ITestOutputHelper testOutputHelper)
        {
            // Capture logs written to NLog and redirect it to the current session (if it belongs to it)
            _output = Output.WriteTo(testOutputHelper);
            NLogCapturingTarget.Subscribe += TestOutputHelper.WriteLine;
        }

        protected TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// <tldr>Captures target and redirects it to the current test session.</tldr>
        ///
        /// When running multiple tests in parallel most test frameworks loads the assemblies once and then caches them for performance.
        /// NLog is based on a static building strategy which causes all logs to be shared by all running test processes since they use the same loaded assembly.
        /// The Output helper wires the xunit test output helper into the log process and acts like a test session log router utilizing the async local mechanism.
        /// </summary>
        protected TextWriter TestOutputHelper => Output.Writer;

        public async Task InitializeAsync()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            _webApplicationHost = new TWebApplicationHost();
            try
            {
                await SetConfigurationAsync(_webApplicationHost, cancellationTokenSource.Token);
            }
            finally
            {
                cancellationTokenSource.Cancel(false);
                await DisposeAsync();
            }
        }

        protected async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await _webApplicationHost.StopAsync(cancellationToken);
            _stopped = true;
        }

        public virtual async Task DisposeAsync()
        {
            if (!_stopped)
            {
                await StopAsync();
            }
            _webApplicationHost.Dispose();
            NLogCapturingTarget.Subscribe -= TestOutputHelper.WriteLine;
            _output.Dispose();
        }
    }
}