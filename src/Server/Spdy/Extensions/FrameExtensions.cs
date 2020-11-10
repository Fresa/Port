using System;
using System.Buffers;
using System.Collections.Generic;
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
            var pipe = new Pipe(new PipeOptions(useSynchronizationContext: false));
            var writeTask = Task.Run(async
                () =>
            {
                Exception? exception = null;
                try
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

                    await pipe.Writer.FlushAsync(cancellationToken)
                              .ConfigureAwait(false);
                }
                catch (Exception caughtException)
                {
                    exception = caughtException;
                }

                await pipe.Writer.CompleteAsync(exception)
                          .ConfigureAwait(false);
            }, cancellationToken);

            System.IO.Pipelines.ReadResult result;
            do
            {
                result = await pipe.Reader.ReadAsync(cancellationToken)
                                   .ConfigureAwait(false);
                yield return result.Buffer;
                pipe.Reader.AdvanceTo(result.Buffer.GetPosition(result.Buffer.Length));
            } while (result.IsCompleted == false && result.IsCanceled == false);

            await pipe.Reader.CompleteAsync()
                      .ConfigureAwait(false);
            await writeTask.ConfigureAwait(false);
        }
    }
}