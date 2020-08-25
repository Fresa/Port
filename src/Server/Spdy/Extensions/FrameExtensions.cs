using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Frames;

namespace Port.Server.Spdy.Extensions
{
    internal static class FrameExtensions
    {
        internal static async IAsyncEnumerable<ReadOnlySequence<byte>>
            WriteAsync(
                this Frame frame,
                [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var pipe = new Pipe();
            var writeTask = Task.Run(async
                () =>
                {
                    var frameWriter =
                        new FrameWriter(pipe.Writer.AsStream());
                    await using (
                        frameWriter.ConfigureAwait(false))
                    {
                        await frame.WriteAsync(
                                       frameWriter,
                                       cancellationToken)
                                   .ConfigureAwait(false);
                    }
                }, cancellationToken);

            ReadResult result;
            do
            {
                result = await pipe.Reader.ReadAsync(cancellationToken)
                                   .ConfigureAwait(false);
                yield return result.Buffer;
                pipe.Reader.AdvanceTo(result.Buffer.GetPosition(result.Buffer.Length));
            } while (result.IsCompleted == false && result.IsCanceled == false);

            await writeTask.ConfigureAwait(false);
        }

        internal static async ValueTask
            WriteToAsync(
                this Frame frame,
                ConcurrentPriorityQueue<byte[]> priorityQueue,
                SynStream.PriorityLevel priorityLevel,
                CancellationToken cancellationToken = default)
        {
            var stream = new MemoryStream();
            await using (stream.ConfigureAwait(false))
            {
                await foreach (var data in frame.WriteAsync(cancellationToken)
                                                .ConfigureAwait(false))
                {
                    foreach (var buffer in data)
                    {
                        await stream.WriteAsync(buffer, cancellationToken)
                                    .ConfigureAwait(false);
                    }
                }
            }

            priorityQueue
                .Enqueue(priorityLevel, stream.ToArray());
        }
    }
}