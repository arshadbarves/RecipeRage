using System;
using System.Net;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using Core.Logging;
using UnityEngine;

namespace Core.RemoteConfig
{
    /// <summary>
    /// Service for synchronizing time with NTP servers
    /// Provides reliable server time for rotation schedules and time-based features
    /// </summary>
    public class NTPTimeService : INTPTimeService
    {
        // Multiple NTP servers for fallback
        private static readonly string[] NTP_SERVERS = new[]
        {
            "time.google.com",      // Google (most reliable)
            "pool.ntp.org",         // NTP Pool Project
            "time.windows.com",     // Microsoft
            "time.apple.com",       // Apple
            "time.cloudflare.com"   // Cloudflare
        };

        private const int NTP_PORT = 123;
        private const int NTP_TIMEOUT_MS = 3000; // Reduced from 5000ms
        private const int RESYNC_INTERVAL_HOURS = 6;
        private const int MAX_RETRY_ATTEMPTS = 2; // Reduced from 3
        private const int BASE_RETRY_DELAY_MS = 500; // Reduced from 1000ms

        private TimeSpan _timeOffset;
        private DateTime _lastSyncTime;
        private bool _isSynced;
        private bool _isAutoSyncEnabled;

        public bool IsSynced => _isSynced;
        public DateTime LastSyncTime => _lastSyncTime;

        public NTPTimeService()
        {
            _timeOffset = TimeSpan.Zero;
            _lastSyncTime = DateTime.MinValue;
            _isSynced = false;
            _isAutoSyncEnabled = false;
        }

        /// <summary>
        /// Synchronizes time with NTP server (tries multiple servers)
        /// </summary>
        public async UniTask<bool> SyncTime()
        {
            // Try each NTP server
            for (int serverIndex = 0; serverIndex < NTP_SERVERS.Length; serverIndex++)
            {
                string server = NTP_SERVERS[serverIndex];

                for (int attempt = 0; attempt < MAX_RETRY_ATTEMPTS; attempt++)
                {
                    try
                    {
                        GameLogger.Log($"NTP sync: Server [{serverIndex + 1}/{NTP_SERVERS.Length}] {server}, Attempt {attempt + 1}/{MAX_RETRY_ATTEMPTS}");

                        var ntpTime = await FetchNTPTime(server);
                        var localTime = DateTime.UtcNow;

                        _timeOffset = ntpTime - localTime;
                        _lastSyncTime = localTime;
                        _isSynced = true;

                        GameLogger.Log($"✓ NTP sync successful! Server: {server}, Offset: {_timeOffset.TotalSeconds:F2}s");

                        // Start auto-sync if not already running
                        if (!_isAutoSyncEnabled)
                        {
                            _isAutoSyncEnabled = true;
                            StartAutoSync().Forget();
                        }

                        return true;
                    }
                    catch (Exception ex)
                    {
                        GameLogger.LogWarning($"✗ NTP sync failed: {server} (attempt {attempt + 1}) - {ex.Message.Split('\n')[0]}");

                        if (attempt < MAX_RETRY_ATTEMPTS - 1)
                        {
                            // Short delay before retry
                            await UniTask.Delay(BASE_RETRY_DELAY_MS);
                        }
                    }
                }

                // If we tried all attempts for this server, move to next server
                GameLogger.LogWarning($"Server {server} failed after {MAX_RETRY_ATTEMPTS} attempts. Trying next server...");
            }

            GameLogger.LogWarning($"NTP sync failed after trying all {NTP_SERVERS.Length} servers. Using local time.");
            return false;
        }

        /// <summary>
        /// Gets the current server time based on NTP synchronization
        /// </summary>
        public DateTime GetServerTime()
        {
            if (!_isSynced)
            {
                GameLogger.LogWarning("NTP not synced, returning local time");
                return DateTime.UtcNow;
            }

            return DateTime.UtcNow + _timeOffset;
        }

        /// <summary>
        /// Gets the calculated time offset between device time and server time
        /// </summary>
        public TimeSpan GetTimeOffset()
        {
            return _timeOffset;
        }

        /// <summary>
        /// Fetches time from NTP server
        /// </summary>
        private async UniTask<DateTime> FetchNTPTime(string ntpServer)
        {
            // NTP message format (48 bytes)
            var ntpData = new byte[48];
            ntpData[0] = 0x1B; // LI = 0 (no warning), VN = 3 (IPv4), Mode = 3 (Client)

            IPAddress[] addresses = await Dns.GetHostAddressesAsync(ntpServer);
            if (addresses.Length == 0)
            {
                throw new Exception($"Could not resolve NTP server: {ntpServer}");
            }

            var ipEndPoint = new IPEndPoint(addresses[0], NTP_PORT);

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.ReceiveTimeout = NTP_TIMEOUT_MS;
                socket.SendTimeout = NTP_TIMEOUT_MS;

                await socket.ConnectAsync(ipEndPoint);
                await socket.SendAsync(new ArraySegment<byte>(ntpData), SocketFlags.None);

                var receiveBuffer = new byte[48];
                int bytesReceived = await socket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), SocketFlags.None);

                if (bytesReceived < 48)
                {
                    throw new Exception($"Invalid NTP response size: {bytesReceived}");
                }

                // Extract timestamp from bytes 40-43 (Transmit Timestamp)
                ulong intPart = BitConverter.ToUInt32(receiveBuffer, 40);
                ulong fractPart = BitConverter.ToUInt32(receiveBuffer, 44);

                // Convert from network byte order (big-endian) to host byte order
                intPart = SwapEndianness(intPart);
                fractPart = SwapEndianness(fractPart);

                // NTP timestamp is seconds since 1900-01-01
                var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
                var ntpEpoch = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                return ntpEpoch.AddMilliseconds(milliseconds);
            }
        }

        /// <summary>
        /// Swaps byte order for endianness conversion
        /// </summary>
        private uint SwapEndianness(ulong value)
        {
            return (uint)(((value & 0x000000ff) << 24) +
                          ((value & 0x0000ff00) << 8) +
                          ((value & 0x00ff0000) >> 8) +
                          ((value & 0xff000000) >> 24));
        }

        /// <summary>
        /// Automatically re-syncs time every 6 hours during active gameplay
        /// </summary>
        private async UniTaskVoid StartAutoSync()
        {
            while (_isAutoSyncEnabled && Application.isPlaying)
            {
                await UniTask.Delay(TimeSpan.FromHours(RESYNC_INTERVAL_HOURS));

                if (_isAutoSyncEnabled && Application.isPlaying)
                {
                    GameLogger.Log("Performing automatic NTP re-sync...");
                    await SyncTime();
                }
            }
        }

        /// <summary>
        /// Stops automatic synchronization
        /// </summary>
        public void StopAutoSync()
        {
            _isAutoSyncEnabled = false;
        }
    }
}
