using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy
{
    public abstract class Frame
    {
        protected abstract bool IsControlFrame { get; }

        protected async ValueTask WriteAsync(
            IFrameWriter frameWriter,
            CancellationToken cancellationToken = default)
        {
            await frameWriter.WriteBooleanAsync(IsControlFrame, cancellationToken)
                .ConfigureAwait(false);
        }

        protected async ValueTask ReadAsync(
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            var isControlFrame = await frameReader.ReadBooleanAsync(cancellation)
                .ConfigureAwait(false);
            if (isControlFrame)
            {

            }
            else
            {

            }
        }
    }
}