using System;
using System.IO;
using System.IO.Pipelines;
using Elskom.Generic.Libs;

namespace Port.Server.Spdy.Extensions
{
    internal static class ByteArrayExtensions
    {
        internal static IFrameReader ZlibInflate(
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

            while (true)
            {
                zStream.NextOut = buffer;
                zStream.NextOutIndex = 0;
                zStream.AvailOut = buffer.Length;
                var result = zStream.Inflate(ZlibConst.ZNOFLUSH);
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
                                $"Got error code {result} when setting dictionary");
                        }

                        break;
                    case var _ when result < 0:
                        throw new InvalidOperationException(
                            $"Got error code {result} when deflating the stream");
                }
            }
        }
    }
}