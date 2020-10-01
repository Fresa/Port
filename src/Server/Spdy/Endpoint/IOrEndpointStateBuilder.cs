using System;

namespace Port.Server.Spdy.Endpoint
{
    internal interface IOrEndpointStateBuilder
    {
        IOrEndpointStateBuilder Or(
            EndpointState endpointState, Action<IEndpointStateBuilder> builder);

        IEndpointStateIterator Build();
    }
}