# Quick Start: Bot Filling System

## What Was Implemented

A complete bot filling system that automatically fills empty player slots with AI bots when matchmaking times out after 60 seconds.

## Key Features

✅ **Automatic Bot Filling**: After 60 seconds of matchmaking, remaining slots are filled with bots
✅ **No Match Failure**: Games always start, even with just 1 human player
✅ **Team Balance**: Bots are distributed evenly across teams
✅ **Clean UI Transition**: All screens hidden, only HUD shown when game starts
✅ **Bot Tracking**: Access active bots via `GetActiveBots()` method

## Files Created

1. **Assets/Scripts/Core/Networking/Bot/BotPlayer.cs**
   - Represents a single bot player
   - Converts to PlayerInfo for lobby display

2. **Assets/Scripts/Core/Networking/Bot/BotManager.cs**
   - Manages bot creation and lifecycle
   - Provides 16 unique bot names

## Files Modified

1. **Assets/Scripts/Core/Networking/Common/NetworkTypes.cs**
   - Added `IsBot` property to `PlayerInfo`

2. **Assets/Scripts/Core/Networking/Services/MatchmakingService.cs**
   - Added bot filling logic on timeout
   - Added `FillMatchWithBots()` method
   - Added `GetActiveBots()` method

3. **Assets/Scripts/Core/Networking/Interfaces/IMatchmakingService.cs**
   - Added `Update()` method signature
   - Added `GetActiveBots()` method signature

4. **Assets/Scripts/Core/State/States/MatchmakingState.cs**
   - Calls `MatchmakingService.Update()` every frame

5. **Assets/Scripts/Core/State/States/GameplayState.cs**
   - Hides all UI screens on entry
   - Shows only game HUD
   - Logs bot information

## How to Test

### Test 1: Solo Player with Bots
```csharp
// Start matchmaking alone
matchmakingService.FindMatch(GameMode.Classic, teamSize: 4);

// Wait 60 seconds
// Expected: 7 bots created (8 total players for 4v4)
// Game starts automatically
```

### Test 2: Partial Party with Bots
```csharp
// Start matchmaking with 3 players in party
matchmakingService.FindMatch(GameMode.Classic, teamSize: 4);

// Wait 60 seconds
// Expected: 5 bots created (8 total players)
// Game starts automatically
```

### Test 3: Check Bot Information
```csharp
// In GameplayState, after bots are created
var bots = GameBootstrap.Services.NetworkingServices
    .MatchmakingService.GetActiveBots();

foreach (var bot in bots)
{
    Debug.Log($"Bot: {bot.BotName}, Team: {bot.TeamId}, Ready: {bot.IsReady}");
}
```

## Console Output Example

When timeout occurs, you'll see:
```
[MatchmakingService] Search timeout reached - filling with bots
[MatchmakingService] Creating 5 bots to fill match (3/8 players)
[BotManager] Created bot: Chef Bot Alpha (Team 0)
[BotManager] Created bot: Chef Bot Beta (Team 1)
[BotManager] Created bot: Chef Bot Gamma (Team 0)
[BotManager] Created bot: Chef Bot Delta (Team 1)
[BotManager] Created bot: Chef Bot Epsilon (Team 0)
[MatchmakingService] Match filled with bots! Starting game with 3 human players and 5 bots
[MatchmakingService] Match ready! Lobby full: lobby_12345
[GameplayState] Starting game with 5 bots
[GameplayState] Bot: Chef Bot Alpha (Team 0)
[GameplayState] Bot: Chef Bot Beta (Team 1)
...
```

## Configuration

### Change Timeout Duration
In `MatchmakingService.cs`:
```csharp
private const float SEARCH_TIMEOUT = 60f; // Change to desired seconds
```

### Add Custom Bot Names
In `BotManager.cs`:
```csharp
private static readonly string[] BotNames = new[]
{
    "Your Bot Name 1",
    "Your Bot Name 2",
    // Add more...
};
```

## Integration Points

### Access Bots in Gameplay
```csharp
var matchmakingService = GameBootstrap.Services.NetworkingServices.MatchmakingService;
var bots = matchmakingService.GetActiveBots();

// Use bots for:
// - Spawning bot characters
// - AI behavior assignment
// - Team balancing
// - Score tracking
```

### Distinguish Bots from Humans
```csharp
foreach (var player in lobby.Players)
{
    if (player.IsBot)
    {
        // Handle bot player
        SpawnBotCharacter(player);
    }
    else
    {
        // Handle human player
        SpawnHumanCharacter(player);
    }
}
```

## Next Steps

To complete the bot system:

1. **Bot AI Implementation**: Create bot behavior scripts
2. **Bot Character Spawning**: Spawn bot characters in game scene
3. **Bot Actions**: Implement cooking/gameplay actions for bots
4. **Bot Difficulty**: Add difficulty levels (Easy, Medium, Hard)
5. **Bot Customization**: Allow bot skin/character selection

## Troubleshooting

**Bots not being created?**
- Check console for timeout message
- Verify `MatchmakingState.Update()` is being called
- Ensure matchmaking service is initialized

**Game not starting after bots fill?**
- Check `OnMatchFound` event is subscribed
- Verify `GameplayState` transition is working
- Check lobby state in debugger

**UI not hiding properly?**
- Verify `UIService.HideAllScreens()` is called
- Check `UIScreenType.Game` is registered
- Ensure HUD screen exists and is configured

## Support

For issues or questions:
1. Check console logs for error messages
2. Review `BOT_FILLING_SYSTEM.md` for detailed documentation
3. Debug with breakpoints in `FillMatchWithBots()` method
