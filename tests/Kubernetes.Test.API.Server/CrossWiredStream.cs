using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Test.API.Server
{
    internal sealed class CrossWiredStream : Stream
    {
        private readonly Stream _read;
        private readonly Stream _write;

        public CrossWiredStream()
            : this(new MemoryStream(), new MemoryStream())
        {

        }
        private CrossWiredStream(
            Stream read,
            Stream write)
        {
            _read = read;
            _write = write;
        }

        internal Stream CreateReverseStream()
        {
            return new CrossWiredStream(_write, _read);
        }

        public override void Flush()
        {
            _write.Flush();
        }

        public override int Read(
            byte[] buffer,
            int offset,
            int count)
            => _read.Read(buffer, offset, count);

        public override long Seek(
            long offset,
            SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(
            long value)
            => throw new NotSupportedException();

        public override void Write(
            byte[] buffer,
            int offset,
            int count)
        {
            _write.Write(buffer, offset, count);
        }

        public override Task WriteAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken) => _write.WriteAsync(
            buffer, offset, count, cancellationToken);

        public override void WriteByte(
            byte value)
        {
            _write.WriteByte(value);
        }

        public override IAsyncResult BeginWrite(
            byte[] buffer,
            int offset,
            int count,
            AsyncCallback callback,
            object state) => _write.BeginWrite(
            buffer, offset, count, callback, state);

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
    }
}