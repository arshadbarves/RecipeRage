namespace Core.GameFramework.Scene
{
    public class SceneLoadProgress
    {
        public float AssetLoadingProgress { get; set; }
        public float SceneLoadingProgress { get; set; }
        public float ClientSyncProgress { get; set; }
        public int ClientsLoaded { get; set; }
        public int TotalClients { get; set; }
        public string CurrentOperation { get; set; }
        public bool IsComplete { get; set; }

        public float TotalProgress {
            get {
                float clientProgress = TotalClients > 0 ? (float)ClientsLoaded / TotalClients : 1f;
                return (AssetLoadingProgress + SceneLoadingProgress + clientProgress) / 3f;
            }
        }
    }
}