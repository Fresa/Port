using System;

namespace Port.Server.Spdy.Endpoint
{
    internal interface IEndpointStateBuilder
    {
        IOrEndpointStateBuilder Then(
            EndpointState endpointState,
            Action<IEndpointStateBuilder>? builder = null);

        IEndpointStateIterator Build();
    }
}