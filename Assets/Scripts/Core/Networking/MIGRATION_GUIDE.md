# Migration Guide: Old → New Networking System

## Overview

The old networking system (`RecipeRageNetworkManager`, `RecipeRageSessionManager`) has been replaced with a new PUBG-style architecture using `NetworkingServiceContainer`.

## Quick Migration

### Old System (Deprecated)
```csharp
// ❌ OLD - Don't use
RecipeRageNetworkManager.Instance.CreateGame(...)
RecipeRageNetworkManager.Instance.JoinGame(...)
RecipeRageNetworkManager.Instance.LeaveGame()

RecipeRageLobbyManager.Instance.CreateLobby(...)
RecipeRageLobbyManager.Instance.JoinLobby(...)

RecipeRageP2PManager.Instance.SendPlayerAction(...)
```

### New System (Current)
```csharp
// ✅ NEW - Use this
var networking = GameBootstrap.Services.NetworkingServices;

// Party Lobby
networking.LobbyManager.CreatePartyLobby(config);
networking.LobbyManager.InviteToParty(friendId);
networking.LobbyManager.LeaveParty();

// Matchmaking
networking.MatchmakingService.FindMatch(gameMode, teamSize);
networking.MatchmakingService.CancelMatchmaking();

// P2P
networking.P2PService.SendPlayerAction(action);
networking.P2PService.SendChatMessage(message);
```

## Detailed Migration

### 1. Creating a Game

**OLD:**
```csharp
RecipeRageNetworkManager.Instance.CreateGame(
    sessionName: "MyGame",
    gameMode: GameMode.Classic,
    mapName: "Kitchen",
    maxPlayers: 4,
    isPrivate: false
);
```

**NEW:**
```csharp
var networking = GameBootstrap.Services.NetworkingServices;

// Create party lobby
var config = new LobbyConfig
{
    LobbyName = "MyGame",
    MaxPlayers = 4,
    GameMode = GameMode.Classic,
    MapName = "Kitchen",
    IsPrivate = true // Party lobbies are always private
};
networking.LobbyManager.CreatePartyLobby(config);

// Then start matchmaking
networking.MatchmakingService.FindMatch(GameMode.Classic, teamSize: 4);
```

### 2. Joining a Game

**OLD:**
```csharp
RecipeRageNetworkManager.Instance.JoinGame(sessionId);
```

**NEW:**
```csharp
var networking = GameBootstrap.Services.NetworkingServices;

// For friend invites - accept invite
networking.LobbyManager.InviteToParty(friendId);

// For matchmaking - automatic
networking.MatchmakingService.FindMatch(GameMode.Classic, teamSize: 4);
// Matchmaking will auto-join when match is found
```

### 3. Leaving a Game

**OLD:**
```csharp
RecipeRageNetworkManager.Instance.LeaveGame();
```

**NEW:**
```csharp
var networking = GameBootstrap.Services.NetworkingServices;

// Leave match lobby (after game ends)
networking.LobbyManager.LeaveMatchLobby();

// Leave party (if you want to disband party)
networking.LobbyManager.LeaveParty();
```

### 4. Sending P2P Messages

**OLD:**
```csharp
RecipeRageP2PManager.Instance.SendPlayerAction(action);
RecipeRageP2PManager.Instance.SendChatMessage(message);
```

**NEW:**
```csharp
var networking = GameBootstrap.Services.NetworkingServices;

networking.P2PService.SendPlayerAction(action);
networking.P2PService.SendChatMessage(message);
networking.P2PService.SendEmote(emoteId);
```

### 5. Event Subscriptions

**OLD:**
```csharp
RecipeRageNetworkManager.Instance.OnGameCreated += OnGameCreated;
RecipeRageNetworkManager.Instance.OnGameJoined += OnGameJoined;
RecipeRageNetworkManager.Instance.OnGameStarted += OnGameStarted;

RecipeRageLobbyManager.Instance.OnLobbyUpdated += OnLobbyUpdated;
```

**NEW:**
```csharp
var networking = GameBootstrap.Services.NetworkingServices;

// Party events
networking.LobbyManager.OnPartyCreated += (result, lobbyInfo) => { };
networking.LobbyManager.OnPartyMemberJoined += (player) => { };
networking.LobbyManager.OnPartyUpdated += () => { };

// Matchmaking events
networking.MatchmakingService.OnMatchmakingStarted += () => { };
networking.MatchmakingService.OnPlayersFound += (current, required) => { };
networking.MatchmakingService.OnMatchFound += (lobbyInfo) => { };

// Match lobby events
networking.LobbyManager.OnMatchLobbyJoined += (result, lobbyInfo) => { };
networking.LobbyManager.OnMatchLobbyUpdated += () => { };

// P2P events
networking.P2PService.OnPlayerActionReceived += (player, action) => { };
networking.P2PService.OnChatMessageReceived += (player, message) => { };
```

## Key Differences

### 1. Two Types of Lobbies

**OLD:** Single lobby type
**NEW:** Party Lobby (persistent) + Match Lobby (temporary)

```csharp
// Party Lobby - Your squad, survives across matches
networking.LobbyManager.CreatePartyLobby(config);

// Match Lobby - Full game, created by matchmaking
// (You don't create this directly, matchmaking does)
```

### 2. Matchmaking Flow

**OLD:** Create game → Wait for players
**NEW:** Create party → Start matchmaking → Auto-join match lobby

```csharp
// 1. Create party (optional, can skip for solo)
networking.LobbyManager.CreatePartyLobby(config);

// 2. Invite friends (optional)
networking.LobbyManager.InviteToParty(friendId);

// 3. Start matchmaking
networking.MatchmakingService.FindMatch(GameMode.Classic, teamSize: 4);

// 4. Wait for match found event
networking.MatchmakingService.OnMatchFound += (lobbyInfo) =>
{
    // Match ready! Start game
    StartGame();
};
```

### 3. Post-Match Flow

**OLD:** Leave game → Back to main menu
**NEW:** Leave match lobby → Return to party lobby

```csharp
// After match ends
networking.LobbyManager.LeaveMatchLobby();

// Check if in party
if (networking.LobbyManager.IsInParty)
{
    // Still in party, can search again
    ShowPartyLobbyUI();
}
else
{
    // Solo player, return to main menu
    ShowMainMenu();
}
```

## State Management Migration

### OLD: MatchmakingState
```csharp
_networkManager = RecipeRageNetworkManager.Instance;
_lobbyManager = _networkManager?.LobbyManager;

_lobbyManager.OnLobbyUpdated += HandleLobbyUpdated;
_networkManager.OnGameStarted += HandleGameStarted;

_networkManager.CreateGame(sessionName, GameMode.Classic, "Kitchen", 4, false);
```

### NEW: MatchmakingState
```csharp
_networkingServices = GameBootstrap.Services.NetworkingServices;

_networkingServices.MatchmakingService.OnMatchFound += HandleMatchFound;
_networkingServices.MatchmakingService.OnMatchmakingFailed += HandleMatchmakingFailed;
_networkingServices.MatchmakingService.OnPlayersFound += HandlePlayersFound;

_networkingServices.MatchmakingService.FindMatch(GameMode.Classic, teamSize: 4);
```

## UI Component Migration

### OLD: LobbyTabComponent
```csharp
private RecipeRageLobbyManager _lobbyManager;

void Start()
{
    _lobbyManager = RecipeRageNetworkManager.Instance?.LobbyManager;
    _lobbyManager.OnLobbyUpdated += UpdateUI;
}

void CreateLobby()
{
    RecipeRageNetworkManager.Instance.CreateGame(...);
}
```

### NEW: LobbyTabComponent
```csharp
private INetworkingServices _networking;

void Start()
{
    _networking = GameBootstrap.Services.NetworkingServices;
    _networking.LobbyManager.OnPartyUpdated += UpdateUI;
}

void CreateParty()
{
    var config = new LobbyConfig { ... };
    _networking.LobbyManager.CreatePartyLobby(config);
}

void StartMatchmaking()
{
    _networking.MatchmakingService.FindMatch(GameMode.Classic, teamSize: 4);
}
```

## Common Patterns

### Pattern 1: Solo Player Quick Match
```csharp
var networking = GameBootstrap.Services.NetworkingServices;

// No party needed, just start matchmaking
networking.MatchmakingService.FindMatch(GameMode.Classic, teamSize: 4);

networking.MatchmakingService.OnMatchFound += (lobbyInfo) =>
{
    Debug.Log("Match found! Starting game...");
    StartGame();
};
```

### Pattern 2: Party with Friends
```csharp
var networking = GameBootstrap.Services.NetworkingServices;

// 1. Create party
var config = new LobbyConfig
{
    LobbyName = "My Squad",
    MaxPlayers = 4,
    GameMode = GameMode.Classic
};
networking.LobbyManager.CreatePartyLobby(config);

// 2. Invite friends
networking.LobbyManager.InviteToParty(friend1Id);
networking.LobbyManager.InviteToParty(friend2Id);

// 3. When ready, start matchmaking
networking.MatchmakingService.FindMatch(GameMode.Classic, teamSize: 4);
```

### Pattern 3: Post-Match Return to Party
```csharp
void OnMatchEnd()
{
    var networking = GameBootstrap.Services.NetworkingServices;
    
    // Leave match lobby
    networking.LobbyManager.LeaveMatchLobby();
    
    // Check if in party
    if (networking.LobbyManager.IsInParty)
    {
        // Return to party lobby UI
        ShowPartyLobbyScreen();
        
        // Party can immediately search again
        // networking.MatchmakingService.FindMatch(...)
    }
    else
    {
        // Solo player - return to main menu
        ShowMainMenu();
    }
}
```

## Deprecated Classes

These classes are marked as `[Obsolete]` and will be removed in a future version:

- ❌ `RecipeRageNetworkManager` - Deleted
- ❌ `RecipeRageSessionManager` - Deleted
- ⚠️ `RecipeRageLobbyManager` - Deprecated (use `NetworkingServices.LobbyManager`)
- ⚠️ `RecipeRageP2PManager` - Deprecated (use `NetworkingServices.P2PService`)

## Testing Your Migration

### Checklist
- [ ] Replace all `RecipeRageNetworkManager.Instance` calls
- [ ] Replace all `RecipeRageLobbyManager` references
- [ ] Replace all `RecipeRageP2PManager` references
- [ ] Update event subscriptions
- [ ] Test party creation
- [ ] Test friend invites
- [ ] Test matchmaking
- [ ] Test post-match flow
- [ ] Remove deprecated class usages

### Compilation Warnings
If you see warnings like:
```
'RecipeRageLobbyManager' is obsolete: 'Use GameBootstrap.Services.NetworkingServices instead'
```

This means you need to migrate that code to use the new system.

## Need Help?

See these documents for more information:
- **IMPLEMENTATION_GUIDE.md** - Complete usage guide
- **README.md** - Quick reference
- **PHASE_2_COMPLETE.md** - What's new in Phase 2

## Summary

**Key Changes:**
1. ✅ Use `GameBootstrap.Services.NetworkingServices` instead of singletons
2. ✅ Two lobby types: Party (persistent) + Match (temporary)
3. ✅ Matchmaking service handles finding matches
4. ✅ Post-match returns to party lobby (not main menu)
5. ✅ Event-driven architecture with proper callbacks

**Benefits:**
- ✅ SOLID principles
- ✅ Better testability
- ✅ Cleaner separation of concerns
- ✅ PUBG-style lobby flow
- ✅ No singletons
