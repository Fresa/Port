using Port.Server.Spdy.Frames;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy.Extensions
{
    internal static class ControlExtensions
    {
        internal static bool TryGetStreamId(
            this Control control,
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
            }

            streamId = default;
            return false;
        }
    }
}