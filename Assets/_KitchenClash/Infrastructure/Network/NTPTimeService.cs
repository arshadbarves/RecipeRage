using System;
using System.Net;
using System.Net.Sockets;
using KitchenClash.Infrastructure.Logging;
using KitchenClash.Application.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace KitchenClash.Infrastructure.Network
{
    public class NTPTimeService : INTPTimeService, IInitializable
    {
        private static readonly string[] NTP_SERVERS = new[]
        {
            "time.google.com", "pool.ntp.org", "time.windows.com", "time.apple.com", "time.cloudflare.com"
        };

        private const int NTP_PORT = 123;
        private const int NTP_TIMEOUT_MS = 3000;

        private TimeSpan _timeOffset;
        private DateTime _lastSyncTime;
        private bool _isSynced;

        public bool IsSynced => _isSynced;
        public DateTime LastSyncTime => _lastSyncTime;

        public void Initialize()
        {
            NTPTime.SetInstance(this);
        }

        public async UniTask<bool> SyncTime()
        {
            for (int i = 0; i < NTP_SERVERS.Length; i++)
            {
                try
                {
                    DateTime ntpTime = await FetchNTPTime(NTP_SERVERS[i]);
                    _timeOffset = ntpTime - DateTime.UtcNow;
                    _lastSyncTime = DateTime.UtcNow;
                    _isSynced = true;
                    GameLogger.Log($"NTP sync successful: {NTP_SERVERS[i]}, Offset: {_timeOffset.TotalSeconds:F2}s");
                    return true;
                }
                catch { }
            }
            return false;
        }

        public DateTime GetServerTime() => _isSynced ? DateTime.UtcNow + _timeOffset : DateTime.UtcNow;
        public TimeSpan GetTimeOffset() => _timeOffset;

        private async UniTask<DateTime> FetchNTPTime(string server)
        {
            byte[] ntpData = new byte[48];
            ntpData[0] = 0x1B;

            IPAddress[] addresses = await Dns.GetHostAddressesAsync(server);
            if (addresses.Length == 0) throw new Exception($"Cannot resolve: {server}");

            IPAddress addr = null;
            foreach (var a in addresses)
                if (a.AddressFamily == AddressFamily.InterNetwork) { addr = a; break; }
            addr ??= addresses[0];

            using var socket = new Socket(addr.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.ReceiveTimeout = NTP_TIMEOUT_MS;
            socket.SendTimeout = NTP_TIMEOUT_MS;
            await socket.ConnectAsync(new IPEndPoint(addr, NTP_PORT));
            await socket.SendAsync(new ArraySegment<byte>(ntpData), SocketFlags.None);

            byte[] buf = new byte[48];
            await socket.ReceiveAsync(new ArraySegment<byte>(buf), SocketFlags.None);

            ulong intPart = SwapEndianness(System.BitConverter.ToUInt32(buf, 40));
            ulong fractPart = SwapEndianness(System.BitConverter.ToUInt32(buf, 44));
            ulong ms = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
            return new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(ms);
        }

        private static uint SwapEndianness(ulong v) =>
            (uint)(((v & 0x000000ff) << 24) + ((v & 0x0000ff00) << 8) + ((v & 0x00ff0000) >> 8) + ((v & 0xff000000) >> 24));
    }
}
