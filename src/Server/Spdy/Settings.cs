using System.Collections.Generic;

namespace Port.Server.Spdy
{
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
}