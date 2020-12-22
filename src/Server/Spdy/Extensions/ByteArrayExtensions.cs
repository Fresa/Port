using System;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using Ionic.Zlib;
using Log.It;
using CompressionLevel = Ionic.Zlib.CompressionLevel;

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
                while (true)
                {
                    zStream.NextOut = 0;
                    zStream.AvailableBytesOut = buffer.Length;
                    result = zStream.Deflate(
                        zStream.TotalBytesIn == input.Length
                            ? FlushType.Finish
                            : FlushType.None);
                    stream.Write(
                        buffer, 0,
                        buffer.Length - zStream.AvailableBytesOut);

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