using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Spdy.Frames;
using Spdy.Frames.Readers;
using Spdy.Frames.Writers;
using Spdy.Network;
using Spdy.Primitives;
using Xunit;

namespace Spdy.UnitTests.Frames
{
    public class Given_a_Go_Away_frame
    {
        private static readonly byte[] Message =
        {
            0x80, 0x03, 0x00, 0x07, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00,
            0x7B, 0x00, 0x00, 0x00, 0x02
        };

        public class When_writing : XUnit2UnitTestSpecificationAsync
        {
            private GoAway _frame;
            private readonly MemoryStream _serialized = new MemoryStream();

            protected override Task GivenAsync(
                CancellationToken cancellationToken)
            {
                _frame = GoAway.InternalError(
                    UInt31.From(123));
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

            private GoAway _message;

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                _message = 
                    (GoAway)(await Control.TryReadAsync(
                                     new FrameReader(
                                         PipeReader.Create(_serialized)),
                                     new ExceptionThrowingHeaderReader(),
                                     cancellationToken)
                                 .ConfigureAwait(false)).Result;
            }

            [Fact]
            public void It_should_have_last_good_stream_id()
            {
                _message.LastGoodStreamId.Should()
                        .Be(UInt31.From(123));
            }

            [Fact]
            public void It_should_have_status()
            {
                _message.Status.Should()
                        .Be(GoAway.StatusCode.InternalError);
            }
        }
    }
}