# Refactor Plan: Consolidate State Management

## Problem Statement

Currently we have **two separate state systems**:
1. **GameStateManager** - Main game flow (MainMenuState, MatchmakingState, GameplayState, etc.)
2. **MatchmakingService.MatchmakingState** - Internal matchmaking phases (Idle, Searching, WaitingForPlayers, etc.)

This creates confusion and redundancy. We should have **ONE state manager** as the single source of truth.

## Goal

Make `GameStateManager` the **only** state system. The `MatchmakingService` becomes a **stateless service** that performs actions and fires events, while `MatchmakingState` (the game state) handles all state logic.

## Architecture Change

### Before (Current)
```
┌─────────────────────────────────────────────────────────────┐
│                    GameStateManager                          │
│  ┌────────────────────────────────────────────────────┐     │
│  │         MatchmakingState (Game State)              │     │
│  │  - Subscribes to MatchmakingService events         │     │
│  │  - Transitions to GameplayState                    │     │
│  │  - Calls service.Update()                          │     │
│  └────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────┘
                          │ uses
                          ▼
┌─────────────────────────────────────────────────────────────┐
│              MatchmakingService (Has State!)                 │
│  - CurrentState: MatchmakingState enum                       │
│  - SearchTime, PlayersFound, RequiredPlayers                │
│  - Update() checks timeout                                   │
│  - ChangeState() manages internal state                      │
│  - Fires events                                              │
└─────────────────────────────────────────────────────────────┘
```

### After (Proposed)
```
┌─────────────────────────────────────────────────────────────┐
│                    GameStateManager                          │
│  ┌────────────────────────────────────────────────────┐     │
│  │         MatchmakingState (Game State)              │     │
│  │  - Tracks SearchTime, PlayersFound                 │     │
│  │  - Detects timeout in Update()                     │     │
│  │  - Calls service.FillWithBots() on timeout         │     │
│  │  - Transitions to GameplayState                    │     │
│  │  - OWNS ALL STATE LOGIC                            │     │
│  └────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────┘
                          │ uses
                          ▼
┌─────────────────────────────────────────────────────────────┐
│           MatchmakingService (Stateless!)                    │
│  - FindMatch() - action                                      │
│  - FillMatchWithBots() - action                              │
│  - CancelMatchmaking() - action                              │
│  - Fires events (data only)                                  │
│  - NO internal state tracking                                │
└─────────────────────────────────────────────────────────────┘
```

## Detailed Refactor Steps

### Step 1: Remove State Enum from MatchmakingService

**File**: `Assets/Scripts/Core/Networking/Common/NetworkTypes.cs`

**Action**: Delete the `MatchmakingState` enum entirely
```csharp
// DELETE THIS:
public enum MatchmakingState
{
    Idle,
    Searching,
    CreatingLobby,
    WaitingForPlayers,
    MatchFound,
    Starting,
    Cancelled,
    Failed
}
```

### Step 2: Remove State Properties from MatchmakingService

**File**: `Assets/Scripts/Core/Networking/Services/MatchmakingService.cs`

**Remove these properties:**
```csharp
// DELETE:
public MatchmakingState CurrentState { get; private set; }
public event Action<MatchmakingState> OnStateChanged;

// DELETE:
private void ChangeState(MatchmakingState newState) { ... }
```

**Keep these (they're data, not state):**
```csharp
// KEEP:
public float SearchTime { get; private set; }
public int PlayersFound { get; private set; }
public int RequiredPlayers { get; private set; }
public bool IsSearching { get; private set; } // Simple flag
```

### Step 3: Simplify MatchmakingService

**File**: `Assets/Scripts/Core/Networking/Services/MatchmakingService.cs`

**Changes:**

1. **Remove `IsSearching` computed property**, replace with simple flag:
```csharp
// BEFORE:
public bool IsSearching => CurrentState != MatchmakingState.Idle && 
                           CurrentState != MatchmakingState.Cancelled && 
                           CurrentState != MatchmakingState.Failed;

// AFTER:
public bool IsSearching { get; private set; }
```

2. **Simplify `FindMatch()`**:
```csharp
public void FindMatch(GameMode gameMode, int teamSize)
{
    if (IsSearching)
    {
        Debug.LogWarning("[MatchmakingService] Already searching");
        return;
    }
    
    _currentGameMode = gameMode;
    _currentTeamSize = teamSize;
    _partySize = _lobbyManager.CurrentPartyLobby?.CurrentPlayers ?? 1;
    RequiredPlayers = teamSize * 2;
    PlayersFound = _partySize;
    _hasFilledWithBots = false;
    
    _botManager.ClearBots();
    
    IsSearching = true; // Simple flag
    _searchStartTime = Time.time;
    
    OnMatchmakingStarted?.Invoke();
    SearchForMatchLobbies(gameMode, teamSize, RequiredPlayers - _partySize);
}
```

3. **Simplify `CancelMatchmaking()`**:
```csharp
public void CancelMatchmaking()
{
    if (!IsSearching)
    {
        Debug.LogWarning("[MatchmakingService] Not searching");
        return;
    }
    
    Debug.Log("[MatchmakingService] Cancelling matchmaking");
    
    // Clean up
    if (_currentSearch != null)
    {
        _currentSearch.Release();
        _currentSearch = null;
    }
    
    // Leave/destroy lobby if needed
    if (_lobbyManager.IsInMatchLobby)
    {
        if (_lobbyManager.IsMatchLobbyOwner)
            _lobbyManager.DestroyMatchLobby();
        else
            _lobbyManager.LeaveMatchLobby();
    }
    
    IsSearching = false;
    OnMatchmakingCancelled?.Invoke();
    ResetMatchmakingState();
}
```

4. **Remove all `ChangeState()` calls**:
```csharp
// DELETE all lines like:
ChangeState(MatchmakingState.Searching);
ChangeState(MatchmakingState.CreatingLobby);
ChangeState(MatchmakingState.WaitingForPlayers);
// etc.
```

### Step 4: Move Timeout Logic to MatchmakingState

**File**: `Assets/Scripts/Core/State/States/MatchmakingState.cs`

**Add these fields:**
```csharp
private float _searchStartTime;
private float _searchTime;
private int _playersFound;
private int _requiredPlayers;
private const float SEARCH_TIMEOUT = 60f;
private bool _hasFilledWithBots;
```

**Enhanced Update() method:**
```csharp
public override void Update()
{
    if (!_isMatchmakingInProgress || _networkingServices == null)
        return;
    
    // Update search time
    _searchTime = Time.time - _searchStartTime;
    
    // Update player counts from service
    _playersFound = _networkingServices.MatchmakingService.PlayersFound;
    _requiredPlayers = _networkingServices.MatchmakingService.RequiredPlayers;
    
    // Check for timeout - trigger bot filling
    if (_searchTime >= SEARCH_TIMEOUT && !_hasFilledWithBots)
    {
        LogMessage($"Matchmaking timeout after {_searchTime:F1}s - filling with bots");
        _hasFilledWithBots = true;
        
        // Tell service to fill with bots
        _networkingServices.MatchmakingService.FillMatchWithBots();
    }
    
    // Update UI with progress (if needed)
    // TODO: Update matchmaking screen with _searchTime, _playersFound, _requiredPlayers
}
```

**Enhanced Enter() method:**
```csharp
public override void Enter()
{
    base.Enter();
    
    _networkingServices = GameBootstrap.Services?.NetworkingServices;
    
    if (_networkingServices == null)
    {
        LogError("NetworkingServices not available");
        CompleteMatchmaking(false);
        return;
    }
    
    // Reset state tracking
    _isMatchmakingInProgress = true;
    _searchStartTime = Time.time;
    _searchTime = 0f;
    _hasFilledWithBots = false;
    _playersFound = 0;
    _requiredPlayers = 0;
    
    // Show UI
    GameBootstrap.Services?.UIService.ShowScreen(UIScreenType.GameModeSelection, true, true);
    
    // Subscribe to events
    _networkingServices.MatchmakingService.OnMatchFound += HandleMatchFound;
    _networkingServices.MatchmakingService.OnMatchmakingFailed += HandleMatchmakingFailed;
    _networkingServices.MatchmakingService.OnPlayersFound += HandlePlayersFound;
    
    // Start matchmaking
    _networkingServices.MatchmakingService.FindMatch(GameMode.Classic, teamSize: 4);
    
    LogMessage("Matchmaking started - searching for players...");
}
```

### Step 5: Update Interface

**File**: `Assets/Scripts/Core/Networking/Interfaces/IMatchmakingService.cs`

**Remove:**
```csharp
// DELETE:
MatchmakingState CurrentState { get; }
event Action<MatchmakingState> OnStateChanged;
```

**Keep:**
```csharp
// KEEP:
bool IsSearching { get; }
float SearchTime { get; }
int PlayersFound { get; }
int RequiredPlayers { get; }

// KEEP all events:
event Action OnMatchmakingStarted;
event Action OnMatchmakingCancelled;
event Action<string> OnMatchmakingFailed;
event Action<int, int> OnPlayersFound;
event Action<LobbyInfo> OnMatchFound;
```

**Make FillMatchWithBots public:**
```csharp
// CHANGE from private to public:
void FillMatchWithBots();
```

### Step 6: Remove Update() from MatchmakingService

**File**: `Assets/Scripts/Core/Networking/Services/MatchmakingService.cs`

**Delete the Update() method entirely:**
```csharp
// DELETE THIS METHOD:
public void Update()
{
    if (!IsSearching)
        return;
    
    SearchTime = Time.time - _searchStartTime;
    
    if (SearchTime >= SEARCH_TIMEOUT && !_hasFilledWithBots)
    {
        FillMatchWithBots();
    }
}
```

**Reason**: MatchmakingState.Update() now handles this.

### Step 7: Make FillMatchWithBots() Public

**File**: `Assets/Scripts/Core/Networking/Services/MatchmakingService.cs`

```csharp
// CHANGE:
private void FillMatchWithBots()

// TO:
public void FillMatchWithBots()
```

**Reason**: MatchmakingState needs to call this when timeout occurs.

## Benefits of This Refactor

### ✅ Single Source of Truth
- Only `GameStateManager` tracks game state
- No confusion about "which state am I in?"

### ✅ Clearer Separation of Concerns
- **MatchmakingService**: Pure service, performs actions, fires events
- **MatchmakingState**: Owns all state logic, makes decisions

### ✅ Easier to Understand
- State flow is linear and clear
- No nested state machines

### ✅ Easier to Debug
- One place to check state: GameStateManager
- Service logs are just actions, not state changes

### ✅ More Testable
- Service can be tested without state logic
- State can be tested without service implementation

## Migration Path

### Phase 1: Make Changes (Breaking)
1. Remove `MatchmakingState` enum
2. Update `MatchmakingService` to be stateless
3. Move logic to `MatchmakingState` game state
4. Update interface

### Phase 2: Test
1. Test matchmaking flow
2. Test timeout and bot filling
3. Test cancellation
4. Test state transitions

### Phase 3: Clean Up
1. Remove unused code
2. Update documentation
3. Update comments

## Files to Modify

1. ✏️ `Assets/Scripts/Core/Networking/Common/NetworkTypes.cs` - Delete enum
2. ✏️ `Assets/Scripts/Core/Networking/Services/MatchmakingService.cs` - Remove state logic
3. ✏️ `Assets/Scripts/Core/Networking/Interfaces/IMatchmakingService.cs` - Update interface
4. ✏️ `Assets/Scripts/Core/State/States/MatchmakingState.cs` - Add state logic
5. 📝 Update documentation

## Estimated Impact

- **Lines Changed**: ~200-300 lines
- **Breaking Changes**: Yes (internal only, no public API changes)
- **Risk Level**: Medium (well-defined refactor)
- **Testing Required**: Full matchmaking flow testing

## Alternative: Keep Both (Not Recommended)

If we keep both state systems:
- Rename `MatchmakingState` enum to `MatchmakingPhase` to avoid confusion
- Keep current architecture
- Document clearly that they serve different purposes

**Why not recommended**: Still confusing, still redundant.

## Recommendation

**Proceed with refactor.** The benefits outweigh the effort, and it will make the codebase much cleaner and easier to maintain.

## Questions to Answer

1. **Do we need MatchmakingService to track its own state?** 
   - **No** - GameStateManager should be the only state tracker

2. **Should services be stateless?**
   - **Yes** - Services should perform actions and fire events, not manage state

3. **Where should timeout logic live?**
   - **In MatchmakingState** - It's state logic, not service logic

4. **Who decides when to fill with bots?**
   - **MatchmakingState** - It tracks time and makes the decision
   - **MatchmakingService** - Just executes the action when told

## Next Steps

1. Review this plan
2. Approve refactor
3. Create feature branch
4. Implement changes
5. Test thoroughly
6. Merge to main

Would you like me to proceed with this refactor?
