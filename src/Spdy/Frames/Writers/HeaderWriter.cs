using System.IO.Pipelines;
using System.Threading.Tasks;
using Spdy.Network;

namespace Spdy.Frames.Writers
{
    internal sealed class HeaderWriter : FrameWriter, IHeaderWriter
    {
        private readonly PipeWriter _writer;

        public HeaderWriter(PipeWriter writer)
            : base(new StreamingNetworkClient(writer.AsStream()))
        {
            _writer = writer;
        }

        public ValueTask DisposeAsync() => 
            _writer.CompleteAsync();
    }
}