using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Log.It;
using Log.It.With.NLog;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Extensions.Logging;
using Port.Server.Observability;
using Test.It.With.XUnit;
using Xunit.Abstractions;

namespace Port.Server.IntegrationTests.TestFramework
{
    public abstract class XUnit2UnitTestSpecificationAsync
        : XUnit2SpecificationAsync
    {
        private readonly List<IDisposable> _disposables =
            new List<IDisposable>();

        private readonly List<IAsyncDisposable> _asyncDisposables =
            new List<IAsyncDisposable>();

        static XUnit2UnitTestSpecificationAsync()
        {
            LogFactoryExtensions.InitializeOnce(
                new NLogFactory(new LogicalThreadContext()));
            var config = new ConfigurationBuilder()
                         .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                         .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                         .Build();
            LogManager.Configuration = new NLogLoggingConfiguration(
                config.GetSection("NLog"));
        }

        protected XUnit2UnitTestSpecificationAsync()
        {
        }

        protected XUnit2UnitTestSpecificationAsync(
            ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            // Capture logs written to NLog and redirect it to the current session (if it belongs to it)
            DisposeOnTearDown(Output.WriteTo(testOutputHelper));
            NLogCapturingTarget.Subscribe += TestOutputHelper.WriteLine;
        }

        protected override CancellationTokenSource CancellationTokenSource
        {
            get;
        } = new CancellationTokenSource(TimeSpan.FromSeconds(3));

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

        protected override async Task DisposeAsync(
            bool disposing)
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }

            foreach (var asyncDisposable in _asyncDisposables)
            {
                await asyncDisposable.DisposeAsync()
                                     .ConfigureAwait(false);
            }

            NLogCapturingTarget.Subscribe -= TestOutputHelper.WriteLine;
        }
    }
}