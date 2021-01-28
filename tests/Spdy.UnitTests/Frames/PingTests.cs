using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Spdy.Frames;
using Spdy.Frames.Readers;
using Spdy.Frames.Writers;
using Spdy.Network;
using Spdy.UnitTests.Extensions;
using Xunit;

namespace Spdy.UnitTests.Frames
{
    public class Given_a_Ping_frame
    {
        private static readonly byte[] Message =
        {
            0x80, 0x03, 0x00, 0x06, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00,
            0x7B
        };

        public class When_writing : XUnit2UnitTestSpecificationAsync
        {
            private Ping _frame;
            private readonly MemoryStream _serialized = new MemoryStream();

            protected override Task GivenAsync(
                CancellationToken cancellationToken)
            {
                _frame = new Ping(123);
                return Task.CompletedTask;
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                await _frame.WriteAsync(
                                new FrameWriter(
                                    new StreamingNetworkClient(_serialized)),
                                new ExceptionThrowingHeaderWriterProvider(),
                                cancellationToken)
                            .ConfigureAwait(false);
            }

            [Fact]
            public void It_should_have_written_a_binary_stream()
            {
                _serialized.ToArray()
                           .Should()
                           .Equal(Message);
            }
        }

        public class When_reading : XUnit2UnitTestSpecificationAsync
        {
            private readonly MemoryStream _serialized =
                new MemoryStream(Message);

            private Ping _message;

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                _message =
                    (await Control.TryReadAsync(
                                      new FrameReader(
                                          PipeReader.Create(_serialized)),
                                      new ExceptionThrowingHeaderReader(),
                                      cancellationToken)
                                  .ConfigureAwait(false)).GetOrThrow() as Ping;
            }

            [Fact]
            public void It_should_have_id()
            {
                _message.Id.Should().Be(123);
            }
        }
    }
}