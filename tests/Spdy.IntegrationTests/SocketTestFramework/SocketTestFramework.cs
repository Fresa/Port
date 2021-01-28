using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Spdy.IntegrationTests.SocketTestFramework.Collections;

namespace Spdy.IntegrationTests.SocketTestFramework
{
    internal abstract class SocketTestFramework : IAsyncDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource =
            new CancellationTokenSource();

        private readonly List<Task> _messageReceivingTasks = new List<Task>();

        public static InMemorySocketTestFramework InMemory()
            => new InMemorySocketTestFramework();

        protected void ReceiveMessagesFor<T>(
            IReceivingClient<T> client)
            where T : notnull
        {
            var task = Task.Run(
                async () =>
                {
                    try
                    {
                        await using var _ = client.ConfigureAwait(false);
                        while (_cancellationTokenSource
                                   .IsCancellationRequested ==
                               false)
                        {
                            var receivedMessageTask = client
                                .ReceiveAsync(
                                    _cancellationTokenSource
                                        .Token);

                            T receivedMessage;
                            try
                            {
                                receivedMessage = await receivedMessageTask
                                    .ConfigureAwait(false);
                            }
                            catch when (receivedMessageTask.IsCanceled)
                            {
                                _cancellationTokenSource.Cancel(false);
                                return;
                            }

                            if (!_subscriptions.TryGetValue(
                                receivedMessage.GetType(),
                                out var subscription))
                            {
                                throw new InvalidOperationException(
                                    $"Missing subscription for {receivedMessage.GetType()}");
                            }

                            subscription(receivedMessage);
                        }
                    }
                    catch when (_cancellationTokenSource
                        .IsCancellationRequested)
                    {
                    }
                });
            task.ContinueWith(
                _ => _cancellationTokenSource.Cancel(false),
                TaskContinuationOptions.OnlyOnFaulted);
            _messageReceivingTasks.Add(task);
        }

        private readonly Dictionary<Type, MessageSubscription> _subscriptions =
            new Dictionary<Type, MessageSubscription>();

        private delegate void MessageSubscription(
            object message);

        private bool TryGetExceptionFromMessageReceivedTasks(
            [NotNullWhen(true)] out Exception? exception)
        {
            var exceptions = _messageReceivingTasks.Aggregate(
                new List<Exception>(), (
                    currentExceptions,
                    task) =>
                {
                    if (task.Exception != null)
                    {
                        currentExceptions.AddRange(
                            task.Exception.InnerExceptions);
                    }

                    return currentExceptions;
                });
            if (exceptions.Any() == false)
            {
                exception = default;
                return false;
            }

            exception =
                exceptions.Count == 1
                    ? exceptions.First()
                    : new AggregateException(exceptions);
            return true;
        }

        public ISubscription<TRequestMessage> On<TRequestMessage>()
        {
            var messagesReceived = new ConcurrentMessageBroker<TRequestMessage>();
            _cancellationTokenSource.Token.Register(
                () =>
                {
                    if (TryGetExceptionFromMessageReceivedTasks(
                        out var exception))
                    {
                        messagesReceived.Complete(exception);
                    }
                    else
                    {
                        messagesReceived.Complete();
                    }
                });
            _subscriptions.Add(
                typeof(TRequestMessage),
                message => messagesReceived.Send((TRequestMessage)message));

            return messagesReceived;
        }

        public SocketTestFramework On<TRequestMessage>(
            Action<TRequestMessage> subscription)
        {
            _subscriptions.Add(
                typeof(TRequestMessage), message
                    => subscription.Invoke((TRequestMessage)message));
            return this;
        }

        public async ValueTask DisposeAsync()
        {
            _cancellationTokenSource.Cancel(false);

            await Task.WhenAll(_messageReceivingTasks)
                      .ConfigureAwait(false);
        }
    }
}