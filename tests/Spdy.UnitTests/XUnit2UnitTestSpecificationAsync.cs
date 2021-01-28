using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Spdy.UnitTests.Observability;
using Test.It.With.XUnit;
using Xunit.Abstractions;

namespace Spdy.UnitTests
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
            LogFactoryExtensions.InitializeOnce();
            NLogBuilderExtensions.ConfigureNLogOnce(new ConfigurationBuilder()
                                                    .SetBasePath(Directory.GetCurrentDirectory())
                                                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                                    .Build());
            NLogCapturingTargetExtensions.RegisterOutputOnce();
        }

        protected XUnit2UnitTestSpecificationAsync()
        {
        }

        protected XUnit2UnitTestSpecificationAsync(
            ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
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
            if (!disposing)
            {
                return;
            }

            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }

            foreach (var asyncDisposable in _asyncDisposables)
            {
                await asyncDisposable.DisposeAsync()
                                     .ConfigureAwait(false);
            }

            await base.DisposeAsync(disposing)
                      .ConfigureAwait(false);
        }
    }
}