using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Port.Server.IntegrationTests.Spdy.Extensions
{
    internal static class SourceBlockExtensions
    {
        internal static async Task<IEnumerable<T>> ReceiveAsync<T>(
            this ISourceBlock<T> source,
            int numberOfItems = 1,
            CancellationToken cancellationToken = default)
        {
            return await Task.WhenAll(
                                 Enumerable.Range(1, numberOfItems)
                                           .Select(
                                               _ => source.ReceiveAsync(cancellationToken)))
                             .ConfigureAwait(false);
        }
    }
}