# Scene Management System Documentation

## Overview
This scene management system is designed for P2P multiplayer games using Unity's Netcode for GameObjects. It provides synchronized scene loading across all connected players with robust error handling and loading screens.

## Key Components

### 1. SceneData (ScriptableObject)
Defines scene configuration and network settings.

```csharp
// Create a new scene configuration
var sceneData = ScriptableObject.CreateInstance<SceneData>();
sceneData.SceneName = "GameplayScene";
sceneData.RequiresNetworkSync = true;
sceneData.LoadPriority = LoadPriority.High;
sceneData.SceneType = SceneType.Gameplay;
sceneData.NetworkTimeout = 15f;
```

### 2. NetworkSceneManager
Handles networked scene loading and P2P synchronization.

```csharp
// Example usage in a NetworkBehaviour
public class GameManager : NetworkBehaviour
{
    private NetworkSceneManager _networkSceneManager;

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            // Load a networked scene
            await _networkSceneManager.LoadSceneAsync(gameplaySceneData);
        }
    }
}
```

### 3. SceneLoadingManager
High-level manager for scene operations and loading screens.

```csharp
public class GameController : MonoBehaviour
{
    [SerializeField] private SceneData mainMenuScene;
    private SceneLoadingManager _sceneManager;

    public async Task LoadMainMenu()
    {
        await _sceneManager.LoadSceneAsync(mainMenuScene, showLoadingScreen: true);
    }
}
```

## Common Usage Scenarios

### 1. Loading a Gameplay Scene
```csharp
// In your game manager
public async Task StartMatch()
{
    if (!IsHost) return;

    try
    {
        bool success = await _sceneManager.LoadSceneAsync(gameplaySceneData);
        if (success)
        {
            Debug.Log("All players loaded into gameplay scene");
        }
    }
    catch (Exception e)
    {
        Debug.LogError($"Failed to load gameplay scene: {e}");
    }
}
```

### 2. Managing Scene Dependencies
```csharp
// In SceneData configuration
sceneData.Dependencies = new string[] 
{
    "CommonUI",
    "GameSystems"
};

// Dependencies will automatically load before the main scene
await _sceneManager.LoadSceneAsync(sceneData);
```

### 3. Handling Scene History
```csharp
// Go back to previous scene
public async Task ReturnToPreviousScene()
{
    await _sceneManager.LoadPreviousScene();
}
```

## Testing

1. Use the provided `SceneLoadingTest` component for verification:
```csharp
// Add to a test scene
var testObject = new GameObject("SceneLoadTest");
var test = testObject.AddComponent<SceneLoadingTest>();
```

2. Test Scene Setup:
- Create a test scene with NetworkManager
- Add SceneLoadingTest component
- Configure test SceneData asset
- Build and run with multiple clients

3. Verification Points:
- All clients should load simultaneously
- Loading screen appears/disappears correctly
- Scene dependencies load in order
- Network timeout works as expected
- Disconnected clients handled properly

## Best Practices

1. Scene Configuration:
- Keep SceneData assets in Resources folder
- Use meaningful scene names
- Set appropriate timeout values
- Configure dependencies correctly

2. Network Considerations:
- Only Host/Server should initiate scene loads
- Handle timeouts gracefully
- Consider bandwidth usage
- Test with varying network conditions

3. Performance:
- Use additive loading for common scenes
- Configure proper load priorities
- Implement asset preloading if needed
- Monitor memory usage during transitions

## Troubleshooting

Common Issues:
1. Scene load timeout
   - Check network conditions
   - Verify timeout settings
   - Ensure all clients are responding

2. Dependencies not loading
   - Verify SceneData configuration
   - Check Resources folder structure
   - Ensure scene names match exactly

3. Clients out of sync
   - Check NetworkSceneManager logs
   - Verify client connection status
   - Ensure proper error handling

## Integration with New Projects

1. Copy the SceneManagement folder to your project
2. Set up SceneData assets for your scenes
3. Initialize NetworkSceneManager in your network bootstrap
4. Configure loading screen prefab
5. Implement scene loading logic in your game manager
