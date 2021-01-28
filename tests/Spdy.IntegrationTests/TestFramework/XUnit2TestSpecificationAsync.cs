using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Spdy.UnitTests.Observability;
using Test.It.With.XUnit;
using Xunit.Abstractions;

namespace Spdy.IntegrationTests.TestFramework
{
    public abstract class XUnit2TestSpecificationAsync
        : XUnit2SpecificationAsync
    {
        private readonly List<IDisposable> _disposables =
            new List<IDisposable>();

        private readonly List<IAsyncDisposable> _asyncDisposables =
            new List<IAsyncDisposable>();

        static XUnit2TestSpecificationAsync()
        {
            LogFactoryExtensions.InitializeOnce();
            NLogBuilderExtensions.ConfigureNLogOnce(new ConfigurationBuilder()
                                                    .SetBasePath(Directory.GetCurrentDirectory())
                                                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                                    .Build());
            NLogCapturingTargetExtensions.RegisterOutputOnce();
        }

        
        protected XUnit2TestSpecificationAsync(
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

    //public abstract class XUnit2SpecificationAsync : SpecificationAsync,
    //    IAsyncLifetime
    //{
    //    private readonly DisposableList _disposableList = new DisposableList();
    //    private readonly ITestOutputHelper _testOutputHelper;


    //    protected XUnit2SpecificationAsync(
    //        ITestOutputHelper testOutputHelper)
    //    {
    //        _testOutputHelper = testOutputHelper;
    //        SetupOutput();
    //    }

    //    protected virtual CancellationTokenSource CancellationTokenSource
    //    {
    //        get;
    //    } = new CancellationTokenSource();

    //    private void SetupOutput()
    //    {
    //        _disposableList.Add(Output.WriteTo(_testOutputHelper));
    //    }

    //    protected readonly TextWriter TestOutputHelper = Output.Writer;

    //    protected virtual Task DisposeAsync(
    //        bool disposing)
    //    {
    //        if (!disposing)
    //        {
    //            return Task.CompletedTask;
    //        }

    //        CancellationTokenSource.Dispose();
    //        _disposableList.Dispose();
    //        return Task.CompletedTask;
    //    }

    //    public async Task InitializeAsync()
    //    {
    //        try
    //        {
    //            await SetupAsync(CancellationTokenSource.Token)
    //                .ConfigureAwait(false);
    //        }
    //        catch
    //        {
    //            await DisposeAsync()
    //                .ConfigureAwait(false);
    //            throw;
    //        }
    //    }

    //    public Task DisposeAsync()
    //    {
    //        GC.SuppressFinalize(this);
    //        return DisposeAsync(true);
    //    }

    //    ~XUnit2SpecificationAsync()
    //    {
    //        DisposeAsync(false)
    //            .GetAwaiter()
    //            .GetResult();
    //    }
    //}

    //internal class DisposableList : List<IDisposable>, IDisposable
    //{
    //    public static DisposableList FromRange(params IDisposable[] disposables)
    //    {
    //        var list = new DisposableList();

    //        if (disposables.Any())
    //        {
    //            list.AddRange(disposables);
    //        }

    //        return list;
    //    }

    //    public void Dispose()
    //    {
    //        ForEach(disposable => disposable.Dispose());
    //    }
    //}
}