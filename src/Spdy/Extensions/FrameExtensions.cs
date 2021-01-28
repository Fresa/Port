using Spdy.Frames;
using Spdy.Primitives;

namespace Spdy.Extensions
{
    internal static class FrameExtensions
    {
        internal static bool TryGetStreamId(
            this Frame control,
            out UInt31 streamId)
        {
            switch (control)
            {
                case Headers headers:
                    streamId = headers.StreamId;
                    return true;
                case RstStream rstStream:
                    streamId = rstStream.StreamId;
                    return true;
                case SynReply synReply:
                    streamId = synReply.StreamId;
                    return true;
                case SynStream synStream:
                    streamId = synStream.StreamId;
                    return true;
                case Data data:
                    streamId = data.StreamId;
                    return true;
            }

            streamId = default;
            return false;
        }

        internal static object ToStructuredLogging(
            this Frame frame)
            => frame switch
            {
                Data data => new
                {
                    data.StreamId,
                    data.IsLastFrame,
                    PayloadSize = data.Payload.Length
                },
                _ => frame
            };
    }
}