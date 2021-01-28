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
using Spdy.UnitTests.Extensions;
using Xunit;

namespace Spdy.UnitTests.Frames
{
    public class Given_a_Window_Update_frame
    {
        private static readonly byte[] Message =
        {
            0x80, 0x03, 0x00, 0x09, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00,
            0x7B, 0x00, 0x00, 0x02, 0x1E
        };

        public class When_writing : XUnit2UnitTestSpecificationAsync
        {
            private WindowUpdate _frame;
            private readonly MemoryStream _serialized = new MemoryStream();

            protected override Task GivenAsync(
                CancellationToken cancellationToken)
            {
                _frame = new WindowUpdate(
                    UInt31.From(123),
                    UInt31.From(542));
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

            private WindowUpdate _message;

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                _message = (await
                    Control.TryReadAsync(
                               new FrameReader(PipeReader.Create(_serialized)),
                               new ExceptionThrowingHeaderReader(),
                               cancellationToken)
                           .ConfigureAwait(false)).GetOrThrow() as WindowUpdate;
            }

            [Fact]
            public void It_should_have_stream_id()
            {
                _message.StreamId.Value.Should()
                        .Be(123);
            }

            [Fact]
            public void It_should_have_delta_window_size()
            {
                _message.DeltaWindowSize.Value.Should()
                        .Be(542);
            }

            [Fact]
            public void It_should_not_be_connection_level_flow_control()
            {
                _message.IsConnectionFlowControl.Should()
                        .BeFalse();
            }
        }
    }
}