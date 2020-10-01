using System;

namespace Port.Server.Spdy.Endpoint
{
    internal interface IEndpointStateBuilder : IEndpointStateIterator
    {
        IOrEndpointStateBuilder Then(
            EndpointState endpointState,
            Action<IEndpointStateBuilder>? builder = null);
    }
}