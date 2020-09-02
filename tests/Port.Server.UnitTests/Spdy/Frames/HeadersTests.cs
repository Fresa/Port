using System.Collections.Generic;
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
    public class Given_a_Headers_frame
    {
        private static readonly byte[] Message =
        {
            0x80, 0x03, 0x00, 0x08, 0x01, 0x00, 0x00, 0x72, 0x00, 0x00, 0x00,
            0x7B, 0x38, 0xAC, 0xE3, 0xC6, 0xA7, 0xC2, 0x03, 0xE5, 0x0E, 0x50,
            0x12, 0xF7, 0x80, 0x94, 0x07, 0x2C, 0x25, 0xA9, 0x60, 0x9A, 0x2B,
            0x14, 0x94, 0x01, 0x1C, 0xA1, 0x19, 0x20, 0xCC, 0x37, 0xBF, 0x2A,
            0x33, 0x27, 0x27, 0x51, 0xDF, 0x54, 0xCF, 0x40, 0x41, 0xC3, 0x37,
            0x31, 0x39, 0x33, 0xAF, 0x24, 0xBF, 0x38, 0xC3, 0x1A, 0x1C, 0x41,
            0x39, 0xC0, 0x70, 0x4D, 0x56, 0xF0, 0x0F, 0x56, 0x88, 0x50, 0x30,
            0x34, 0xD0, 0xB3, 0xB4, 0x56, 0x28, 0x2A, 0xB3, 0x52, 0x30, 0x35,
            0xD0, 0x33, 0xD0, 0x54, 0x70, 0x4F, 0x4D, 0xCE, 0xCE, 0x57, 0xD0,
            0x57, 0x00, 0x26, 0x4E, 0x70, 0x22, 0x55, 0x70, 0x03, 0x16, 0x43,
            0x69, 0xF9, 0x15, 0x40, 0x21, 0x90, 0x02, 0x00, 0x55, 0x4F, 0x1F,
            0x61
        };

        public class When_writing : XUnit2SpecificationAsync
        {
            private Headers _frame;
            private readonly MemoryStream _serialized = new MemoryStream();

            protected override Task GivenAsync(
                CancellationToken cancellationToken)
            {
                _frame = Headers.Last(
                    UInt31.From(123),
                    new Dictionary<string, string[]>
                    {
                        {
                            "Host", new []{"test"}
                        },
                        {
                            "User-Agent",
                            new []{"Mozilla/5.0 (Macintosh; Intel Mac OS X 10.9; rv: 50.0) Gecko / 20100101 Firefox / 50.0"}
                        }
                    });
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

            private Headers _message;

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                _message = (Headers)await
                    Control.ReadAsync(
                               new FrameReader(PipeReader.Create(_serialized)),
                               cancellationToken)
                           .ConfigureAwait(false);
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
            public void It_should_have_headers()
            {
                _message.Values.Should()
                        .HaveCount(2)
                        .And
                        .ContainEquivalentOf(
                            new KeyValuePair<string, string[]>("Host", new[] { "test" }))
                        .And.ContainEquivalentOf(
                            new KeyValuePair<string, string[]>(
                                "User-Agent",
                                new[] { "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.9; rv: 50.0) Gecko / 20100101 Firefox / 50.0" }));
            }
        }
    }
}