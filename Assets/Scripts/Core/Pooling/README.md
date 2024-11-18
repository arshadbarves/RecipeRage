# Network Object Pooling System Documentation

## Overview
A network-aware object pooling system built on Unity's ObjectPool, optimized for mobile P2P games. Provides efficient object reuse with network synchronization.

## Key Features
- Built on Unity's ObjectPool for performance
- Network synchronization support
- Mobile-optimized memory management
- Pre-warming capability
- Automatic cleanup
- Server-authoritative spawning

## Components

### 1. NetworkObjectPool
Main pooling system manager:

```csharp
// Get singleton instance
NetworkObjectPool pool = NetworkObjectPool.Instance;

// Configure in inspector
[SerializeField] private PoolConfig[] poolConfigs;
```

### 2. PoolConfig
Configuration for each pooled prefab:

```csharp
[Serializable]
public class PoolConfig
{
    public NetworkObject Prefab;
    public int DefaultCapacity = 10;
    public int MaxSize = 20;
    public bool AutoExpand;
}
```

### 3. PooledNetworkObject
Helper component for pool management:

```csharp
// Automatically added to pooled objects
var pooled = obj.GetComponent<PooledNetworkObject>();
pooled.ReturnToPool(); // Return to pool
```

## Usage Examples

### 1. Basic Setup
```csharp
// 1. Create empty GameObject in scene
// 2. Add NetworkObjectPool component
// 3. Configure pool in inspector
// 4. Add prefabs to PoolConfig array
```

### 2. Spawning Objects
```csharp
// Server-side spawning
if (IsServer)
{
    NetworkObject obj = NetworkObjectPool.Instance.GetNetworkObject(
        prefab,
        position,
        rotation
    );
}
```

### 3. Returning Objects
```csharp
// Method 1: Using PooledNetworkObject
obj.GetComponent<PooledNetworkObject>().ReturnToPool();

// Method 2: Direct pool access
NetworkObjectPool.Instance.ReturnNetworkObject(obj, prefab);
```

### 4. Pool Configuration
```csharp
// In Inspector:
PoolConfig config = new PoolConfig
{
    Prefab = yourNetworkPrefab,
    DefaultCapacity = 10,  // Initial pool size
    MaxSize = 20,         // Maximum pool size
    AutoExpand = true    // Allow exceeding MaxSize
};
```

## Testing

1. Setup Test Scene:
```
- Add NetworkManager
- Add NetworkObjectPool
- Add NetworkObjectPoolTests
- Configure UI elements
- Set test prefab
```

2. Run Tests:
- Build and run with multiple clients
- Use test buttons:
  - Spawn Test: Single object spawn
  - Return All: Return objects to pool
  - Stress Test: Rapid spawn/return

3. Test Coverage:
- Object spawning
- Pool reuse
- Network synchronization
- Memory management
- Stress handling

## Best Practices

1. Pool Configuration:
- Set appropriate initial capacity
- Configure reasonable max size
- Enable auto-expand for dynamic loads
- Group similar objects in same pool

2. Performance Optimization:
- Pre-warm pools at startup
- Return objects promptly
- Monitor pool usage
- Clean up unused pools

3. Network Considerations:
- Only spawn from server
- Handle client disconnects
- Validate object states
- Monitor network traffic

## Mobile Optimization

1. Memory Management:
- Use appropriate pool sizes
- Enable object reuse
- Clear unused pools
- Monitor memory usage

2. Performance:
- Minimize runtime allocation
- Batch object operations
- Use efficient transforms
- Optimize component usage

## Troubleshooting

Common Issues:
1. Objects not spawning
   - Check server authority
   - Verify pool configuration
   - Check NetworkObject component

2. Memory leaks
   - Ensure objects are returned
   - Check cleanup on scene change
   - Monitor pool sizes

3. Network sync issues
   - Verify NetworkObject setup
   - Check client authority
   - Monitor network messages

## Integration Steps

1. Setup:
```csharp
// 1. Add to scene
var poolGO = new GameObject("NetworkObjectPool");
var pool = poolGO.AddComponent<NetworkObjectPool>();

// 2. Configure pools
pool.poolConfigs = new PoolConfig[]
{
    new PoolConfig { Prefab = prefab1, DefaultCapacity = 10 },
    new PoolConfig { Prefab = prefab2, DefaultCapacity = 5 }
};
```

2. Usage in GameManager:
```csharp
public class GameManager : NetworkBehaviour
{
    private NetworkObjectPool _pool;

    void Start()
    {
        _pool = NetworkObjectPool.Instance;
    }

    void SpawnGameObject()
    {
        if (IsServer)
        {
            var obj = _pool.GetNetworkObject(prefab, position, rotation);
            // Configure spawned object
        }
    }
}
```

## Performance Monitoring

Monitor pool performance:
- Active object count
- Pool capacity usage
- Memory allocation
- Network message count

## Notes
- Always spawn from server
- Return objects when done
- Configure pools appropriately
- Test with multiple clients
- Monitor memory usage

_Last Updated: [Current Date]_
