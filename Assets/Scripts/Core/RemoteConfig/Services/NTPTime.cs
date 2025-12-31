using System;

namespace Core.RemoteConfig
{
    /// <summary>
    /// Static accessor for NTP-synchronized time.
    /// Usage: NTPTime.UtcNow (similar to DateTime.UtcNow)
    /// Automatically falls back to DateTime.UtcNow if NTP is not synced.
    /// </summary>
    public static class NTPTime
    {
        private static INTPTimeService _instance;

        /// <summary>
        /// Gets the current UTC time from NTP server (or fallback to local time if not synced)
        /// </summary>
        public static DateTime UtcNow => _instance?.IsSynced == true
            ? _instance.GetServerTime()
            : DateTime.UtcNow;

        /// <summary>
        /// Indicates whether NTP time has been successfully synchronized
        /// </summary>
        public static bool IsSynced => _instance?.IsSynced ?? false;

        /// <summary>
        /// Gets the time offset between device time and server time
        /// </summary>
        public static TimeSpan TimeOffset => _instance?.GetTimeOffset() ?? TimeSpan.Zero;

        /// <summary>
        /// Sets the NTPTimeService instance. Called by VContainer during initialization.
        /// </summary>
        public static void SetInstance(INTPTimeService service)
        {
            _instance = service;
        }

        /// <summary>
        /// Clears the instance (for testing or cleanup purposes)
        /// </summary>
        public static void ClearInstance()
        {
            _instance = null;
        }
    }
}
