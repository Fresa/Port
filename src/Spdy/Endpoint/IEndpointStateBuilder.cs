using System;

namespace Spdy.Endpoint
{
    internal interface IEndpointStateBuilder
    {
        IOrEndpointStateBuilder Then(
            EndpointState endpointState,
            Action<IEndpointStateBuilder>? builder = null);

        IEndpointStateIterator Build();
    }
}