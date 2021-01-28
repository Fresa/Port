using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Spdy.Extensions;
using Spdy.Frames.Readers;
using Spdy.Frames.Writers;
using Spdy.Primitives;

namespace Spdy.Frames
{
    /// <summary>
    /// A SETTINGS frame contains a set of id/value pairs for communicating configuration data about how the two endpoints may communicate. SETTINGS frames can be sent at any time by either endpoint, are optionally sent, and are fully asynchronous. When the server is the sender, the sender can request that configuration data be persisted by the client across SPDY sessions and returned to the server in future communications.
    /// 
    /// Persistence of SETTINGS ID/Value pairs is done on a per origin/IP pair (the "origin" is the set of scheme, host, and port from the URI. See RFC6454). That is, when a client connects to a server, and the server persists settings within the client, the client SHOULD return the persisted settings on future connections to the same origin AND IP address and TCP port. Clients MUST NOT request servers to use the persistence features of the SETTINGS frames, and servers MUST ignore persistence related flags sent by a client.
    /// 
    /// +----------------------------------+
    /// |1|   version    |         4       |
    /// +----------------------------------+
    /// | Flags (8)  |  Length (24 bits)   |
    /// +----------------------------------+
    /// |         Number of entries        |
    /// +----------------------------------+
    /// |          ID/Value Pairs          |
    /// |             ...                  |
    /// </summary>
    public class Settings : Control
    {
        private Settings(
            Options flags,
            params Setting[] values)
            : base(Type)
        {
            Flags = flags;
            Values = values.ToDictionary(
                               setting => setting.Id, setting => setting)
                           .Values;
        }

        public Settings(
            params Setting[] values)
            : this(Options.None, values)
        {
        }

        public static Settings Clear(
            params Setting[] values) => new Settings(
            Options.ClearSettings, values);

        public const ushort Type = 4;

        /// <summary>
        /// Flags related to this frame. 
        /// </summary>
        protected new Options Flags
        {
            get => (Options) base.Flags;
            private set => base.Flags = (byte) value;
        }

        [Flags]
        public enum Options : byte
        {
            None = 0,

            /// <summary>
            /// When set, the client should clear any previously persisted SETTINGS ID/Value pairs. If this frame contains ID/Value pairs with the FLAG_SETTINGS_PERSIST_VALUE set, then the client will first clear its existing, persisted settings, and then persist the values with the flag set which are contained within this frame. Because persistence is only implemented on the client, this flag can only be used when the sender is the server.
            /// </summary>
            ClearSettings = 1
        }

        public bool ClearSettings => Flags.HasFlag(Options.ClearSettings);

        public IReadOnlyCollection<Setting> Values { get; }

        internal static async ValueTask<ReadResult<Settings>> TryReadAsync(
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
                var flag = await frameReader.ReadByteAsync(cancellation)
                                            .ToEnumAsync<ValueOptions>()
                                            .ConfigureAwait(false);
                var id = await frameReader.ReadUInt24Async(cancellation)
                                          .ToEnumAsync<Id>()
                                          .ConfigureAwait(false);
                var value = await frameReader.ReadUInt32Async(cancellation)
                                             .ConfigureAwait(false);
                settings.TryAdd(id, new Setting(id, flag, value));
            }

            return ReadResult.Ok(
                new Settings(
                    flags.ToEnum<Options>(), settings.Values.ToArray()));
        }

        protected override async ValueTask WriteControlFrameAsync(
            IFrameWriter frameWriter,
            IHeaderWriterProvider _,
            CancellationToken cancellationToken = default)
        {
            var length = UInt24.From((uint) Values.Count * 8 + 4);
            await frameWriter.WriteUInt24Async(length, cancellationToken)
                             .ConfigureAwait(false);
            await frameWriter.WriteUInt32Async(
                                 (uint) Values.Count, cancellationToken)
                             .ConfigureAwait(false);
            foreach (var setting in Values.OrderBy(setting => setting.Id))
            {
                await frameWriter.WriteByteAsync(
                                     (byte) setting.Flags, cancellationToken)
                                 .ConfigureAwait(false);
                await frameWriter.WriteUInt24Async(
                                     UInt24.From((uint) setting.Id),
                                     cancellationToken)
                                 .ConfigureAwait(false);
                await frameWriter.WriteUInt32Async(
                                     setting.Value, cancellationToken)
                                 .ConfigureAwait(false);
            }
        }

        public static Setting ClientCertificateVectorSize(
            uint value,
            ValueOptions flags = ValueOptions.None)
            => new Setting(
                Id.ClientCertificateVectorSize,
                flags,
                value);


        public static Setting CurrentCwnd(
            uint value,
            ValueOptions flags = ValueOptions.None)
            => new Setting(
                Id.CurrentCwnd,
                flags,
                value);

        public static Setting DownloadBandwidth(
            uint value,
            ValueOptions flags = ValueOptions.None)
            => new Setting(
                Id.DownloadBandwidth,
                flags,
                value);

        public static Setting DownloadRetransRate(
            uint value,
            ValueOptions flags = ValueOptions.None)
            => new Setting(
                Id.DownloadRetransRate,
                flags,
                value);

        public static Setting InitialWindowSize(
            uint value,
            ValueOptions flags = ValueOptions.None)
            => new Setting(
                Id.InitialWindowSize,
                flags,
                value);

        public static Setting MaxConcurrentStreams(
            uint value,
            ValueOptions flags = ValueOptions.None)
            => new Setting(
                Id.MaxConcurrentStreams,
                flags,
                value);

        public static Setting RoundTripTime(
            uint value,
            ValueOptions flags = ValueOptions.None)
            => new Setting(
                Id.RoundTripTime,
                flags,
                value);

        public static Setting UploadBandwidth(
            uint value,
            ValueOptions flags = ValueOptions.None)
            => new Setting(
                Id.UploadBandwidth,
                flags,
                value);

        /// <summary>
        /// +----------------------------------+
        /// | Flags(8) |      ID (24 bits)     |
        /// +----------------------------------+
        /// |          Value (32 bits)         |
        /// +----------------------------------+
        /// </summary>
        public sealed class Setting
        {
            internal Setting(
                Id id,
                ValueOptions flags,
                uint value)
            {
                Id = id;
                Flags = flags;
                Value = value;
            }

            public Id Id { get; }

            public uint Value { get; }

            /// <summary>
            /// When set, the sender of this SETTINGS frame is requesting that the recipient persist the ID/Value and return it in future SETTINGS frames sent from the sender to this recipient. Because persistence is only implemented on the client, this flag is only sent by the server.
            /// </summary>
            public bool ShouldPersist => Flags == ValueOptions.PersistValue;

            /// <summary>
            /// When set, the sender is notifying the recipient that this ID/Value pair was previously sent to the sender by the recipient with the FLAG_SETTINGS_PERSIST_VALUE, and the sender is returning it. Because persistence is only implemented on the client, this flag is only sent by the client.
            /// </summary>
            public bool IsPersisted => Flags == ValueOptions.Persisted;

            public ValueOptions Flags { get; }
        }

        public enum ValueOptions : ushort
        {
            None = 0,

            /// <summary>
            /// When set, the sender of this SETTINGS frame is requesting that the recipient persist the ID/Value and return it in future SETTINGS frames sent from the sender to this recipient. Because persistence is only implemented on the client, this flag is only sent by the server.
            /// </summary>
            PersistValue = 1,

            /// <summary>
            /// When set, the sender is notifying the recipient that this ID/Value pair was previously sent to the sender by the recipient with the FLAG_SETTINGS_PERSIST_VALUE, and the sender is returning it. Because persistence is only implemented on the client, this flag is only sent by the client.
            /// </summary>
            Persisted = 2
        }

        public enum Id : uint
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