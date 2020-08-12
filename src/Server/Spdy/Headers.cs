using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Extensions;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    public class Headers : Control
    {
        public Headers(
            byte flags,
            UInt31 streamId,
            IReadOnlyDictionary<string, string> values)
        {
            Flags = flags;
            StreamId = streamId;
            Values = values;
        }

        public const ushort Type = 8;

        /// <summary>
        /// Flags related to this frame. 
        /// </summary>
        private new byte Flags
        {
            get => base.Flags;
            set
            {
                if (value > 1)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(Flags),
                        $"Flags can only be 0 = none or 1 = {nameof(IsLastFrame)}");
                }

                base.Flags = value;
            }
        }

        /// <summary>
        /// Marks this frame as the last frame to be transmitted on this stream and puts the sender in the half-closed (Section 2.3.6) state.
        /// </summary>
        public bool IsLastFrame => Flags == 1;

        /// <summary>
        /// The stream this HEADERS block is associated with.
        /// </summary>
        public UInt31 StreamId { get; }

        /// <summary>
        /// A set of name/value pairs carried as part of the SYN_STREAM. see Name/Value Header Block (Section 2.6.10).
        /// </summary>
        public IReadOnlyDictionary<string, string> Values { get; }

        internal static async ValueTask<Headers> ReadAsync(
            byte flags,
            UInt24 length,
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            var streamId = UInt31.From(
                await frameReader.ReadUInt32Async(cancellation)
                    .ConfigureAwait(false) & 0x7FFF);
            // An unsigned 24 bit value representing the number of bytes after the length field. The minimum length of the length field is 4 (when the number of name value pairs is 0).
            var headerLength = (int)length.Value - 4;
            var values =
                await
                    (await frameReader
                        .ReadBytesAsync(headerLength, cancellation)
                        .ConfigureAwait(false))
                    .ZlibDecompress(SpdyConstants.HeadersDictionary)
                    .AsFrameReader()
                    .ReadNameValuePairs(cancellation)
                    .ConfigureAwait(false);

            return new Headers(flags, streamId, values);
        }

        protected override async ValueTask WriteFrameAsync(
            IFrameWriter frameWriter,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}