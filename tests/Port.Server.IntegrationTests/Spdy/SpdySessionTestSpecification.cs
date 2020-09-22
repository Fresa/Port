using System.Threading;
using System.Threading.Tasks;
using Port.Server.IntegrationTests.TestFramework;
using Port.Server.Spdy;
using Xunit.Abstractions;

namespace Port.Server.IntegrationTests.Spdy
{
    public abstract class
        SpdySessionTestSpecification : XUnit2UnitTestSpecificationAsync
    {
        protected SpdySessionTestSpecification(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            DisposeAsyncOnTearDown(Server);
        }

        protected SpdyTestServer Server { get; } = new SpdyTestServer();
        protected SpdySession Session { get; private set; } = default!;

        protected sealed override async Task GivenAsync(
            CancellationToken cancellationToken)
        {
            Session = DisposeAsyncOnTearDown(await Server
                                                   .ConnectAsync(cancellationToken)
                                                   .ConfigureAwait(false));
            await GivenASessionAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        protected virtual Task GivenASessionAsync(
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}