// Example usage in GameManager
public class GameManager : NetworkBehaviour
{
[Inject] private SceneLoadManager sceneLoadManager;
[Inject] private EventManager eventManager;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            // Subscribe to events
            eventManager.Subscribe<SceneLoadCompleteEvent>(this, OnSceneLoadComplete);
        }
    }

    private async Task StartGame()
    {
        if (IsServer)
        {
            await sceneLoadManager.LoadScene(SceneConfig.GameScene.Kitchen1);
        }
    }

    private void OnSceneLoadComplete(SceneLoadCompleteEvent evt)
    {
        if (evt.LoadedScene == SceneConfig.GameScene.Kitchen1)
        {
            // All clients have loaded, start the game
            StartGameplay();
        }
    }

    private void StartGameplay()
    {
        // Initialize gameplay elements
        // Spawn players
        // Start countdown
        // etc.
    }

}

// LoadingScreen example
public class LoadingScreen : MonoBehaviour
{
[Inject] private SceneLoadManager sceneLoadManager;
[Inject] private EventManager eventManager;

    [SerializeField] private UnityEngine.UI.Slider progressBar;
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    [SerializeField] private TMPro.TextMeshProUGUI operationText;
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Progress Bars")]
    [SerializeField] private UnityEngine.UI.Slider assetProgress;
    [SerializeField] private UnityEngine.UI.Slider sceneProgress;
    [SerializeField] private UnityEngine.UI.Slider clientProgress;

    private void Start()
    {
        eventManager.Subscribe<SceneLoadStartEvent>(this, OnSceneLoadStart);
        InitializeUI();
    }

    private void InitializeUI()
    {
        progressBar.value = 0f;
        if (assetProgress != null) assetProgress.value = 0f;
        if (sceneProgress != null) sceneProgress.value = 0f;
        if (clientProgress != null) clientProgress.value = 0f;
    }

    private void Update()
    {
        var progress = sceneLoadManager.LoadProgress;

        // Update detailed progress bars
        if (assetProgress != null)
            assetProgress.value = Mathf.Lerp(assetProgress.value, progress.AssetLoadProgress, Time.deltaTime * smoothSpeed);

        if (sceneProgress != null)
            sceneProgress.value = Mathf.Lerp(sceneProgress.value, progress.SceneLoadProgress, Time.deltaTime * smoothSpeed);

        if (clientProgress != null)
            clientProgress.value = Mathf.Lerp(clientProgress.value,
                progress.TotalClients > 0 ? (float)progress.ClientsLoaded / progress.TotalClients : 0f,
                Time.deltaTime * smoothSpeed);

        // Update main progress bar
        progressBar.value = Mathf.Lerp(progressBar.value, progress.TotalProgress, Time.deltaTime * smoothSpeed);

        // Update status texts
        UpdateStatusText(progress);
    }

    private void UpdateStatusText(SceneLoadProgress progress)
        {
            string networkStatus = networkManager.IsListening
                ? $"Players Ready: {progress.ClientsLoaded}/{progress.TotalClients}"
                : "Local Mode";

            statusText.text = $"Loading... {(progressBar.value * 100):F0}%\n{networkStatus}";

            if (operationText != null)
                operationText.text = progress.CurrentOperation;
        }

    private void OnSceneLoadStart(SceneLoadStartEvent evt)
    {
        InitializeUI();
        statusText.text = $"Loading {evt.TargetScene}...";
    }

    private void OnDestroy()
    {
        eventManager.Unsubscribe(this);
    }

}

```

This scene management system provides:

1. Synchronized scene loading across all clients
2. Loading screen support
3. Progress tracking
4. Client load status tracking
5. Scene configuration through ScriptableObjects
6. Event-based communication
7. Timeout handling for disconnected clients

Key features:
- Waits for all clients to load before proceeding
- Handles client disconnections during loading
- Shows loading progress
- Supports different loading modes per scene
- Integration with the event system
- Scene validation and error handling

To use it:
1. Create a SceneConfig ScriptableObject and configure your scenes
2. Register the SceneLoadManager in your dependency injection container
3. Use LoadScene method to change scenes
4. Subscribe to scene loading events to respond to load states
5. Use the loading screen prefab to show loading progress
```

This corrected version:

1. Properly handles the SceneEventProgressStatus
2. Provides more accurate loading progress tracking
3. Adds smooth progress bar updates
4. Includes better error handling
5. Shows loading status for both local and networked scene loading
6. Displays player connection status during loading

The loading progress is now calculated based on:

- Number of clients that have loaded the scene
- Scene loading progress (when available)
- Additional loading factors you might want to add
