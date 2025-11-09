# Spawn System Quick Start

Get up and running with the spawn system in 5 minutes.

## Step 1: Add Spawn Manager to Scene

1. In Unity, right-click in Hierarchy
2. Select `GameObject > RecipeRage > Spawning > Spawn Manager`
3. Select the SpawnManager GameObject
4. In Inspector, assign:
   - `Player Prefab`: Your player prefab with NetworkObject
   - `Bot Prefab`: Your bot prefab with NetworkObject

## Step 2: Create Spawn Points

### For Free-For-All Mode (Neutral Spawns)

1. Right-click in Hierarchy
2. Select `GameObject > RecipeRage > Spawning > Spawn Point - Neutral`
3. Position the spawn point in your scene
4. Rotate to face desired direction (forward arrow shows spawn direction)
5. Repeat to create 4-8 spawn points around your map

### For Team-Based Mode

1. Create Team A spawn points:
   - Right-click > `GameObject > RecipeRage > Spawning > Spawn Point - Team A`
   - Position on one side of the map (shows blue in scene view)
   - Create 2-4 spawn points

2. Create Team B spawn points:
   - Right-click > `GameObject > RecipeRage > Spawning > Spawn Point - Team B`
   - Position on opposite side of the map (shows red in scene view)
   - Create 2-4 spawn points

## Step 3: Use in Code

### Spawn Players

```csharp
using Gameplay.Spawning;

public class MyGameState : IState
{
    private SpawnManager _spawnManager;

    public void Enter()
    {
        // Get spawn manager
        _spawnManager = SpawnManagerIntegration.GetSpawnManager();
        
        // Spawn player (server only)
        if (NetworkManager.Singleton.IsServer)
        {
            _spawnManager.SpawnPlayer(clientId, TeamCategory.Neutral);
        }
    }
}
```

### Spawn Bots

```csharp
using Core.Networking.Bot;
using Gameplay.Spawning;

// Create bots
var bots = new List<BotPlayer>
{
    new BotPlayer("Bot1", BotDifficulty.Easy),
    new BotPlayer("Bot2", BotDifficulty.Medium)
};

// Spawn them
var spawnManager = SpawnManagerIntegration.GetSpawnManager();
spawnManager.SpawnBots(bots, TeamCategory.Neutral);
```

## Step 4: Test in Play Mode

1. Press Play in Unity
2. Check Console for spawn logs:
   - `[SpawnManager] Initialized X spawn points`
   - `[SpawnManager] Spawned player X at position`
3. Verify players/bots spawn at your spawn points
4. Check that spawn point gizmos are visible in Scene view

## Common Configurations

### Respawn System (Reuse Spawn Points)
- Enable `Reuse Spawn Points` in SpawnManager
- Players can respawn at same locations
- Good for battle royale, deathmatch modes

### One-Time Spawns (No Reuse)
- Disable `Reuse Spawn Points` in SpawnManager
- Each spawn point used once
- Good for initial game start

### Random Spawn Positions
- Enable `Randomize Spawn Position` in SpawnManager
- Adjust `Spawn Radius` on each SpawnPoint
- Adds variety to spawn locations

### Spawn Cooldown
- Set `Spawn Cooldown` in SpawnManager (default: 2 seconds)
- Prevents spawn spam
- Set to 0 for instant spawning

## Troubleshooting

**Players spawn at (0,0,0)?**
- Check that SpawnManager found spawn points (see Console)
- Verify spawn points are active in Hierarchy
- Ensure spawn points have SpawnPoint component

**No spawn points found?**
- SpawnManager must be in the same scene as spawn points
- Spawn points must be active when scene loads
- Check Console for initialization logs

**Team spawns not working?**
- Verify TeamCategory is set correctly on spawn points
- Check gizmo colors: Blue = Team A, Red = Team B, Green = Neutral
- Ensure you're passing correct TeamCategory when spawning

## Next Steps

- Read full documentation: `Assets/Scripts/Gameplay/Spawning/README.md`
- Customize spawn point settings in Inspector
- Integrate with your game state management
- Add spawn effects and animations
