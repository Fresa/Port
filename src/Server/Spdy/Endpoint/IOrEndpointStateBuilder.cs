using System;

namespace Port.Server.Spdy.Endpoint
{
    internal interface IOrEndpointStateBuilder : IEndpointStateIterator
    {
        IOrEndpointStateBuilder Or(
            EndpointState endpointState, Action<IEndpointStateBuilder> builder);
    }
}