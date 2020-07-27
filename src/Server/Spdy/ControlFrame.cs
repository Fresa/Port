using System;
using System.Collections.Generic;

namespace Port.Server.Spdy
{
    public abstract class Frame
    {
        public abstract bool IsControlFrame { get; }
    }

    public class DataFrame : Frame
    {
        public int StreamId { get; set; }
        public bool LastFrame => Flags == 1;
        public byte Flags { get; set; }
        public uint Length { get; set; }
        public byte[] Data { get; set; } = new byte[0];
        public override bool IsControlFrame => false;
    }

    public abstract class ControlFrame : Frame
    {
        public sealed override bool IsControlFrame => true;
        public short Version { get; set; } = 3;
        public abstract short Type { get; }
        protected byte Flags { get; set; }
        protected uint Length { get; set; }
        protected byte[] Data { get; set; } = new byte[0];
    }

    public class SynStreamFrame : ControlFrame
    {
        public override short Type => 1;
        public bool IsFin => Flags == 1;
        public bool IsUnidirectional => Flags == 2;
        public int StreamId { get; set; }
        public int AssociatedToStreamId { get; set; }
        public bool IsIndependentStream => AssociatedToStreamId == 0;
        public ushort Priority { get; set; }

        public Dictionary<string, string> Headers { get; set; } =
            new Dictionary<string, string>();
    }

    public class SynReplyFrame : ControlFrame
    {
        public override short Type => 2;
        public bool IsFin => Flags == 1;
        public int StreamId { get; set; }

        public Dictionary<string, string> Headers { get; set; } =
            new Dictionary<string, string>();
    }

    public class RstStreamFrame : ControlFrame
    {
        public override short Type => 3;

        protected new uint Length
        {
            get => 8;
            set
            {
                if (value != 8)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(Length), "Length can only be 8");
                }
            }
        }

        public int StreamId { get; set; }
        public RstStreamStatusCode StatusCode { get; set; }

        public enum RstStreamStatusCode
        {
            ProtocolError = 1,
            InvalidStream = 2,
            RefusedStream = 3,
            UnsupportedVersion = 4,
            Cancel = 5,
            InternalError = 6,
            FlowControlError = 7,
            StreamInUse = 8,
            StreamAlreadyClosed = 9,
            Unused = 10,
            FrameToLarge = 11
        }
    }

    public class SettingsFrame : ControlFrame
    {
        public override short Type => 4;
        public bool ClearSettings => Flags == 1;

        public Dictionary<SettingsId, int> Settings { get; set; } =
            new Dictionary<SettingsId, int>();
        public enum SettingsId
        {
            UploadBandwidth = 1,
            DownloadBandwidth = 2,
            RoundTripTime = 3,
            MaxConcurrentStreams = 4,
            CurrentCwnd = 5,
            DownloadRetransRate = 6,
            InitialWindowSize = 7,
            ClientCertificateVectorSize = 8
        }
    }
}