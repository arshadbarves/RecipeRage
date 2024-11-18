Example usage:

```csharp
// Example component using AssetManager
public class GameObjectSpawner : MonoBehaviour
{
    [Inject] private AssetManager assetManager;
    
    [SerializeField] private string prefabKey;
    
    private async Task SpawnObject()
    {
        try
        {
            GameObject prefab = await assetManager.LoadAsset<GameObject>(prefabKey);
            if (prefab != null)
            {
                GameObject instance = await assetManager.InstantiatePrefab(
                    prefabKey, 
                    transform.position, 
                    Quaternion.identity
                );
                
                // Configure spawned object
                if (instance != null)
                {
                    // Setup component references, etc.
                }
            }
        }
        catch (AssetManager.AssetLoadException e)
        {
            Debug.LogError($"Failed to spawn object: {e.Message}");
        }
    }
}

// Installer setup
public class GameFrameworkInstaller : MonoInstaller
{
    public override void Install(IContainerBuilder builder)
    {
        // Register asset management system
        builder.Register<IAssetLoader, AddressableLoader>(Lifetime.Singleton);
        builder.Register<AssetManager>(Lifetime.Singleton);
    }
}
```

This Asset Management system provides:

1. Asynchronous asset loading
2. Addressables integration
3. Asset caching
4. Memory management
5. Progress tracking
6. Error handling
7. Instance management
8. Resource cleanup

Key features:

- Async/await pattern for clean code
- Prevention of duplicate loading
- Automatic cache management
- Resource cleanup
- Loading progress tracking
- Error handling and logging
- Memory leak prevention
- Type-safe asset loading