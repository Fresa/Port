using System;
using System.Collections.Generic;

namespace Port.Server.Spdy
{
    public abstract class Frame
    {
        public abstract bool IsControlFrame { get; }
    }

    public class Data : Frame
    {
        public int StreamId { get; set; }
        public bool LastFrame => Flags == 1;
        public byte Flags { get; set; }
        public uint Length { get; set; }
        public byte[] Payload { get; set; } = new byte[0];
        public override bool IsControlFrame => false;
    }

    public abstract class Control : Frame
    {
        public sealed override bool IsControlFrame => true;
        public short Version { get; set; } = 3;
        public abstract short Type { get; }
        protected byte Flags { get; set; }
        protected uint Length { get; set; }
        protected byte[] Data { get; set; } = new byte[0];
    }

    public class SynStream : Control
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

    public class SynReply : Control
    {
        public override short Type => 2;
        public bool IsFin => Flags == 1;
        public int StreamId { get; set; }

        public Dictionary<string, string> Headers { get; set; } =
            new Dictionary<string, string>();
    }

    public class RstStream : Control
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
        public StatusCode Status { get; set; }

        public enum StatusCode
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

    public class Settings : Control
    {
        public override short Type => 4;
        public bool ClearSettings => Flags == 1;

        public Dictionary<Id, int> Values { get; set; } =
            new Dictionary<Id, int>();

        public enum Id
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

    public class Ping : Control
    {
        public override short Type => 6;
        public uint Id { get; set; }
    }

    public class GoAway : Control
    {
        public override short Type => 7;

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

        public int LastGoodStreamId { get; set; }
        public StatusCode Status { get; set; }

        public enum StatusCode
        {
            Ok = 0,
            ProtocolError = 1,
            InternalError = 2
        }
    }

    public class Headers : Control
    {
        public override short Type => 8;
        public bool IsLastFrame => Flags == 1;
        public Dictionary<string, string> Values { get; set; } =
            new Dictionary<string, string>();
    }

    public class WindowUpdate : Control
    {
        public override short Type => 9;
        protected new byte Flags
        {
            get => 0;
            set
            {
                if (value != 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(Flags), "Flags can only be 0");
                }
            }
        }
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

        private int _deltaWindowSize;
        public int DeltaWindowSize
        {
            get => _deltaWindowSize;
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(DeltaWindowSize), "Delta window size must be greater than 0");
                }

                _deltaWindowSize = value;
            }
        }
    }
}