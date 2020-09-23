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
    public class Given_a_Settings_frame
    {
        private static readonly byte[] Message =
        {
            0x80, 0x03, 0x00, 0x04, 0x01, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00,
            0x02, 0x02, 0x00, 0x00, 0x02, 0x00, 0x00, 0x04, 0xDD, 0x01, 0x00,
            0x00, 0x08, 0x00, 0x00, 0x00, 0x64
        };

        public class When_writing : XUnit2SpecificationAsync
        {
            private Settings _frame;
            private readonly MemoryStream _serialized = new MemoryStream();

            protected override Task GivenAsync(
                CancellationToken cancellationToken)
            {
                _frame = Settings.Clear(
                        Settings.ClientCertificateVectorSize(
                            100,
                            Settings.ValueOptions.PersistValue),
                        Settings.DownloadBandwidth(
                            1245,
                            Settings.ValueOptions.Persisted)
                    );
                return Task.CompletedTask;
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                await _frame.WriteAsync(
                        new FrameWriter(_serialized), cancellationToken)
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

            private Settings _message;

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                _message = (Settings)(await Control.TryReadAsync(
                        new FrameReader(PipeReader.Create(_serialized)),
                        cancellationToken)
                    .ConfigureAwait(false)).Result;
            }

            [Fact]
            public void It_should_have_clear_settings()
            {
                _message.ClearSettings.Should()
                    .BeTrue();
            }

            [Fact]
            public void It_should_have_values()
            {
                _message.Values.Should()
                    .HaveCount(2)
                    .And
                    .Contain(
                        setting => setting.ShouldPersist &&
                                   setting.IsPersisted == false &&
                                   setting.Value == 100)
                    .And.Contain(
                        setting => setting.ShouldPersist == false &&
                                   setting.IsPersisted &&
                                   setting.Value == 1245);
            }
        }
    }
}