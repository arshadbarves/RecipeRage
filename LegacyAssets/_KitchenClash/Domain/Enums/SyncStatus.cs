namespace KitchenClash.Domain
{
    public class SyncStatus
    {
        public SyncState State { get; private set; } = SyncState.NotSynced;
        public bool IsSyncing => State == SyncState.Syncing;
        public bool IsSynced => State == SyncState.Synced;
        public string ErrorMessage { get; private set; }

        public void MarkPendingChanges() => State = SyncState.NotSynced;
        public void MarkSyncStarted() => State = SyncState.Syncing;
        public void MarkSyncCompleted() { State = SyncState.Synced; ErrorMessage = null; }
        public void MarkSyncFailed(string error) { State = SyncState.Error; ErrorMessage = error; }
    }

    public enum SyncState
    {
        NotSynced,
        Syncing,
        Synced,
        Error
    }
}
