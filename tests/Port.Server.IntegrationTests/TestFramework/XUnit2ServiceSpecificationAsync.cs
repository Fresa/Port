using System;
using System.Collections.Generic;
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
        private TWebApplicationHost _webApplicationHost = default!;

        private readonly List<IDisposable> _disposables =
            new List<IDisposable>();
        private readonly List<IAsyncDisposable> _asyncDisposables =
            new List<IAsyncDisposable>();

        protected XUnit2ServiceSpecificationAsync(
            ITestOutputHelper testOutputHelper)
        {
            // Capture logs written to NLog and redirect it to the current session (if it belongs to it)
            DisposeOnTearDown(Output.WriteTo(testOutputHelper));
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
                await SetConfigurationAsync(
                    _webApplicationHost, cancellationTokenSource.Token);
            }
            finally
            {
                cancellationTokenSource.Cancel(false);
                await DisposeAsync();
            }
        }

        protected T DisposeOnTearDown<T>(
            T disposable)
            where T : IDisposable
        {
            _disposables.Add(disposable);
            return disposable;
        }

        protected T DisposeAsyncOnTearDown<T>(
            T disposable)
            where T : IAsyncDisposable
        {
            _asyncDisposables.Add(disposable);
            return disposable;
        }

        public async Task DisposeAsync()
        {
            await _webApplicationHost.StopAsync();

            _webApplicationHost.Dispose();

            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }

            foreach (var asyncDisposable in _asyncDisposables)
            {
                await asyncDisposable.DisposeAsync();
            }

            NLogCapturingTarget.Subscribe -= TestOutputHelper.WriteLine;
        }
    }
}