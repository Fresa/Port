using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Port.Server.Spdy.Endpoint
{
    internal sealed class EndpointStateIterator : IEndpointStateBuilder, IOrEndpointStateBuilder
    {
        private readonly ConcurrentDictionary<EndpointState, EndpointStateIterator> _tree =
            new ConcurrentDictionary<EndpointState, EndpointStateIterator>();

        private readonly EndpointState _state;

        private EndpointStateIterator(
            EndpointState state)
        {
            _state = state;
            _current = this;
        }

        internal static IEndpointStateBuilder StartWith(EndpointState state) 
            => new EndpointStateIterator(state);

        private EndpointStateIterator _current;
        public EndpointState Current => _current._state;

        public bool TransitionTo(EndpointState state)
        {
            var current = _current;
            if (_tree.TryGetValue(state, out var next))
            {
                return Interlocked.CompareExchange(
                    ref _current, next, current) == current;
            }

            return false;
        }

        public IOrEndpointStateBuilder Then(
            EndpointState endpointState, Action<IEndpointStateBuilder>? builder = null)
        {
            var iterator = new EndpointStateIterator(endpointState);
            builder?.Invoke(iterator);
            if (_tree.TryAdd(endpointState, iterator) == false)
            {
                throw new ArgumentException(
                    paramName: nameof(endpointState),
                    message:
                    $"{endpointState.GetType()} state have already been declared");
            }

            return this;
        }

        public IOrEndpointStateBuilder Or(
            EndpointState endpointState, Action<IEndpointStateBuilder> builder)
            => Then(endpointState, builder);
    }
}