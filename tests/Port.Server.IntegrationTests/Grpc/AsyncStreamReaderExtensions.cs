using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace Port.Server.IntegrationTests.Grpc
{
    internal static class AsyncStreamReaderExtensions
    {
        internal static async Task<T> ReadAsync<T>(
            this IAsyncStreamReader<T> reader, CancellationToken cancellationToken)
        {
            var hadNext = await reader.MoveNext(cancellationToken)
                                      .ConfigureAwait(false);
            if (hadNext)
            {
                return reader.Current;
            }

            throw new InvalidOperationException("No more items available");
        }
    }
}