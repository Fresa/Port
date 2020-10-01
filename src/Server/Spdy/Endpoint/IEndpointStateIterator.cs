namespace Port.Server.Spdy.Endpoint
{
    internal interface IEndpointStateIterator
    {
        EndpointState Current { get; }

        bool TransitionTo(
            EndpointState state);
    }
}