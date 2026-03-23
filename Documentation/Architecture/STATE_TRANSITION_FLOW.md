# State Transition Flow with Bot Filling

Historical reference only. Use current code plus `PROJECT_MEMORY.md` and `CURRENT_CODEBASE_AUDIT.md` for the live state-flow model.

## Complete Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    MATCHMAKING STATE                             │
│  - Shows GameModeSelection UI                                    │
│  - Subscribes to MatchmakingService events                       │
│  - Calls MatchmakingService.Update() every frame                 │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│              MATCHMAKING SERVICE (Searching)                     │
│  - SearchTime increments every frame                             │
│  - Searching for players...                                      │
│  - Current: 3/8 players                                          │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼ (60 seconds elapsed)
┌─────────────────────────────────────────────────────────────────┐
│         MatchmakingService.Update() - TIMEOUT DETECTED           │
│  if (SearchTime >= SEARCH_TIMEOUT && !_hasFilledWithBots)       │
│      FillMatchWithBots()                                         │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│              FillMatchWithBots() - CREATE BOTS                   │
│  1. Calculate needed bots: 8 - 3 = 5 bots                       │
│  2. _botManager.CreateBots(5)                                    │
│  3. Update PlayersFound = 8                                      │
│  4. OnPlayersFound?.Invoke(8, 8)                                 │
│  5. HandleMatchReady(matchLobby) ◄── CRITICAL CALL               │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│           HandleMatchReady() - FIRE MATCH FOUND EVENT            │
│  1. Unsubscribe from lobby updates                               │
│  2. ChangeState(MatchmakingState.Starting)                       │
│  3. OnMatchFound?.Invoke(lobbyInfo) ◄── EVENT FIRED              │
│  4. ResetMatchmakingState()                                      │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼ (Event received)
┌─────────────────────────────────────────────────────────────────┐
│      MatchmakingState.HandleMatchFound() - EVENT HANDLER         │
│  1. Log: "Match found! Lobby: xxx, Players: 8/8"                │
│  2. CompleteMatchmaking(true)                                    │
│  3. services.StateManager.ChangeState(new GameplayState())       │
│     ◄── GAME STATE TRANSITION HAPPENS HERE                       │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│              MATCHMAKING STATE EXIT                              │
│  - Unsubscribe from events                                       │
│  - Hide GameModeSelection screen                                 │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│              GAMEPLAY STATE ENTER                                │
│  1. Load "Game" scene if needed                                  │
│  2. uiService.HideAllScreens(true) ◄── HIDE ALL UI               │
│  3. uiService.ShowScreen(UIScreenType.Game) ◄── SHOW HUD         │
│  4. Initialize game mode                                         │
│  5. Start order system                                           │
│  6. Reset scores                                                 │
│  7. Log bot information                                          │
│  8. GAME STARTS! 🎮                                              │
└─────────────────────────────────────────────────────────────────┘
```

## Key Points

### ✅ State Transition DOES Happen

The game state **DOES** transition from `MatchmakingState` to `GameplayState` when bots fill the match because:

1. **Event Chain**: `FillMatchWithBots()` → `HandleMatchReady()` → `OnMatchFound` event
2. **Event Handler**: `MatchmakingState.HandleMatchFound()` listens to `OnMatchFound`
3. **State Change**: Handler calls `StateManager.ChangeState(new GameplayState())`

### ✅ UI Transition Happens

All UI screens are hidden and only HUD is shown because:

1. **GameplayState.Enter()** calls `uiService.HideAllScreens(true)`
2. Then calls `uiService.ShowScreen(UIScreenType.Game, true, false)`
3. This ensures clean transition to gameplay

## Code References

### 1. Timeout Detection
**File**: `MatchmakingService.cs` (Line ~560)
```csharp
public void Update()
{
    if (!IsSearching) return;
    SearchTime = Time.time - _searchStartTime;
    
    if (SearchTime >= SEARCH_TIMEOUT && !_hasFilledWithBots)
    {
        FillMatchWithBots(); // ◄── Triggers bot filling
    }
}
```

### 2. Bot Filling and Event Fire
**File**: `MatchmakingService.cs` (Line ~575)
```csharp
private void FillMatchWithBots()
{
    // ... create bots ...
    HandleMatchReady(matchLobby); // ◄── Calls this
}

private void HandleMatchReady(LobbyInfo lobbyInfo)
{
    // ... cleanup ...
    OnMatchFound?.Invoke(lobbyInfo); // ◄── Fires event
    ResetMatchmakingState();
}
```

### 3. Event Handler and State Transition
**File**: `MatchmakingState.cs` (Line ~115)
```csharp
private void HandleMatchFound(LobbyInfo lobbyInfo)
{
    CompleteMatchmaking(true);
    
    var services = GameBootstrap.Services;
    if (services != null)
    {
        services.StateManager.ChangeState(new GameplayState()); // ◄── State change
    }
}
```

### 4. UI Cleanup in Gameplay
**File**: `GameplayState.cs` (Line ~20)
```csharp
public override void Enter()
{
    base.Enter();
    
    var uiService = GameBootstrap.Services?.UIService;
    if (uiService != null)
    {
        uiService.HideAllScreens(true);  // ◄── Hide all
        uiService.ShowScreen(UIScreenType.Game, true, false); // ◄── Show HUD
    }
    // ... rest of initialization ...
}
```

## Testing the Flow

### Console Output You Should See

```
[MatchmakingService] Starting matchmaking: Mode=Classic, TeamSize=4, PartySize=1
[MatchmakingService] State changed: Idle -> Searching
... (60 seconds pass) ...
[MatchmakingService] Search timeout reached - filling with bots
[MatchmakingService] Creating 7 bots to fill match (1/8 players)
[BotManager] Created bot: Chef Bot Alpha (Team 0)
[BotManager] Created bot: Chef Bot Beta (Team 1)
... (5 more bots) ...
[MatchmakingService] Match filled with bots! Starting game with 1 human players and 7 bots
[MatchmakingService] Match ready! Lobby full: lobby_12345
[MatchmakingService] State changed: WaitingForPlayers -> Starting
[MatchmakingState] Match found! Lobby: lobby_12345, Players: 8/8
[MatchmakingState] Matchmaking complete. Success: True
[GameStateManager] State changed: MatchmakingState -> GameplayState  ◄── STATE CHANGE
[GameplayState] Entered
[GameplayState] Starting game with 7 bots
[GameplayState] Bot: Chef Bot Alpha (Team 0)
... (bot list) ...
```

## Verification Checklist

- ✅ Timeout detected after 60 seconds
- ✅ Bots created by BotManager
- ✅ `OnMatchFound` event fired
- ✅ `HandleMatchFound` receives event
- ✅ `StateManager.ChangeState()` called
- ✅ GameplayState entered
- ✅ All UI screens hidden
- ✅ Game HUD shown
- ✅ Game starts with bots

## If State Transition Doesn't Happen

Check these potential issues:

1. **Event not subscribed**: Verify `OnMatchFound` subscription in `MatchmakingState.Enter()`
2. **Services null**: Check `GameBootstrap.Services` is initialized
3. **StateManager null**: Verify `services.StateManager` exists
4. **Event not firing**: Add breakpoint in `HandleMatchReady()` to verify `OnMatchFound?.Invoke()` is called
5. **Handler not called**: Add breakpoint in `MatchmakingState.HandleMatchFound()` to verify it receives the event

## Summary

**The system IS correctly implemented.** The game state WILL transition from MatchmakingState to GameplayState when bots fill the match after timeout. The complete event chain is in place and working as designed.
