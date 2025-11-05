# Bot Spawning System - Setup Guide

## Overview

The bot spawning system automatically spawns AI bot players as NetworkObjects when a match starts with bots.

## How It Works

### Flow

```
Match Found with Bots
    ↓
GameplayState.Enter()
    ↓
Load Game scene
    ↓
Initialize NetworkManager
    ↓
GameStarter.StartGame()
    ↓
StartHost() called
    ↓
Host player spawns
    ↓
Spawn bots IMMEDIATELY (same time as players)
    ↓
Bots run simple AI
    ↓
Players can't tell who's a bot! ✅
```

## Components

### 1. BotSpawner
**File:** `Assets/Scripts/Core/Networking/Bot/BotSpawner.cs`

**Responsibilities:**
- Spawns bot NetworkObjects on the server
- Uses the same player prefab as human players
- Manages bot lifecycle (spawn/despawn)
- Positions bots in circular pattern

### 2. BotController
**File:** `Assets/Scripts/Core/Networking/Bot/BotController.cs`

**Responsibilities:**
- Attached to bot prefab (same as PlayerController)
- Runs simple AI on server only
- Moves bots to random positions
- Can be extended for cooking actions

**AI Behavior:**
- Every 2 seconds, bot performs an action
- Actions: Move to random position, interact, or idle
- Simple movement towards target position

### 3. GameStarter Integration
**File:** `Assets/Scripts/Core/Networking/Services/GameStarter.cs`

**Responsibilities:**
- Detects if bots exist in match
- Creates BotSpawner with player prefab
- Spawns bots immediately when host starts
- Bots spawn at the same time as players
- Only runs on server (host)

## Setup Instructions

### Step 1: Add BotController to Player Prefab

1. Open your player prefab (the one assigned to NetworkManager)
2. Add the `BotController` component
3. Configure settings:
   - **Action Interval**: 2 seconds (how often bot acts)
   - **Move Radius**: 10 units (how far bot can move)
   - **Enable AI**: True

### Step 2: Verify NetworkManager Configuration

1. Ensure NetworkManager has a Player Prefab assigned
2. The prefab should have:
   - `NetworkObject` component
   - `PlayerController` component
   - `BotController` component (newly added)

### Step 3: Test Bot Spawning

1. Start matchmaking
2. Wait for timeout (6 seconds)
3. Bots fill the match
4. Game starts
5. Check console logs:
   ```
   [BotSpawner] Spawning 5 bots
   [BotSpawner] Spawned bot: Chef Bot Alpha at (8, 0, 0)
   [BotController] Bot spawned on server: Bot_Chef Bot Alpha
   ```

## Bot Behavior

### Current AI (Simple)

Bots currently:
- ✅ Spawn as NetworkObjects
- ✅ Move to random positions
- ✅ Rotate towards movement direction
- ❌ Don't interact with stations (placeholder)
- ❌ Don't cook or complete orders

### Extending Bot AI

To add cooking behavior, modify `BotController.PerformAction()`:

```csharp
private void PerformAction()
{
    int action = Random.Range(0, 5);
    
    switch (action)
    {
        case 0:
            // Move to random position
            _targetPosition = GetRandomPosition();
            break;
            
        case 1:
            // Find nearest ingredient spawner
            FindAndMoveToIngredientSpawner();
            break;
            
        case 2:
            // Find nearest cooking station
            FindAndMoveToCookingStation();
            break;
            
        case 3:
            // Try to interact with nearby object
            TryInteract();
            break;
            
        case 4:
            // Idle
            _targetPosition = transform.position;
            break;
    }
}
```

## Configuration

### Bot Settings (in BotController)

```csharp
[Header("Bot Settings")]
[SerializeField] private float _actionInterval = 2f;    // Time between actions
[SerializeField] private float _moveRadius = 10f;       // How far bot can move
[SerializeField] private bool _enableAI = true;         // Toggle AI on/off
```

### Spawn Positions

Bots spawn in a circular pattern around the origin. Modify `GetBotSpawnPosition()` to change:

```csharp
private Vector3 GetBotSpawnPosition(int botIndex)
{
    // Circular pattern
    float angle = (botIndex * 45f) * Mathf.Deg2Rad;
    float radius = 8f;
    
    return new Vector3(
        Mathf.Cos(angle) * radius,
        0f,
        Mathf.Sin(angle) * radius
    );
}
```

## Troubleshooting

### Bots Don't Spawn

**Check:**
1. Are you the server/host? (Bots only spawn on server)
2. Is Player Prefab assigned in NetworkManager?
3. Does prefab have BotController component?
4. Check console for errors

### Bots Spawn But Don't Move

**Check:**
1. Is `Enable AI` checked in BotController?
2. Is the bot on the server? (AI only runs on server)
3. Check console for bot action logs

### Bots Spawn at Wrong Position

**Solution:**
- Modify `GetBotSpawnPosition()` in BotSpawner
- Adjust `radius` and `angle` calculation

## Console Output

Expected logs when bots spawn:

```
[GameStarter] Starting as host...
[GameStarter] Successfully started as host
[GameStarter] Spawning 5 bots immediately with players
[BotSpawner] Spawning 5 bots
[BotSpawner] Spawned bot: Chef Bot Alpha at (8, 0, 0)
[BotController] Initialized bot: Chef Bot Alpha
[BotController] Bot spawned on server: Bot_Chef Bot Alpha
[GameStarter] Spawned 5 bots - players won't know who's a bot!
[BotController] Chef Bot Alpha moving to (3.2, 0, 5.1)
```

## Next Steps

To make bots fully functional:

1. **Implement Interaction** - Make bots interact with stations
2. **Add Cooking Logic** - Make bots follow recipes
3. **Pathfinding** - Use NavMesh for better movement
4. **Difficulty Levels** - Easy/Medium/Hard bot behavior
5. **Team Coordination** - Bots work together on team modes

## Architecture

```
NetworkingServiceContainer
    ├── GameStarter
    │   └── SpawnBotsIfNeeded() - Spawns bots when host starts
    │
    ├── BotSpawner (created in GameStarter)
    │   ├── Spawns bot NetworkObjects
    │   └── Uses player prefab
    │
    └── MatchmakingService
        └── BotManager
            └── Tracks bot metadata (BotPlayer)

Bot Prefab (same as Player Prefab)
    ├── NetworkObject
    ├── PlayerController (handles movement/interaction)
    └── BotController (AI behavior)
```

## Summary

✅ **Bot spawning system implemented**
✅ **Bots spawn as NetworkObjects**
✅ **Bots spawn immediately with players (no delay)**
✅ **Players can't tell who's a bot**
✅ **Simple AI for movement**
✅ **Server-authoritative**
✅ **Uses same prefab as players**
⏳ **Cooking AI not yet implemented**

The foundation is ready - bots will spawn and move around. You can now extend the AI to make them cook and complete orders!
