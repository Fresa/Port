using System;
using System.Collections.Concurrent;

namespace Spdy.Endpoint
{
    internal sealed class EndpointStateBuilder : IEndpointStateBuilder, IOrEndpointStateBuilder
    {
        private readonly ConcurrentDictionary<EndpointState, EndpointStateNode> _branches =
            new ConcurrentDictionary<EndpointState, EndpointStateNode>();

        private readonly EndpointState _state;

        private EndpointStateBuilder(
            EndpointState state)
        {
            _state = state;
        }

        internal static IEndpointStateBuilder StartWith(EndpointState state)
            => new EndpointStateBuilder(state);

        public IOrEndpointStateBuilder Then(
            EndpointState endpointState, Action<IEndpointStateBuilder>? configure = null)
        {
            var builder = new EndpointStateBuilder(endpointState);
            configure?.Invoke(builder);
            if (_branches.TryAdd(endpointState, builder.Node) == false)
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

        public IEndpointStateIterator Build() => new EndpointStateIterator(Node);
        private EndpointStateNode Node => 
            new EndpointStateNode(_state, _branches);
    }
}