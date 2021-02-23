using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kubernetes.Test.API.Server
{
    internal sealed class DuplexStreamContent : HttpContent
    {
        private readonly Stream _stream;

        public DuplexStreamContent(
            Stream stream) => _stream = stream;

        protected override Task<Stream> CreateContentReadStreamAsync()
            => Task.FromResult(_stream);

        protected override Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context)
            => Task.CompletedTask;

        protected override bool TryComputeLength(
            out long length)
        {
            length = 0;
            return false;
        }
    }
}