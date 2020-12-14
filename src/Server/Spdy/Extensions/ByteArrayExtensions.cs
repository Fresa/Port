using System;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using Ionic.Zlib;

namespace Port.Server.Spdy.Extensions
{
    internal static class ByteArrayExtensions
    {
        internal static byte[] ZlibCompress(
            this byte[] input,
            byte[] dictionary)
        {
            if (input.Any() == false)
            {
                return Array.Empty<byte>();
            }

            using var stream = new MemoryStream();

            var buffer = new byte[1024];
            var zStream = new ZlibCodec
            {
                InputBuffer = input,
                AvailableBytesIn = input.Length,
                OutputBuffer = buffer
            };

            var result = zStream.InitializeDeflate(
                CompressionLevel.Default,
                zStream.WindowBits);
            if (result < 0)
            {
                throw new InvalidOperationException(
                    $"Got error code {result} when initializing deflate routine: {zStream.Message}");
            }

            result = zStream.SetDictionary(dictionary);
            if (result < 0)
            {
                throw new InvalidOperationException(
                    $"Got error code {result} when setting dictionary: {zStream.Message}");
            }

            try
            {
                var flush = FlushType.None;
                while (true)
                {
                    if (zStream.TotalBytesIn == input.Length)
                    {
                        flush = FlushType.Finish;
                    }

                    zStream.NextOut = 0;
                    zStream.AvailableBytesOut = buffer.Length;
                    result = zStream.Deflate(flush);
                    stream.Write(
                        buffer, 0, buffer.Length - zStream.AvailableBytesOut);

                    switch (result)
                    {
                        case ZlibConstants.Z_STREAM_END:
                            return stream.ToArray();
                        case var _ when result < 0:
                            throw new InvalidOperationException(
                                $"Got error code {result} when deflating the stream: {zStream.Message}");
                    }
                }
            }
            finally
            {
                zStream.EndDeflate();
            }
        }

        internal static byte[] ZlibDecompress(
            this byte[] input,
            byte[] dictionary)
        {
            if (input.Any() == false)
            {
                return Array.Empty<byte>();
            }
            using var stream = new MemoryStream();
            var buffer = new byte[1024];

            var zStream = new ZlibCodec
            {
                InputBuffer = input,
                AvailableBytesIn = input.Length,
                OutputBuffer = buffer
            };
            
            var result = zStream.InitializeInflate(zStream.WindowBits);
            if (result < 0)
            {
                throw new InvalidOperationException(
                    $"Got error code {result} when initializing inflate routine: {zStream.Message}");
            }

            try
            {
                while (true)
                {
                    zStream.NextOut = 0;
                    zStream.AvailableBytesOut = buffer.Length;
                    result = zStream.Inflate(FlushType.None);
                    stream.Write(
                        buffer, 0, buffer.Length - zStream.AvailableBytesOut);

                    switch (result)
                    {
                        case ZlibConstants.Z_STREAM_END:
                            return stream.ToArray();
                        case ZlibConstants.Z_NEED_DICT:
                            result = zStream.SetDictionary(dictionary);
                            if (result < 0)
                            {
                                throw new InvalidOperationException(
                                    $"Got error code {result} when setting dictionary: {zStream.Message}");
                            }

                            break;
                        case var _ when result < 0:
                            throw new InvalidOperationException(
                                $"Got error code {result} when deflating the stream: {zStream.Message}");
                    }
                }
            }
            finally
            {
                zStream.EndInflate();
            }
        }

        internal static IFrameReader ToFrameReader(
            this byte[] buffer)
        {
            return new FrameReader(PipeReader.Create(new MemoryStream(buffer)));
        }

        internal static string ToHexArrayRepresentation(
            this byte[] buffer)
        {
            return buffer
                .Aggregate(
                    "{ ", (
                            @byte,
                            representation)
                        => @byte + $"0x{representation:X2}, ")
                .TrimEnd()
                .TrimEnd(',') + " }";
        }
    }
}