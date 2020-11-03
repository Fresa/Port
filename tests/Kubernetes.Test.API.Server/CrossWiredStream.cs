using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Test.API.Server
{
    internal sealed class CrossWiredStream : Stream
    {
        private readonly Pipe _read;
        private readonly Pipe _write;

        public CrossWiredStream()
            : this(new Pipe(), new Pipe())
        {
        }

        private CrossWiredStream(
            Pipe read,
            Pipe write)
        {
            _read = read;
            _write = write;
        }

        internal Stream CreateReverseStream()
            => new CrossWiredStream(_write, _read);


        public override bool CanTimeout => false;

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override Task FlushAsync(
            CancellationToken cancellationToken)
            => _write.Writer.FlushAsync(cancellationToken)
                     .AsTask();

        public override int Read(
            byte[] buffer,
            int offset,
            int count)
            => throw new NotImplementedException();

        public override void Close()
        {
            _read.Reader.Complete();
            _write.Writer.Complete();
        }

        public override async ValueTask WriteAsync(
            ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await _write.Writer.WriteAsync(buffer, cancellationToken)
                        .ConfigureAwait(false);

            if (result.IsCanceled || result.IsCompleted)
            {
                throw new OperationCanceledException();
            }
        }

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await _read.Reader.ReadAsync(cancellationToken)
                                    .ConfigureAwait(false);
            
            if (result.Buffer.Length == 0 &&
                result.IsCanceled || result.IsCompleted)
            {
                throw new OperationCanceledException();
            }

            foreach (var resultBuffer in result.Buffer)
            {
                resultBuffer.CopyTo(buffer);
            }

            _read.Reader.AdvanceTo(
                result.Buffer.GetPosition(result.Buffer.Length));
            return (int) result.Buffer.Length;
        }

        public override long Seek(
            long offset,
            SeekOrigin origin)
            => throw new NotImplementedException();

        public override void SetLength(
            long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(
            byte[] buffer,
            int offset,
            int count)
        {
            throw new NotImplementedException();
        }

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