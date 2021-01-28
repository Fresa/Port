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
    public class Given_a_Syn_Reply_frame
    {
        private static readonly byte[] Message =
        {
            0x80, 0x03, 0x00, 0x02, 0x01, 0x00, 0x00, 0x6A, 0x00, 0x00, 0x00,
            0x7B, 0x78, 0xBB, 0xE3, 0xC6, 0xA7, 0xC2, 0x02, 0xE5, 0x0E, 0xA4,
            0xF2, 0x80, 0xA5, 0x24, 0x15, 0x4C, 0xA3, 0x66, 0x80, 0x30, 0xDF,
            0xFC, 0xAA, 0xCC, 0x9C, 0x9C, 0x44, 0x7D, 0x53, 0x3D, 0x03, 0x05,
            0x0D, 0xDF, 0xC4, 0xE4, 0xCC, 0xBC, 0x92, 0xFC, 0xE2, 0x0C, 0x6B,
            0x70, 0x04, 0xE5, 0x00, 0xC3, 0x35, 0x59, 0xC1, 0x3F, 0x58, 0x21,
            0x42, 0xC1, 0xD0, 0x40, 0xCF, 0xD2, 0x5A, 0xA1, 0xA8, 0xCC, 0x4A,
            0xC1, 0xD4, 0x40, 0xCF, 0x40, 0x53, 0xC1, 0x3D, 0x35, 0x39, 0x3B,
            0x5F, 0x41, 0x5F, 0x01, 0x98, 0x38, 0xC1, 0x89, 0x54, 0xC1, 0x0D,
            0x58, 0x0C, 0xA5, 0xE5, 0x57, 0x00, 0x85, 0x40, 0x0A, 0x00, 0x00,
            0x00, 0x00, 0xFF, 0xFF
        };

        public class When_writing : XUnit2UnitTestSpecificationAsync
        {
            private SynReply _frame;
            private readonly MemoryStream _serialized = new MemoryStream();

            protected override Task GivenAsync(CancellationToken cancellationToken)
            {
                _frame = SynReply.AcceptAndClose(
                    UInt31.From(123),
                    new NameValueHeaderBlock
                    (
                        ("host", new[] { "test" }),
                        ("user-agent",
                            new[] { "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.9; rv: 50.0) Gecko / 20100101 Firefox / 50.0" })
                    ));
                return Task.CompletedTask;
            }

            protected override async Task WhenAsync(CancellationToken cancellationToken)
            {
                await _frame.WriteAsync(new FrameWriter(
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

            private SynReply _message;

            protected override async Task WhenAsync(CancellationToken cancellationToken)
            {
                var reader = new FrameReader(PipeReader.Create(_serialized));
                _message = (await Control.TryReadAsync(
                                             reader,
                                             new HeaderReader(reader),
                                             cancellationToken)
                                         .ConfigureAwait(false))
                    .GetOrThrow() as SynReply;
            }

            [Fact]
            public void It_should_be_fin()
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
                _message.Headers.Should()
                    .HaveCount(2)
                    .And
                    .BeEquivalentTo(new NameValueHeaderBlock(
                        ("host", new[] { "test" }),
                        ("user-agent",
                            new[] { "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.9; rv: 50.0) Gecko / 20100101 Firefox / 50.0" })));
            }
        }
    }
}