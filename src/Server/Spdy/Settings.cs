using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Port.Server.Spdy
{
    public class Settings : Control
    {
        public Settings(
            byte flags)
            : base(flags)
        {
        }

        public const ushort Type = 4;
        public bool ClearSettings => Flags == 1;

        public Dictionary<Id, int> Values { get; set; } =
            new Dictionary<Id, int>();

        internal static async ValueTask<Settings> ReadAsync(
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

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