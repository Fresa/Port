using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Port.Server.Spdy;
using Port.Server.Spdy.Frames;
using Test.It.With.XUnit;
using Xunit;

namespace Port.Server.UnitTests.Spdy.Frames
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

        public class When_reading : XUnit2UnitTestSpecificationAsync
        {
            private readonly MemoryStream _serialized =
                new MemoryStream(Message);

            private Ping _message;

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                _message = (Ping)
                    (await Control.TryReadAsync(
                                     new FrameReader(
                                         PipeReader.Create(_serialized)),
                                     cancellationToken)
                                 .ConfigureAwait(false)).Result;
            }

            [Fact]
            public void It_should_have_id()
            {
                _message.Id.Should().Be(123);
            }
        }
    }
}