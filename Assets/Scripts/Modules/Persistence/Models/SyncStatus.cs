using System;
using Core.RemoteConfig;

namespace Modules.Persistence
{
    /// <summary>
    /// Tracks the synchronization status of cloud data.
    /// </summary>
    public class SyncStatus
    {
        public bool IsSyncing { get; set; }
        public bool HasPendingChanges { get; set; }
        public DateTime? LastSyncTime { get; set; }
        public string LastError { get; set; }
        
        public void MarkSyncStarted()
        {
            IsSyncing = true;
            LastError = null;
        }
        
        public void MarkSyncCompleted()
        {
            IsSyncing = false;
            HasPendingChanges = false;
            LastSyncTime = NTPTime.UtcNow;
            LastError = null;
        }
        
        public void MarkSyncFailed(string error)
        {
            IsSyncing = false;
            LastError = error;
        }
        
        public void MarkPendingChanges()
        {
            HasPendingChanges = true;
        }
    }
}
