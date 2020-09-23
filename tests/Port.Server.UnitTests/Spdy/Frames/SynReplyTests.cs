using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Port.Server.Spdy;
using Port.Server.Spdy.Extensions;
using Port.Server.Spdy.Frames;
using Port.Server.Spdy.Primitives;
using Test.It.With.XUnit;
using Xunit;

namespace Port.Server.UnitTests.Spdy.Frames
{
    public class Given_a_Syn_Stream_frame
    {
        private static readonly byte[] Message =
        {
            0x80, 0x03, 0x00, 0x01, 0x02, 0x00, 0x00, 0x7C, 0x00, 0x00, 0x00,
            0x7B, 0x00, 0x00, 0x02, 0x0C, 0x40, 0x00, 0x38, 0xAC, 0xE3, 0xC6,
            0xA7, 0xC2, 0x03, 0xE5, 0x0E, 0x50, 0x12, 0xF7, 0x80, 0x94, 0x07,
            0x5C, 0x25, 0xC0, 0x48, 0x62, 0x00, 0x11, 0x46, 0x20, 0x5E, 0x28,
            0x28, 0x1B, 0x38, 0x42, 0xB3, 0x41, 0x98, 0x6F, 0x7E, 0x55, 0x66,
            0x4E, 0x4E, 0xA2, 0xBE, 0xA9, 0x9E, 0x81, 0x82, 0x86, 0x6F, 0x62,
            0x72, 0x66, 0x5E, 0x49, 0x7E, 0x71, 0x86, 0x35, 0x38, 0x9A, 0x72,
            0x80, 0xA1, 0x9B, 0xAC, 0xE0, 0x1F, 0xAC, 0x10, 0xA1, 0x60, 0x68,
            0xA0, 0x67, 0x69, 0xAD, 0x50, 0x54, 0x66, 0xA5, 0x60, 0x6A, 0xA0,
            0x67, 0xA0, 0xA9, 0xE0, 0x9E, 0x9A, 0x9C, 0x9D, 0xAF, 0xA0, 0xAF,
            0x00, 0x4C, 0xA2, 0xE0, 0xA4, 0xAA, 0xE0, 0x06, 0x2C, 0x8C, 0xD2,
            0xF2, 0x2B, 0x80, 0x42, 0x20, 0x05, 0x00, 0x3D, 0x21, 0x21, 0x59
        };

        public class When_writing : XUnit2SpecificationAsync
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
                    new Dictionary<string, IReadOnlyList<string>>
                    {
                        {
                            "Host", new[] {"test", "test2"}
                        },
                        {
                            "User-Agent",
                            new[]
                            {
                                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.9; rv: 50.0) Gecko / 20100101 Firefox / 50.0"
                            }
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
                var a = _serialized.ToArray()
                                   .ToHexArrayRepresentation();
                _serialized.ToArray()
                           .Should()
                           .Equal(Message);
            }
        }

        public class When_reading : XUnit2SpecificationAsync
        {
            private readonly MemoryStream _serialized =
                new MemoryStream(Message);

            private SynStream _message;

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                _message = (SynStream) (await Control.TryReadAsync(
                                                        new FrameReader(
                                                            PipeReader.Create(
                                                                _serialized)),
                                                        cancellationToken)
                                                    .ConfigureAwait(false)).Result;
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
                        .And.ContainEquivalentOf(
                            new KeyValuePair<string, string[]>(
                                "Host", new[] {"test", "test2"}))
                        .And.ContainEquivalentOf(
                            new KeyValuePair<string, string[]>(
                                "User-Agent",
                                new[]
                                {
                                    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.9; rv: 50.0) Gecko / 20100101 Firefox / 50.0"
                                }));
            }
        }
    }
}