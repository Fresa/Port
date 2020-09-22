using System.IO;
using System.IO.Pipelines;
using System.Text;
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
    public class Given_a_last_Data_frame
    {
        private static readonly byte[] Message =
        {
            0x00, 0x00, 0x00, 0x7B, 0x01, 0x00, 0x00, 0x11, 0x74, 0x68, 0x69,
            0x73, 0x20, 0x69, 0x73, 0x20, 0x61, 0x20, 0x70, 0x61, 0x79, 0x6C,
            0x6F, 0x61, 0x64
        };

        public class When_writing : XUnit2SpecificationAsync
        {
            private Data _frame;
            private readonly MemoryStream _serialized = new MemoryStream();

            protected override Task GivenAsync(
                CancellationToken cancellationToken)
            {
                _frame = Data.Last(
                    UInt31.From(123),
                    Encoding.UTF8.GetBytes("this is a payload"));
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

            private Data _message;

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                _message = (await Data.TryReadAsync(
                                         new FrameReader(
                                             PipeReader.Create(_serialized)),
                                         cancellationToken)
                                     .ConfigureAwait(false)).Result;
            }

            [Fact]
            public void It_should_be_last_frame()
            {
                _message.IsLastFrame.Should()
                        .BeTrue();
            }

            [Fact]
            public void It_should_have_stream_id()
            {
                _message.StreamId.Value.Should()
                        .Be(123);
            }

            [Fact]
            public void It_should_have_a_payload()
            {
                Encoding.UTF8.GetString(_message.Payload)
                        .Should()
                        .Be("this is a payload");
            }
        }
    }
}