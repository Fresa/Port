using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Log.It;

namespace Port.Server.IntegrationTests.SocketTestFramework
{
    internal abstract class SocketTestFramework : IAsyncDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource =
            new CancellationTokenSource();

        private readonly ILogger _logger = LogFactory.Create<SocketTestFramework>();
        private readonly List<Task> _backgroundTasks = new List<Task>();

        public static InMemorySocketTestFramework InMemory()
        {
            return new InMemorySocketTestFramework();
        }

        protected void ReceiveMessagesFor<T>(
            IReceivingClient<T> client)
            where T : notnull
        {
            var task = Task.Run(
                async () =>
                {
                    await using var _ = client.ConfigureAwait(false);
                    while (_cancellationTokenSource.IsCancellationRequested ==
                           false)
                    {
                        try
                        {
                            var receivedMessage = await client
                                                        .ReceiveAsync(
                                                            _cancellationTokenSource
                                                                .Token)
                                                        .ConfigureAwait(false);

                            if (!_subscriptions.TryGetValue(
                                receivedMessage.GetType(),
                                out var subscription))
                            {
                                throw new InvalidOperationException(
                                    $"Missing subscription for {receivedMessage.GetType()}");
                            }

                            await subscription(
                                    receivedMessage,
                                    _cancellationTokenSource.Token)
                                .ConfigureAwait(false);
                        }
                        catch when (_cancellationTokenSource
                            .IsCancellationRequested)
                        {
                            return;
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Unhandled exception when receiving messages");
                            _cancellationTokenSource.Cancel(false);
                        }
                    }
                });
            _backgroundTasks.Add(task);
        }

        private readonly Dictionary<Type, MessageSubscription> _subscriptions =
            new Dictionary<Type, MessageSubscription>();

        private delegate Task MessageSubscription(
            object message,
            CancellationToken cancellationToken = default);

        public ISourceBlock<TRequestMessage> On<TRequestMessage>(CancellationToken cancellationToken = default)
        {
            var messagesReceived = new BufferBlock<TRequestMessage>();
            On<TRequestMessage>(
                async message =>
                {
                    await messagesReceived.SendAsync(message, cancellationToken)
                                          .ConfigureAwait(false);
                });
            
            return messagesReceived;
        }

        public SocketTestFramework On<TRequestMessage>(
            Action<TRequestMessage> subscription)
        {
            _subscriptions.Add(
                typeof(TRequestMessage),
                (
                    message,
                    cancellationToken) => Task.Run(
                    () => subscription.Invoke((TRequestMessage) message),
                    cancellationToken));
            return this;
        }

        public SocketTestFramework On<TRequestMessage>(
            Func<TRequestMessage, Task> subscription)
        {
            _subscriptions.Add(
                typeof(TRequestMessage),
                async (
                    message,
                    _) => await subscription.Invoke((TRequestMessage)message)
                                            .ConfigureAwait(false));
            return this;
        }

        public SocketTestFramework On<TRequestMessage, TResponseMessage>(
            Func<TRequestMessage, Task> subscription)
            where TRequestMessage : IRespond<TResponseMessage>
        {
            _subscriptions.Add(
                typeof(TRequestMessage),
                async (
                    message,
                    _) => await subscription.Invoke((TRequestMessage) message)
                                            .ConfigureAwait(false));
            return this;
        }

        public SocketTestFramework On<TRequestMessage, TResponseMessage>(
            Func<TRequestMessage, CancellationToken, Task>
                subscription)
            where TRequestMessage : IRespond<TResponseMessage>
        {
            _subscriptions.Add(
                typeof(TRequestMessage),
                async (
                        message,
                        cancellationToken) =>
                    await subscription.Invoke(
                        (TRequestMessage) message, cancellationToken)
                        .ConfigureAwait(false));
            return this;
        }

        public async ValueTask DisposeAsync()
        {
            _cancellationTokenSource.Cancel(false);

            await Task.WhenAll(_backgroundTasks)
                .ConfigureAwait(false);
        }
    }
}