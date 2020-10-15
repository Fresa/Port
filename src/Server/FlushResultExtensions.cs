using System.IO.Pipelines;

namespace Port.Server
{
    internal static class FlushResultExtensions
    {
        internal static bool HasMore(
            this FlushResult readResult)
            => readResult.IsCanceled == false &&
               readResult.IsCompleted == false;
    }
}