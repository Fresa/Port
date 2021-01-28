using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Spdy.Endpoint
{
    internal sealed class EndpointStateNode
    {
        private readonly IReadOnlyDictionary<EndpointState, EndpointStateNode> _branches;
        internal EndpointState State { get; }

        internal EndpointStateNode(
            EndpointState state,
            IReadOnlyDictionary<EndpointState, EndpointStateNode> branches)
        {
            _branches = branches;
            State = state;
        }

        internal bool TryGetNext(
            EndpointState state,
            [NotNullWhen(true)] out EndpointStateNode? iterator)
        {
            return _branches.TryGetValue(state, out iterator);
        }
    }
}