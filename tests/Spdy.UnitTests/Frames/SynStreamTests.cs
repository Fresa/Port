using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Spdy.Collections;
using Spdy.Frames;
using Spdy.Frames.Readers;
using Spdy.Frames.Writers;
using Spdy.Network;
using Spdy.Primitives;
using Spdy.UnitTests.Extensions;
using Xunit;

namespace Spdy.UnitTests.Frames
{
    public class Given_a_Syn_Stream_frame
    {
        private static readonly byte[] Message =
        {
            0x80, 0x03, 0x00, 0x01, 0x02, 0x00, 0x00, 0x73, 0x00, 0x00, 0x00,
            0x7B, 0x00, 0x00, 0x02, 0x0C, 0x40, 0x00, 0x78, 0xBB, 0xE3, 0xC6,
            0xA7, 0xC2, 0x02, 0xE5, 0x0E, 0xA4, 0xF2, 0x80, 0xAB, 0x04, 0x18,
            0x49, 0x0C, 0x20, 0xC2, 0x08, 0x23, 0x1B, 0x84, 0xF9, 0xE6, 0x57,
            0x65, 0xE6, 0xE4, 0x24, 0xEA, 0x9B, 0xEA, 0x19, 0x28, 0x68, 0xF8,
            0x26, 0x26, 0x67, 0xE6, 0x95, 0xE4, 0x17, 0x67, 0x58, 0x83, 0xA3,
            0x29, 0x07, 0x18, 0xBA, 0xC9, 0x0A, 0xFE, 0xC1, 0x0A, 0x11, 0x0A,
            0x86, 0x06, 0x7A, 0x96, 0xD6, 0x0A, 0x45, 0x65, 0x56, 0x0A, 0xA6,
            0x06, 0x7A, 0x06, 0x9A, 0x0A, 0xEE, 0xA9, 0xC9, 0xD9, 0xF9, 0x0A,
            0xFA, 0x0A, 0xC0, 0x24, 0x0A, 0x4E, 0xAA, 0x0A, 0x6E, 0xC0, 0xC2,
            0x28, 0x2D, 0xBF, 0x02, 0x28, 0x04, 0x52, 0x00, 0x00, 0x00, 0x00,
            0xFF, 0xFF
        };

        public class When_writing : XUnit2UnitTestSpecificationAsync
        {
            private SynStream _frame;
            private readonly MemoryStream _serialized = new MemoryStream();

            protected override Task GivenAsync(
                CancellationToken cancellationToken)
            {
                _frame = new SynStream(
                    SynStream.Options.Unidirectional,
                    UInt31.From(123),
                    UInt31.From(524),
                    SynStream.PriorityLevel.High,
                    new NameValueHeaderBlock(
                        ("host", new[] {"test", "test2"}),
                        ("user-agent",
                            new[]
                            {
                                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.9; rv: 50.0) Gecko / 20100101 Firefox / 50.0"
                            })));
                return Task.CompletedTask;
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                await _frame.WriteAsync(
                                new FrameWriter(
                                    new StreamingNetworkClient(_serialized)),
                                DisposeAsyncOnTearDown(new HeaderWriterProvider()),
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

            private SynStream _message;

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                var reader = new FrameReader(PipeReader.Create(_serialized));
                _message = (await Control.TryReadAsync(
                                      reader,
                                      new HeaderReader(reader),
                                      cancellationToken)
                                  .ConfigureAwait(false))
                    .GetOrThrow() as SynStream;
            }

            [Fact]
            public void It_should_be_unidirectional()
            {
                _message.IsUnidirectional.Should()
                        .BeTrue();
            }

            [Fact]
            public void It_should_not_be_fin()
            {
                _message.IsFin.Should()
                        .BeFalse();
            }

            [Fact]
            public void It_should_not_be_independent_stream()
            {
                _message.IsIndependentStream.Should()
                        .BeFalse();
            }

            [Fact]
            public void It_should_have_stream_id()
            {
                _message.StreamId.Value.Should()
                        .Be(123);
            }

            [Fact]
            public void It_should_have_associated_stream_id()
            {
                _message.AssociatedToStreamId.Value.Should()
                        .Be(524);
            }

            [Fact]
            public void It_should_have_priority()
            {
                _message.Priority.Should()
                        .Be(SynStream.PriorityLevel.High);
            }

            [Fact]
            public void It_should_have_headers()
            {
                _message.Headers.Should()
                        .HaveCount(2)
                        .And
                        .BeEquivalentTo(
                            new NameValueHeaderBlock(
                                ("host", new[] {"test", "test2"}),
                                ("user-agent", new[]
                                {
                                    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.9; rv: 50.0) Gecko / 20100101 Firefox / 50.0"
                                })));
            }
        }
    }
}