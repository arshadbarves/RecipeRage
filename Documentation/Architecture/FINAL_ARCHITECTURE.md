# Final Architecture: Proper State-Based Matchmaking

Historical reference only. This file predates the March 2026 documentation cleanup. Use current code, `Documentation/Architecture/PROJECT_MEMORY.md`, and `Documentation/Architecture/CURRENT_CODEBASE_AUDIT.md` before using this document.

## Overview

We now have a **clean, state-based architecture** where matchmaking is handled by a dedicated game state.

## State Flow

```
MainMenuState → MatchmakingState → GameplayState → GameOverState
     ↓               ↓                   ↓              ↓
  Main Menu      Searching...         Playing        Results
```

## Complete Flow

### 1. User in MainMenu
```
GameStateManager: MainMenuState
└── MainMenuScreen
    └── LobbyTabComponent
        └── User clicks "Play" button
```

### 2. Transition to MatchmakingState
```
LobbyTabComponent.OnPlayClicked()
└── StateManager.ChangeState(new MatchmakingState(GameMode.Classic, teamSize: 4))
```

### 3. MatchmakingState.Enter()
```
MatchmakingState.Enter()
├── Hide MainMenu UI
├── Initialize timeout tracking
├── Subscribe to matchmaking events
└── MatchmakingService.FindMatch(gameMode, teamSize)
```

### 4. MatchmakingState.Update() - Every Frame
```
MatchmakingState.Update()
├── Calculate searchTime = Time.time - _searchStartTime
└── If searchTime >= 60 seconds:
    └── MatchmakingService.FillMatchWithBots()
```

### 5. Bot Filling
```
MatchmakingService.FillMatchWithBots()
├── BotManager.CreateBots(neededCount)
├── Update player counts
└── Fire OnMatchFound event
```

### 6. Match Found
```
MatchmakingState.HandleMatchFound()
└── StateManager.ChangeState(new GameplayState())
```

### 7. Gameplay
```
GameplayState.Enter()
├── Load "Game" scene
├── Hide all UI
├── Show HUD
└── Start game with bots
```

## Component Responsibilities

### MatchmakingState (Game State) ⭐
**Location:** `Assets/Scripts/Core/State/States/MatchmakingState.cs`

**Responsibilities:**
- ✅ Owns matchmaking logic
- ✅ Tracks timeout (60 seconds)
- ✅ Calls FillMatchWithBots() on timeout
- ✅ Handles match found event
- ✅ Transitions to GameplayState
- ✅ Handles matchmaking failure

**Lifecycle:**
```csharp
Enter()  → Start matchmaking, subscribe to events
Update() → Check timeout, trigger bot filling
Exit()   → Unsubscribe, cancel if needed
```

### LobbyTabComponent (UI Component)
**Location:** `Assets/Scripts/UI/Components/Tabs/LobbyTabComponent.cs`

**Responsibilities:**
- ✅ Shows Play button
- ✅ Transitions to MatchmakingState when clicked
- ✅ That's it! (Clean and simple)

**Code:**
```csharp
private void OnPlayClicked()
{
    _stateManager.ChangeState(new MatchmakingState(GameMode.Classic, teamSize: 4));
}
```

### MatchmakingWidgetComponent (UI Component)
**Location:** `Assets/Scripts/UI/Components/MatchmakingWidgetComponent.cs`

**Responsibilities:**
- ✅ Shows/hides matchmaking UI overlay
- ✅ Displays player count (3/8)
- ✅ Displays search time (0:45)
- ✅ Shows "Match Found!" message
- ✅ Handles cancel button
- ❌ Does NOT handle state transitions (MatchmakingState does)

### MatchmakingService (Stateless Service)
**Location:** `Assets/Scripts/Core/Networking/Services/MatchmakingService.cs`

**Responsibilities:**
- ✅ FindMatch() - Starts search
- ✅ FillMatchWithBots() - Creates bots
- ✅ CancelMatchmaking() - Cancels search
- ✅ Fires events (OnMatchFound, OnPlayersFound, etc.)
- ❌ Does NOT track timeout (MatchmakingState does)
- ❌ Does NOT manage state (MatchmakingState does)

### BotManager (Bot System)
**Location:** `Assets/Scripts/Core/Networking/Bot/BotManager.cs`

**Responsibilities:**
- ✅ Creates bots with unique names
- ✅ Tracks active bots
- ✅ Provides bot list

## Benefits of This Architecture

### ✅ Clean Separation of Concerns
- **States** = Logic and flow
- **Services** = Actions and data
- **UI Components** = Display and user input

### ✅ Easy to Test
```csharp
// Test MatchmakingState
var state = new MatchmakingState(GameMode.Classic, 4);
state.Enter();
// Simulate 60 seconds
state.Update(); // Should trigger bot filling
```

### ✅ Easy to Extend
Want ranked matchmaking?
```csharp
public class RankedMatchmakingState : MatchmakingState
{
    // Override timeout, add rank checking, etc.
}
```

### ✅ Follows State Machine Pattern
```
MainMenuState
    ↓ (User clicks Play)
MatchmakingState
    ↓ (Match found or timeout)
GameplayState
    ↓ (Game ends)
GameOverState
    ↓ (User clicks Continue)
MainMenuState
```

### ✅ Clear Ownership
- **MatchmakingState** owns matchmaking logic
- **LobbyTabComponent** just triggers the transition
- **MatchmakingWidgetComponent** just shows UI
- **MatchmakingService** just performs actions

## Configuration

### Timeout Duration
In `MatchmakingState.cs`:
```csharp
private const float SEARCH_TIMEOUT = 60f; // Change to desired seconds
```

### Game Mode and Team Size
When transitioning:
```csharp
// Classic 4v4
_stateManager.ChangeState(new MatchmakingState(GameMode.Classic, teamSize: 4));

// Time Attack 2v2
_stateManager.ChangeState(new MatchmakingState(GameMode.TimeAttack, teamSize: 2));

// Team Battle 6v6
_stateManager.ChangeState(new MatchmakingState(GameMode.TeamBattle, teamSize: 6));
```

## Console Output

```
[LobbyTabComponent] Play button clicked - Transitioning to MatchmakingState
[MatchmakingState] Entered
[MatchmakingState] Starting matchmaking: Classic, Team Size: 4
[MatchmakingService] Starting matchmaking: Mode=Classic, TeamSize=4, PartySize=1
[MatchmakingWidgetComponent] Shown
[MatchmakingState] Matchmaking started - searching for players...
... (60 seconds pass) ...
[MatchmakingState] Matchmaking timeout after 60.0s - filling with bots
[MatchmakingService] Creating 7 bots to fill match (1/8 players)
[BotManager] Created bot: Chef Bot Alpha (Team 0)
[BotManager] Created bot: Chef Bot Beta (Team 1)
... (more bots) ...
[MatchmakingService] Match filled with bots! Starting game with 1 human players and 7 bots
[MatchmakingState] Match found! Lobby: lobby_12345, Players: 8/8
[GameStateManager] State changed: MatchmakingState -> GameplayState
[MatchmakingState] Exited
[GameplayState] Entered
[GameplayState] Starting game with 7 bots
```

## Comparison: Before vs After

### Before (UI-Driven)
```
MainMenuState
└── LobbyTabComponent
    ├── Tracks timeout ❌ (UI shouldn't do this)
    ├── Calls FillMatchWithBots() ❌ (UI shouldn't do this)
    └── Logic mixed with UI ❌
```

### After (State-Driven) ✅
```
MainMenuState
└── LobbyTabComponent
    └── Triggers state transition ✅ (UI just triggers)

MatchmakingState
├── Tracks timeout ✅ (State owns logic)
├── Calls FillMatchWithBots() ✅ (State makes decisions)
└── Clean separation ✅
```

## Testing Checklist

### ✅ State Transitions
- [ ] MainMenu → MatchmakingState (click Play)
- [ ] MatchmakingState → GameplayState (match found)
- [ ] MatchmakingState → MainMenuState (failure)

### ✅ Timeout and Bot Filling
- [ ] Wait 60 seconds
- [ ] Bots are created
- [ ] Game starts automatically

### ✅ UI Behavior
- [ ] MainMenu hides when matchmaking starts
- [ ] Widget shows during search
- [ ] Widget shows "Match Found!"
- [ ] All UI hidden in gameplay

### ✅ Cancellation
- [ ] Click cancel button
- [ ] Returns to MainMenu
- [ ] Matchmaking stopped

## Summary

✅ **Clean state-based architecture**
✅ **MatchmakingState owns all matchmaking logic**
✅ **UI components just show UI**
✅ **Services are stateless**
✅ **Easy to test and extend**
✅ **Follows SOLID principles**
✅ **Proper separation of concerns**

This is the **correct way** to implement matchmaking in a state machine architecture! 🎉
