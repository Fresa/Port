using System.Threading;
using System.Threading.Tasks;
using Spdy.Frames;
using Spdy.IntegrationTests.TestFramework;
using Xunit.Abstractions;
using Ping = Spdy.Frames.Ping;

namespace Spdy.IntegrationTests
{
    public abstract class
        SpdyClientSessionTestSpecification : XUnit2TestSpecificationAsync
    {
        protected SpdyClientSessionTestSpecification(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        protected SpdyTestServer Server { get; } = new SpdyTestServer();
        protected SpdySession Session { get; private set; } = default!;

        protected CancellationToken CancellationToken
            => CancellationTokenSource.Token;

        protected virtual Configuration.Configuration SpdySessionConfiguration
            => Configuration.Configuration.Default;

        internal SubscriptionCollection<Frame> Subscriptions { get; } =
            new SubscriptionCollection<Frame>();

        protected sealed override async Task GivenAsync(
            CancellationToken cancellationToken)
        {
            Subscriptions.Add(Server.On<Ping>());
            Subscriptions.Add(Server.On<SynStream>());
            Subscriptions.Add(Server.On<SynReply>());
            Subscriptions.Add(Server.On<Settings>());
            Subscriptions.Add(Server.On<WindowUpdate>());
            Subscriptions.Add(Server.On<RstStream>());
            Subscriptions.Add(Server.On<GoAway>());
            Subscriptions.Add(Server.On<Data>());
            Subscriptions.Add(Server.On<Headers>());

            Session = DisposeAsyncOnTearDown(
                await Server
                      .ConnectAsync(SpdySessionConfiguration, cancellationToken)
                      .ConfigureAwait(false));
            DisposeAsyncOnTearDown(Server);
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