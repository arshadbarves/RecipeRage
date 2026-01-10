using System;
using Cysharp.Threading.Tasks;

namespace Core.RemoteConfig.Interfaces
{
    /// <summary>
    /// Interface for NTP time synchronization service
    /// Provides reliable server time for rotation schedules and time-based features
    /// </summary>
    public interface INTPTimeService
    {
        /// <summary>
        /// Synchronizes time with NTP server
        /// </summary>
        /// <returns>True if synchronization succeeded, false otherwise</returns>
        UniTask<bool> SyncTime();

        /// <summary>
        /// Gets the current server time based on NTP synchronization
        /// </summary>
        /// <returns>Current server time in UTC</returns>
        DateTime GetServerTime();

        /// <summary>
        /// Gets the calculated time offset between device time and server time
        /// </summary>
        /// <returns>Time offset as TimeSpan</returns>
        TimeSpan GetTimeOffset();

        /// <summary>
        /// Indicates whether time has been successfully synchronized
        /// </summary>
        bool IsSynced { get; }

        /// <summary>
        /// Timestamp of the last successful synchronization
        /// </summary>
        DateTime LastSyncTime { get; }
    }
}
