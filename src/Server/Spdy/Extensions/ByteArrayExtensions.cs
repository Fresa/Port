using System;
using System.IO;
using System.IO.Pipelines;
using Elskom.Generic.Libs;

namespace Port.Server.Spdy.Extensions
{
    internal static class ByteArrayExtensions
    {
        internal static byte[] ZlibCompress(
            this byte[] input,
            byte[] dictionary)
        {
            var stream = new MemoryStream();
            
            var zStream = new ZStream
            {
                NextIn = input,
                NextInIndex = 0,
                AvailIn = input.Length
            };

            var result = zStream.DeflateInit(ZlibConst.ZDEFAULTCOMPRESSION, 11);
            if (result < 0)
            {
                throw new InvalidOperationException(
                    $"Got error code {result} when initializing deflate routine: {zStream.Msg}");
            }

            var buffer = new byte[1024];
            zStream.NextOut = buffer;

            try
            {
                var flush = ZlibConst.ZNOFLUSH;
                while (true)
                {
                    if (zStream.TotalIn == input.Length)
                    {
                        flush = ZlibConst.ZFINISH;
                    }
                    zStream.NextOutIndex = 0;
                    zStream.AvailOut = buffer.Length;
                    result = zStream.Deflate(flush);
                    stream.Write(buffer, 0, buffer.Length - zStream.AvailOut);

                    switch (result)
                    {
                        case ZlibConst.ZSTREAMEND:
                            return stream.ToArray();
                        case ZlibConst.ZNEEDDICT:
                            result = zStream.DeflateSetDictionary(
                                dictionary,
                                dictionary.Length);
                            if (result < 0)
                            {
                                throw new InvalidOperationException(
                                    $"Got error code {result} when setting dictionary: {zStream.Msg}");
                            }

                            break;
                        case var _ when result < 0:
                            throw new InvalidOperationException(
                                $"Got error code {result} when deflating the stream: {zStream.Msg}");
                    }
                }

            }
            finally
            {
                zStream.DeflateEnd();
            }
        }

        internal static IFrameReader ZlibDecompress(
            this byte[] input,
            byte[] dictionary)
        {
            var stream = new MemoryStream();
            var buffer = new byte[1024];

            var zStream = new ZStream
            {
                NextIn = input,
                NextInIndex = 0,
                AvailIn = input.Length
            };

            var result = zStream.InflateInit();
            if (result < 0)
            {
                throw new InvalidOperationException(
                    $"Got error code {result} when initializing inflate routine: {zStream.Msg}");
            }

            zStream.NextOut = buffer;

            try
            {
                while (true)
                {
                    zStream.NextOutIndex = 0;
                    zStream.AvailOut = buffer.Length;
                    result = zStream.Inflate(ZlibConst.ZNOFLUSH);
                    stream.Write(buffer, 0, buffer.Length - zStream.AvailOut);

                    switch (result)
                    {
                        case ZlibConst.ZSTREAMEND:
                            return new FrameReader(PipeReader.Create(stream));
                        case ZlibConst.ZNEEDDICT:
                            result = zStream.InflateSetDictionary(
                                dictionary,
                                dictionary.Length);
                            if (result < 0)
                            {
                                throw new InvalidOperationException(
                                    $"Got error code {result} when setting dictionary: {zStream.Msg}");
                            }

                            break;
                        case var _ when result < 0:
                            throw new InvalidOperationException(
                                $"Got error code {result} when deflating the stream: {zStream.Msg}");
                    }
                }
            }
            finally
            {
                zStream.InflateEnd();
            }
        }
    }
}