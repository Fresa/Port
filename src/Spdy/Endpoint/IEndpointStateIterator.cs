namespace Spdy.Endpoint
{
    internal interface IEndpointStateIterator
    {
        EndpointState Current { get; }

        bool TransitionTo(
            EndpointState state);
    }
}