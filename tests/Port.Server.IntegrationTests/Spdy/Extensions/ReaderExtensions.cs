using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.IntegrationTests.SocketTestFramework;
using Port.Server.IntegrationTests.SocketTestFramework.Collections;

namespace Port.Server.IntegrationTests.Spdy.Extensions
{
    internal static class ReaderExtensions
    {
        internal static async Task<IEnumerable<T>> ReceiveAsync<T>(
            this ISubscription<T> source,
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