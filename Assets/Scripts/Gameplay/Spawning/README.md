# Spawn System

A flexible spawn management system that uses scene-based spawn points with team categorization for players and bots.

## Components

### SpawnPoint
Place `SpawnPoint` components in your scene to define where players and bots can spawn.

**Features:**
- Team categorization (Neutral, Team A, Team B)
- Availability tracking
- Spawn radius for randomization
- Visual gizmos for easy scene setup

**Inspector Settings:**
- `Team Category`: Which team can use this spawn point
- `Is Available`: Whether the spawn point is currently available
- `Spawn Radius`: Radius for random position offset
- `Show Gizmos`: Display visual indicators in scene view

**Gizmo Colors:**
- Blue: Team A spawn points
- Red: Team B spawn points
- Green: Neutral spawn points
- Gray: Unavailable spawn points

### SpawnManager
Manages all spawning logic for players and bots.

**Features:**
- Automatic spawn point discovery and categorization
- Team-based spawn point selection
- Spawn point reuse or one-time use
- Spawn cooldown
- Player and bot spawning

**Inspector Settings:**
- `Randomize Spawn Position`: Add random offset within spawn radius
- `Reuse Spawn Points`: Allow multiple spawns at same point
- `Spawn Cooldown`: Minimum time between spawns
- `Player Prefab`: Prefab to spawn for players
- `Bot Prefab`: Prefab to spawn for bots

## Setup

### 1. Scene Setup

1. Add a `SpawnManager` component to a GameObject in your scene
2. Assign player and bot prefabs in the inspector
3. Create empty GameObjects for spawn points
4. Add `SpawnPoint` component to each spawn point GameObject
5. Configure team category for each spawn point
6. Position and rotate spawn points as desired

```
Scene Hierarchy:
├── SpawnManager
├── SpawnPoints
│   ├── SpawnPoint_Neutral_1 (TeamCategory: Neutral)
│   ├── SpawnPoint_Neutral_2 (TeamCategory: Neutral)
│   ├── SpawnPoint_TeamA_1 (TeamCategory: TeamA)
│   ├── SpawnPoint_TeamA_2 (TeamCategory: TeamA)
│   ├── SpawnPoint_TeamB_1 (TeamCategory: TeamB)
│   └── SpawnPoint_TeamB_2 (TeamCategory: TeamB)
```

### 2. Code Integration

#### Spawning Players

```csharp
using Gameplay.Spawning;

// Get spawn manager
var spawnManager = SpawnManagerIntegration.GetSpawnManager();

// Spawn player on neutral spawn point
spawnManager.SpawnPlayer(clientId, TeamCategory.Neutral);

// Spawn player on team-specific spawn point
spawnManager.SpawnPlayer(clientId, TeamCategory.TeamA);
```

#### Spawning Bots

```csharp
using Gameplay.Networking.Bot;
using Gameplay.Spawning;

// Create bot player
var botPlayer = new BotPlayer("BotName", BotDifficulty.Medium);

// Get spawn manager
var spawnManager = SpawnManagerIntegration.GetSpawnManager();

// Spawn single bot
spawnManager.SpawnBot(botPlayer, TeamCategory.Neutral);

// Spawn multiple bots
List<BotPlayer> bots = new List<BotPlayer>();
spawnManager.SpawnBots(bots, TeamCategory.TeamA);
```

#### Managing Spawn Points

```csharp
// Check if spawn points are available
bool hasSpawns = spawnManager.HasAvailableSpawnPoints(TeamCategory.TeamA);

// Get spawn point counts
int totalSpawns = spawnManager.GetSpawnPointCount(TeamCategory.Neutral);
int availableSpawns = spawnManager.GetAvailableSpawnPointCount(TeamCategory.Neutral);

// Release spawn point when player leaves
spawnManager.ReleaseSpawnPoint(clientId);

// Reset all spawn points
spawnManager.ResetAllSpawnPoints();
```

### 3. Integration with GameplayState

Update your gameplay state to use the spawn manager:

```csharp
public class GameplayState : IState
{
    private SpawnManager _spawnManager;

    public void Enter()
    {
        _spawnManager = SpawnManagerIntegration.GetSpawnManager();
        
        // Spawn players
        foreach (var clientId in connectedClients)
        {
            _spawnManager.SpawnPlayer(clientId, GetPlayerTeam(clientId));
        }
        
        // Spawn bots
        _spawnManager.SpawnBots(botPlayers, TeamCategory.Neutral);
    }

    public void Exit()
    {
        _spawnManager?.ResetAllSpawnPoints();
        SpawnManagerIntegration.Clear();
    }
}
```

## Team Categories

### TeamCategory.Neutral
- Used for free-for-all game modes
- Fallback for team modes when team-specific points unavailable
- Accessible by all players

### TeamCategory.TeamA
- Used for team-based game modes
- Blue gizmo color in editor
- Players assigned to Team A spawn here

### TeamCategory.TeamB
- Used for team-based game modes
- Red gizmo color in editor
- Players assigned to Team B spawn here

## Spawn Point Selection Logic

1. Try to find available spawn point for requested team
2. If no team-specific points available, fallback to neutral points
3. If multiple points available, select randomly
4. If reuse enabled, all points are always available
5. If reuse disabled, points become unavailable after use

## Best Practices

### Spawn Point Placement
- Place spawn points away from hazards
- Ensure clear space around spawn points
- Face spawn points toward gameplay area
- Distribute evenly across map
- Provide enough spawn points for max players + bots

### Team Balance
- Equal number of spawn points per team
- Similar strategic positions for each team
- Consider map symmetry for fairness

### Configuration
- Enable reuse for respawn mechanics
- Disable reuse for initial spawn only
- Use randomization for variety
- Adjust spawn radius based on map size
- Set appropriate cooldown to prevent spam

### Performance
- Spawn points are cached at initialization
- Minimal runtime overhead
- Gizmos only visible in editor

## Troubleshooting

### No spawn points found
- Ensure SpawnPoint components exist in scene
- Check that SpawnManager is in the same scene
- Verify spawn points are active in hierarchy

### Players spawning at origin
- Check that spawn points are properly positioned
- Verify SpawnManager has found spawn points (check logs)
- Ensure spawn points are available

### Team spawns not working
- Verify team category is set correctly on spawn points
- Check that you're passing correct TeamCategory when spawning
- Ensure team-specific spawn points exist in scene

### Spawn cooldown preventing spawns
- Adjust spawn cooldown in SpawnManager inspector
- Check timing of spawn calls
- Consider disabling cooldown for initial spawns
