using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Spdy.Frames.Readers;
using Spdy.Frames.Writers;
using Xunit;
using Xunit.Abstractions;

namespace Spdy.UnitTests
{
    public partial class Given_zlib_compression
    {
        #region Fixture
        private static readonly Encoding Encoding = Encoding.UTF8;

        private static readonly string Uncompressed = "POST /cgi-bin/process.cgi HTTP/1.1\r\n" +
                           "User-Agent: Mozilla/4.0 (compatible; MSIE5.01; Windows NT)\r\n" +
                           "Host: www.tutorialspoint.com\r\n" +
                           "Content-Type: text/xml; charset=utf-8\r\n" +
                           "Content-Length: length\r\n" +
                           "Accept-Language: en-us\r\n" +
                           "Accept-Encoding: gzip, deflate\r\n" +
                           "Connection: Keep-Alive\r\n" +
                           "\r\n" +
                           "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                           "<string xmlns = \"http://clearforest.com/\">string</string>";
        private static readonly byte[] UncompressedBytes =
            Encoding.GetBytes(Uncompressed);

        private static readonly byte[] CompressedBytes =
        {
            0x78, 0xBB, 0xE3, 0xC6, 0xA7, 0xC2, 0x54, 0x94, 0xCD, 0x6A, 0xC3,
            0x30, 0x10, 0x84, 0xEF, 0x02, 0xBD, 0xC3, 0xA0, 0x53, 0x7A, 0xB0,
            0xE4, 0x40, 0x7B, 0x91, 0x63, 0x07, 0x1F, 0x02, 0x0D, 0x49, 0x7F,
            0x20, 0x2E, 0x3D, 0xBB, 0xAE, 0x49, 0x04, 0xAE, 0x6C, 0x22, 0x85,
            0x40, 0x9F, 0xBE, 0x2B, 0x59, 0x69, 0xE9, 0x49, 0x68, 0x59, 0x49,
            0xBB, 0xA3, 0xFD, 0xE6, 0xF5, 0xE5, 0xD0, 0x40, 0x75, 0x47, 0x93,
            0x7D, 0x18, 0xAB, 0x88, 0xB5, 0xAE, 0x77, 0x4E, 0xD2, 0x1E, 0xB7,
            0xB1, 0xE4, 0xEC, 0x2D, 0xC0, 0x50, 0x07, 0x18, 0x34, 0x29, 0xF3,
            0x6D, 0x86, 0xA1, 0x55, 0xF7, 0x32, 0xC7, 0xA2, 0x1B, 0xBF, 0x26,
            0x2A, 0x9F, 0xA4, 0x2E, 0xF0, 0x74, 0xD8, 0x6E, 0x1E, 0x64, 0xBE,
            0x2C, 0xF0, 0x6E, 0xEC, 0xE7, 0x78, 0x75, 0x78, 0x6E, 0xEE, 0x38,
            0x7B, 0x24, 0x9F, 0xD1, 0x20, 0x4C, 0xA4, 0xBF, 0x78, 0xFA, 0xF3,
            0x76, 0x70, 0xD3, 0x68, 0xAC, 0x97, 0x74, 0x96, 0xB3, 0x34, 0x39,
            0x59, 0x43, 0x16, 0xA6, 0x11, 0xDB, 0x25, 0x09, 0x0A, 0xFC, 0x6B,
            0xE1, 0x2F, 0x6D, 0x1F, 0xFD, 0x52, 0x63, 0xF6, 0x4D, 0xCE, 0xEA,
            0xD9, 0xD0, 0xF7, 0xC9, 0x5A, 0x35, 0x7A, 0x9B, 0x5D, 0xDC, 0x6F,
            0x7C, 0x93, 0xD8, 0xD6, 0x88, 0x1A, 0x21, 0x89, 0x14, 0xEF, 0x4B,
            0x2E, 0xAE, 0xB1, 0x0B, 0x48, 0xD5, 0x01, 0x29, 0xCE, 0x38, 0x5B,
            0xAD, 0xE9, 0x7D, 0x24, 0x38, 0x4B, 0xB1, 0x94, 0xB9, 0xC0, 0xCD,
            0x22, 0x4A, 0x11, 0xCB, 0x11, 0xEB, 0x8A, 0xF2, 0x9C, 0x3F, 0x53,
            0x08, 0x94, 0x6D, 0x1D, 0x4A, 0x88, 0x93, 0xF7, 0x93, 0x56, 0xAA,
            0x1B, 0xFA, 0xF6, 0x4C, 0x7C, 0xD0, 0xA8, 0x87, 0x0E, 0x95, 0xA8,
            0xE6, 0xC4, 0x95, 0x9A, 0xD7, 0xEA, 0x07, 0x00, 0x00, 0xFF, 0xFF,
            0x03, 0x00, 0x99, 0x18, 0x7A, 0x92
        };
        #endregion

        public class When_compressing_http_headers_with_custom_dictionary : XUnit2UnitTestSpecificationAsync
        {
            private byte[] _compressedBytes = new byte[CompressedBytes.Length];

            public When_compressing_http_headers_with_custom_dictionary(
                ITestOutputHelper output) : base(output)
            {
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                var headerWriterProvider = DisposeAsyncOnTearDown(new HeaderWriterProvider());
                var pipe = new Pipe();
                var headerWriter = await headerWriterProvider
                                         .RequestLastWriterAsync(
                                             pipe.Writer, 
                                             cancellationToken)
                                         .ConfigureAwait(false);
                await using (headerWriter.ConfigureAwait(false))
                {
                    await headerWriter.WriteBytesAsync(UncompressedBytes, cancellationToken)
                                      .ConfigureAwait(false);
                }

                await using var memory = new MemoryStream(_compressedBytes);
                await pipe.Reader.CopyToAsync(memory, cancellationToken)
                          .ConfigureAwait(false);
            }

            [Fact]
            public void It_should_compressed()
            {
                _compressedBytes.Should().Equal(CompressedBytes);
            }
        }

        public class When_decompressing_http_headers_with_custom_dictionary : XUnit2UnitTestSpecificationAsync
        {
            private byte[] _decompressedBytes;
            private HeaderReader _reader;

            public When_decompressing_http_headers_with_custom_dictionary(
                ITestOutputHelper output) : base(output)
            {
            }

            protected override Task GivenAsync(
                CancellationToken cancellationToken)
            {
                var frameReader = new Mock<IFrameReader>();
                frameReader.Setup(
                               reader => reader.ReadBytesAsync(
                                   CompressedBytes.Length,
                                   It.IsAny<CancellationToken>()))
                           .Returns(new ValueTask<byte[]>(CompressedBytes));

                _reader = DisposeAsyncOnTearDown(new HeaderReader(
                    frameReader.Object));

                return Task.CompletedTask;
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                var reader = await _reader.RequestReaderAsync(
                                 CompressedBytes.Length, cancellationToken)
                             .ConfigureAwait(false);
                _decompressedBytes =
                    await reader.ReadBytesAsync(
                                     UncompressedBytes.Length,
                                     cancellationToken)
                                 .ConfigureAwait(false);
            }

            [Fact]
            public void It_should_have_decompressed()
            {
                _decompressedBytes.Should().Equal(UncompressedBytes);
            }
        }
    }
}
