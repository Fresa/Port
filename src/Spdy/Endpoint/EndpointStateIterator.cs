using System.Threading;

namespace Spdy.Endpoint
{
    internal sealed class EndpointStateIterator : IEndpointStateIterator
    {
        internal EndpointStateIterator(
            EndpointStateNode currentNode)
        {
            _current = currentNode;
        }

        private EndpointStateNode _current;
        public EndpointState Current => _current.State;

        public bool TransitionTo(EndpointState state)
        {
            var current = _current;
            if (current.TryGetNext(state, out var next))
            {
                return Interlocked.CompareExchange(
                    ref _current, next, current) == current;
            }

            return false;
        }
    }
}