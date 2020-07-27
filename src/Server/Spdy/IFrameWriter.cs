using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy
{
    public interface IFrameWriter
    {
        ValueTask WriteBooleanAsync(bool value, CancellationToken cancellationToken = default);
    }
}