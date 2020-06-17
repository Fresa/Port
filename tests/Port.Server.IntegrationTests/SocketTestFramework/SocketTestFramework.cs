using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.IntegrationTests.SocketTestFramework
{
    internal abstract class SocketTestFramework : IAsyncDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource =
            new CancellationTokenSource();

        private readonly List<Task> _backgroundTasks = new List<Task>();

        public static InMemorySocketTestFramework InMemory(
            IMessageClientFactory messageClientFactory)
        {
            return new InMemorySocketTestFramework(
                messageClientFactory);
        }

        protected void ReceiveMessagesFor(
            IMessageClient client)
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
                                .ReceiveAsync(_cancellationTokenSource.Token)
                                .ConfigureAwait(false);

                            if (!_subscriptions.TryGetValue(
                                receivedMessage.Type,
                                out var subscription))
                            {
                                throw new InvalidOperationException(
                                    $"Missing subscription for {receivedMessage.Type}");
                            }

                            await subscription(
                                receivedMessage,
                                _cancellationTokenSource.Token);
                        }
                        catch when (_cancellationTokenSource
                            .IsCancellationRequested)
                        {
                            return;
                        }
                    }
                });
            _backgroundTasks.Add(task);
        }

        private readonly Dictionary<Type, MessageSubscription> _subscriptions =
            new Dictionary<Type, MessageSubscription>();

        private delegate Task MessageSubscription(
            IMessage message,
            CancellationToken cancellationToken = default);

        public SocketTestFramework On<TRequestMessage, TResponseMessage>(
            Action<TRequestMessage> subscription)
            where TRequestMessage : IRespond<TResponseMessage>, IMessage
            where TResponseMessage : IMessage
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

        public SocketTestFramework On<TRequestMessage, TResponseMessage>(
            Func<TRequestMessage, Task> subscription)
            where TRequestMessage : IRespond<TResponseMessage>, IMessage
            where TResponseMessage : IMessage
        {
            _subscriptions.Add(
                typeof(TRequestMessage),
                async (
                    message,
                    _) => await subscription.Invoke((TRequestMessage) message));
            return this;
        }

        public SocketTestFramework On<TRequestMessage, TResponseMessage>(
            Func<TRequestMessage, CancellationToken, Task>
                subscription)
            where TRequestMessage : IRespond<TResponseMessage>, IMessage
            where TResponseMessage : IMessage
        {
            _subscriptions.Add(
                typeof(TRequestMessage),
                async (
                        message,
                        cancellationToken) =>
                    await subscription.Invoke(
                        (TRequestMessage) message, cancellationToken));
            return this;
        }

        public async ValueTask DisposeAsync()
        {
            _cancellationTokenSource.Cancel();

            await Task.WhenAll(_backgroundTasks);
        }
    }
}