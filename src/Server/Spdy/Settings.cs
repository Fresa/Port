using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Port.Server.Spdy.Extensions;
using Port.Server.Spdy.Primitives;

namespace Port.Server.Spdy
{
    public class Settings : Control
    {
        internal Settings(
            byte flags,
            IReadOnlyDictionary<Id, Setting> values) : base(Type)
        {
            Flags = flags;
            Values = values;
        }

        public const ushort Type = 4;

        /// <summary>
        /// Flags related to this frame. 
        /// </summary>
        protected new byte Flags
        {
            get => base.Flags;
            private set
            {
                if (value > 1)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(Flags),
                        $"Flags can only be 0 = none or 1 = {nameof(ClearSettings)}");
                }

                base.Flags = value;
            }
        }

        public bool ClearSettings => Flags == 1;

        public IReadOnlyDictionary<Id, Setting> Values { get; }

        internal static async ValueTask<Settings> ReadAsync(
            byte flags,
            UInt24 _,
            IFrameReader frameReader,
            CancellationToken cancellation = default)
        {
            // A 32-bit value representing the number of ID/value pairs in this message.
            var numberOfSettings = await frameReader
                .ReadUInt32Async(cancellation)
                .ConfigureAwait(false);
            var settings = new Dictionary<Id, Setting>();
            for (var i = 0; i < numberOfSettings; i++)
            {
                var settingFlags = await frameReader.ReadByteAsync(cancellation)
                    .ConfigureAwait(false);
                var id = await frameReader.ReadUInt24Async(cancellation)
                    .ToEnumAsync<Id>()
                    .ConfigureAwait(false);
                var value = await frameReader.ReadUInt32Async(cancellation)
                    .ConfigureAwait(false);
                settings.TryAdd(id, new Setting(settingFlags, value));
            }

            return new Settings(flags, settings);
        }

        protected override async ValueTask WriteControlFrameAsync(
            IFrameWriter frameWriter,
            CancellationToken cancellationToken = default)
        {
            var length = UInt24.From((uint)Values.Count * 8 + 4);
            await frameWriter.WriteUInt24Async(
                    length, cancellationToken)
                .ConfigureAwait(false);
            await frameWriter.WriteUInt32Async(
                    (uint)Values.Count, cancellationToken)
                .ConfigureAwait(false);
            foreach (var (id, setting) in Values.OrderBy(pair => pair.Key))
            {
                await frameWriter.WriteByteAsync(setting.Flags, cancellationToken)
                    .ConfigureAwait(false);
                await frameWriter.WriteUInt24Async(
                        UInt24.From((uint)id), cancellationToken)
                    .ConfigureAwait(false);
                await frameWriter.WriteUInt32Async(
                        setting.Value, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// +----------------------------------+
        /// | Flags(8) |      ID (24 bits)     |
        /// +----------------------------------+
        /// |          Value (32 bits)         |
        /// +----------------------------------+
        /// </summary>
        public class Setting
        {
            internal Setting(
                byte flags,
                uint value)
            {
                Flags = flags;
                Value = value;
            }

            public uint Value { get; }

            private byte _flags;

            public byte Flags
            {
                get => _flags;
                set
                {
                    if (value > 2)
                    {
                        throw new ArgumentOutOfRangeException(
                            nameof(Flags),
                            $"Flags can only be 0 = none, 1 = {nameof(PersistValue)} or 2 = {nameof(Persisted)}");
                    }

                    _flags = value;
                }
            }

            /// <summary>
            /// When set, the sender of this SETTINGS frame is requesting that the recipient persist the ID/Value and return it in future SETTINGS frames sent from the sender to this recipient. Because persistence is only implemented on the client, this flag is only sent by the server.
            /// </summary>
            public bool PersistValue => Flags == 1;

            /// <summary>
            /// When set, the sender is notifying the recipient that this ID/Value pair was previously sent to the sender by the recipient with the FLAG_SETTINGS_PERSIST_VALUE, and the sender is returning it. Because persistence is only implemented on the client, this flag is only sent by the client.
            /// </summary>
            public bool Persisted => Flags == 2;
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