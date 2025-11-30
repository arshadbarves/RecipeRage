# Actual Matchmaking Flow Documentation

## Important Discovery

**MatchmakingState (game state) is NOT used in this project!**

The matchmaking happens directly from the MainMenu UI, not through a separate game state.

## Actual Flow

### 1. User in MainMenu
```
GameStateManager: MainMenuState
├── MainMenuScreen (UI)
    └── LobbyTabComponent
        ├── Play Button
        └── MatchmakingWidgetComponent
```

### 2. User Clicks "Play"
```
LobbyTabComponent.OnPlayClicked()
├── Reset timeout tracking
├── _searchStartTime = Time.time
├── _hasFilledWithBots = false
└── _matchmakingService.FindMatch(GameMode.Classic, teamSize: 4)
```

### 3. Matchmaking Starts
```
MatchmakingService.FindMatch()
├── IsSearching = true
├── Fire OnMatchmakingStarted event
└── SearchForMatchLobbies()

MatchmakingWidgetComponent (listens to event)
└── Show widget with "Searching for players..."
```

### 4. Timeout Detection (60 seconds)
```
LobbyTabComponent.Update() (called every frame)
├── Check if IsSearching && !_hasFilledWithBots
├── Calculate searchTime = Time.time - _searchStartTime
└── If searchTime >= 60 seconds:
    └── _matchmakingService.FillMatchWithBots()
```

### 5. Bot Filling
```
MatchmakingService.FillMatchWithBots()
├── Calculate needed bots
├── BotManager.CreateBots(count)
├── Update player counts
├── Fire OnMatchFound event
└── HandleMatchReady(lobbyInfo)
```

### 6. Match Found
```
MatchmakingWidgetComponent.OnMatchFound()
├── Update UI: "Match Found!"
└── TransitionToGameplay()
    └── GameStateManager.ChangeState(new GameplayState())
```

### 7. Gameplay State
```
GameplayState.Enter()
├── Load "Game" scene
├── UIService.HideAllScreens()
├── UIService.ShowScreen(UIScreenType.Game) // HUD only
├── Initialize game systems
└── Log bot information
```

## Key Components

### LobbyTabComponent
**Responsibilities:**
- Handles Play button click
- Tracks matchmaking timeout
- Calls FillMatchWithBots() on timeout
- Updates matchmaking widget

**Location:** `Assets/Scripts/UI/Components/Tabs/LobbyTabComponent.cs`

### MatchmakingWidgetComponent
**Responsibilities:**
- Shows/hides matchmaking UI
- Displays player count and search time
- Handles cancel button
- Transitions to GameplayState when match found

**Location:** `Assets/Scripts/UI/Components/MatchmakingWidgetComponent.cs`

### MatchmakingService
**Responsibilities:**
- Performs matchmaking actions (FindMatch, FillMatchWithBots, Cancel)
- Fires events (OnMatchmakingStarted, OnMatchFound, etc.)
- Manages bot creation via BotManager
- **Stateless** - no internal state tracking

**Location:** `Assets/Scripts/Core/Networking/Services/MatchmakingService.cs`

### BotManager
**Responsibilities:**
- Creates bots with unique names
- Tracks active bots
- Provides bot list

**Location:** `Assets/Scripts/Core/Networking/Bot/BotManager.cs`

## State Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    MainMenuState                             │
│  ┌────────────────────────────────────────────────────┐     │
│  │         MainMenuScreen (UI)                        │     │
│  │  ┌──────────────────────────────────────────┐     │     │
│  │  │     LobbyTabComponent                    │     │     │
│  │  │  - Tracks timeout                        │     │     │
│  │  │  - Calls FillMatchWithBots()             │     │     │
│  │  │  ┌────────────────────────────────┐     │     │     │
│  │  │  │  MatchmakingWidgetComponent    │     │     │     │
│  │  │  │  - Shows UI                    │     │     │     │
│  │  │  │  - Transitions to GameplayState│     │     │     │
│  │  │  └────────────────────────────────┘     │     │     │
│  │  └──────────────────────────────────────────┘     │     │
│  └────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────┘
                          │
                          │ OnMatchFound event
                          │ MatchmakingWidgetComponent.TransitionToGameplay()
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                    GameplayState                             │
│  - Hide all UI                                               │
│  - Show HUD                                                  │
│  - Start game with bots                                      │
└─────────────────────────────────────────────────────────────┘
```

## Why No MatchmakingState?

The game uses a **UI-driven approach** instead of a **state-driven approach**:

### UI-Driven (Current)
- Matchmaking happens in MainMenuState
- UI components handle the flow
- Simpler for this use case

### State-Driven (Alternative)
- Would have separate MatchmakingState
- State owns the logic
- More complex, but cleaner separation

**Current approach is fine** because:
- Matchmaking is a UI feature, not a game state
- User stays in MainMenu during search
- Simpler implementation
- Less state transitions

## MatchmakingState.cs File

The file `Assets/Scripts/Core/State/States/MatchmakingState.cs` exists but is **NOT USED**.

**Options:**
1. **Delete it** - Since it's not used (recommended)
2. **Keep it** - For future use if you want state-driven matchmaking
3. **Refactor to use it** - Move logic from LobbyTabComponent to MatchmakingState

**Recommendation:** Keep it for now, but document that it's not currently used. It might be useful if you want to add a dedicated matchmaking screen later.

## Complete Timeline

```
0s   - User clicks Play button
     - LobbyTabComponent.OnPlayClicked()
     - MatchmakingService.FindMatch()
     - Widget shows "Searching..."

1-59s - Searching for players
      - Widget updates player count
      - Widget updates search time
      - LobbyTabComponent.Update() checks timeout

60s  - Timeout detected
     - LobbyTabComponent calls FillMatchWithBots()
     - BotManager creates bots
     - MatchmakingService fires OnMatchFound

60s+ - Widget shows "Match Found!"
     - MatchmakingWidgetComponent.TransitionToGameplay()
     - GameStateManager.ChangeState(GameplayState)
     - All UI hidden, HUD shown
     - Game starts with bots
```

## Configuration

### Timeout Duration
In `LobbyTabComponent.cs`:
```csharp
private const float SEARCH_TIMEOUT = 60f; // Change to desired seconds
```

### Bot Names
In `BotManager.cs`:
```csharp
private static readonly string[] BotNames = new[]
{
    "Chef Bot Alpha", "Chef Bot Beta", // etc.
};
```

## Testing

### Test Matchmaking Flow
1. Start game
2. Click "Play" button in MainMenu
3. Wait 60 seconds
4. Verify bots are created
5. Verify game transitions to GameplayState
6. Verify UI is hidden and HUD is shown

### Console Output
```
[LobbyTabComponent] Play button clicked - Starting matchmaking
[MatchmakingService] Starting matchmaking: Mode=Classic, TeamSize=4, PartySize=1
[MatchmakingWidgetComponent] Shown
... (60 seconds) ...
[LobbyTabComponent] Matchmaking timeout after 60.0s - filling with bots
[MatchmakingService] Creating 7 bots to fill match (1/8 players)
[BotManager] Created bot: Chef Bot Alpha (Team 0)
... (more bots) ...
[MatchmakingService] Match filled with bots! Starting game with 1 human players and 7 bots
[MatchmakingWidgetComponent] Match found! Lobby: lobby_12345, Players: 8/8
[MatchmakingWidgetComponent] Transitioning to gameplay...
[GameStateManager] State changed: MainMenuState -> GameplayState
[GameplayState] Entered
[GameplayState] Starting game with 7 bots
```

## Summary

✅ **Matchmaking is UI-driven, not state-driven**
✅ **LobbyTabComponent handles timeout detection**
✅ **MatchmakingWidgetComponent handles state transition**
✅ **MatchmakingService is stateless**
✅ **Bot filling works on 60-second timeout**
✅ **Game transitions to GameplayState automatically**

The system is complete and working as designed!
