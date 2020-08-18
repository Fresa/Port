using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Port.Server.Spdy;
using Port.Server.Spdy.Frames;
using Port.Server.Spdy.Primitives;
using Test.It.With.XUnit;
using Xunit;

namespace Port.Server.UnitTests.Spdy.Frames
{
    public class Given_a_Window_Update_frame
    {
        private static readonly byte[] Message =
        {
            0x80, 0x03, 0x00, 0x09, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00,
            0x7B, 0x00, 0x00, 0x02, 0x1E
        };

        public class When_writing : XUnit2SpecificationAsync
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
                                new FrameWriter(_serialized),
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

        public class When_reading : XUnit2SpecificationAsync
        {
            private readonly MemoryStream _serialized =
                new MemoryStream(Message);

            private WindowUpdate _message;

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                _message = (WindowUpdate) await
                    Control.ReadAsync(
                               new FrameReader(PipeReader.Create(_serialized)),
                               cancellationToken)
                           .ConfigureAwait(false);
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
                _message.IsConnectionLevelFlowControl.Should()
                        .BeFalse();
            }
        }
    }
}