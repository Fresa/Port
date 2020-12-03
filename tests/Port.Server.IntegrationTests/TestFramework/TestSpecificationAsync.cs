using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Port.Server.Observability;
using Test.It.With.XUnit;
using Xunit.Abstractions;

namespace Port.Server.IntegrationTests.TestFramework
{
    public class TestSpecificationAsync : XUnit2SpecificationAsync
    {
        static TestSpecificationAsync()
        {
            LogFactoryExtensions.InitializeOnce();
            NLogBuilderExtensions.ConfigureNLogOnce(new ConfigurationBuilder()
                                                    .SetBasePath(Directory.GetCurrentDirectory())
                                                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                                    .Build());
            NLogCapturingTargetExtensions.RegisterOutputOnce();
        }

        public TestSpecificationAsync(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        protected virtual Task TearDownAsync()
        {
            return Task.CompletedTask;
        }

        protected sealed override async Task DisposeAsync(bool disposing)
        {
            await TearDownAsync()
                .ConfigureAwait(false);
            await base.DisposeAsync(disposing)
                .ConfigureAwait(false);
        }
    }
}