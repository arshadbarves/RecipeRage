# Bot Filling System

## Overview

The bot filling system automatically fills remaining player slots with AI bots when matchmaking times out, ensuring games start even with fewer human players.

## How It Works

### 1. Matchmaking Timeout Detection

- **Timeout Duration**: 60 seconds (configurable in `MatchmakingService.SEARCH_TIMEOUT`)
- **Monitoring**: The `MatchmakingState.Update()` calls `MatchmakingService.Update()` every frame
- **Trigger**: When `SearchTime >= SEARCH_TIMEOUT`, bot filling is triggered automatically

### 2. Bot Creation

When timeout occurs:
1. Calculate needed bots: `RequiredPlayers - CurrentPlayers`
2. Create bots using `BotManager.CreateBots(count)`
3. Bots are assigned to teams alternately (Team A, Team B, Team A, etc.)
4. Each bot gets a unique ID and name from a predefined pool

### 3. Game Start

After bots are created:
1. Update player count to show match is full
2. Trigger `OnMatchFound` event
3. Transition to `GameplayState`
4. Hide all UI screens
5. Show only the game HUD
6. Log bot information for debugging

## Key Components

### BotPlayer (`Assets/Scripts/Core/Networking/Bot/BotPlayer.cs`)

Represents a single bot player with:
- Unique bot ID
- Display name (e.g., "Chef Bot Alpha", "Robo Chef")
- Team assignment
- Always ready status
- Converts to `PlayerInfo` for lobby display

### BotManager (`Assets/Scripts/Core/Networking/Bot/BotManager.cs`)

Manages all bots:
- Creates bots with unique names
- Tracks active bots
- Clears bots when matchmaking restarts
- Provides bot name pool (16 unique names)

### MatchmakingService Updates

**New Fields:**
- `_botManager`: Instance of BotManager
- `_hasFilledWithBots`: Prevents multiple bot fills

**New Methods:**
- `FillMatchWithBots()`: Creates bots and starts the match
- `GetActiveBots()`: Returns list of active bots

**Modified Methods:**
- `Update()`: Checks for timeout and triggers bot filling
- `FindMatch()`: Clears previous bots and resets flag
- `ResetMatchmakingState()`: Resets bot filling flag

### PlayerInfo Updates

Added `IsBot` property to distinguish bots from human players:
```csharp
public bool IsBot { get; set; }
```

### GameplayState Updates

Enhanced to:
- Hide all UI screens on entry
- Show only the game HUD
- Log bot information for debugging
- Access bots via `NetworkingServices.MatchmakingService.GetActiveBots()`

## Bot Names

The system includes 16 unique bot names:
- Chef Bot Alpha, Beta, Gamma, Delta, Epsilon, Zeta, Eta, Theta
- Robo Chef, Cyber Cook, AI Chef, Bot Gordon
- Digital Chef, Virtual Cook, Auto Chef, Mecha Cook

If more than 16 bots are needed, names are suffixed with numbers (e.g., "Chef Bot Alpha 2").

## Configuration

### Timeout Duration

Change in `MatchmakingService.cs`:
```csharp
private const float SEARCH_TIMEOUT = 60f; // Change to desired seconds
```

### Bot Names

Modify the `BotNames` array in `BotManager.cs`:
```csharp
private static readonly string[] BotNames = new[]
{
    "Your Custom Name 1",
    "Your Custom Name 2",
    // Add more names...
};
```

## Usage Example

```csharp
// Start matchmaking (4v4 match)
matchmakingService.FindMatch(GameMode.Classic, teamSize: 4);

// After 60 seconds, if only 3 players joined:
// - System creates 5 bots automatically
// - Match starts with 3 humans + 5 bots
// - Game transitions to GameplayState
// - All UI screens hidden except HUD

// Access bots in gameplay
var bots = matchmakingService.GetActiveBots();
foreach (var bot in bots)
{
    Debug.Log($"Bot: {bot.BotName} on Team {bot.TeamId}");
}
```

## Flow Diagram

```
Matchmaking Started
        ↓
    Searching...
        ↓
    60 seconds elapsed?
        ↓
    Yes → Fill with Bots
        ↓
    Create needed bots
        ↓
    Update player count
        ↓
    Trigger OnMatchFound
        ↓
    Transition to GameplayState
        ↓
    Hide all UI screens
        ↓
    Show HUD only
        ↓
    Game starts!
```

## Testing

To test the bot filling system:

1. Start matchmaking with a small party (1-2 players)
2. Wait 60 seconds without other players joining
3. Observe console logs:
   - "Search timeout reached - filling with bots"
   - "Creating X bots to fill match"
   - "Match filled with bots! Starting game..."
4. Game should transition to gameplay with bots

## Future Enhancements

Potential improvements:
- Configurable bot difficulty levels
- Bot AI behavior implementation
- Bot skin/character customization
- Dynamic timeout based on player count
- Progressive bot filling (add bots gradually)
- Bot removal if human players join late
