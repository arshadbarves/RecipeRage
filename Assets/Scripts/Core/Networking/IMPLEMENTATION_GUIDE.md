# Networking Implementation Guide

## Overview

This networking system implements a **PUBG-style lobby and matchmaking architecture** with:
- **Party Lobbies**: Persistent lobbies for friends (survives across matches)
- **Match Lobbies**: Temporary lobbies for full games (disbanded after match)
- **Matchmaking**: Combines parties and solo players to fill matches
- **P2P Networking**: Host-based gameplay with no dedicated servers

## Architecture

```
NetworkingServiceContainer (Main Facade)
├── LobbyService (Party + Match lobby management)
├── MatchmakingService (Search, create, fill lobbies)
├── P2PService (In-game networking)
├── PlayerManager (Player state, ready, team)
└── TeamManager (Team assignment, balancing)
```

## Key Components

### 1. LobbyService
**Purpose**: Manages both Party and Match lobbies

**Party Lobby** (Persistent):
- Created when player invites friends
- Survives across multiple matches
- Private, invite-only
- Party leader controls settings

**Match Lobby** (Temporary):
- Created by matchmaking
- Contains all players for the match
- Public for matchmaking
- Disbanded after match ends

**Key Methods**:
```csharp
// Party Lobby
CreatePartyLobby(LobbyConfig config)
InviteToParty(ProductUserId friendId)
LeaveParty()

// Match Lobby
CreateMatchLobby(LobbyConfig config)
JoinMatchLobby(string lobbyId)
LeaveMatchLobby()
```

### 2. MatchmakingService
**Purpose**: Find matches for parties and solo players

**Flow**:
1. Player/party starts matchmaking
2. Search for existing match lobbies with space
3. If found → Join existing lobby
4. If not found → Create new lobby and wait
5. When lobby fills → Match starts

**Key Methods**:
```csharp
FindMatch(GameMode gameMode, int teamSize)
CancelMatchmaking()
SearchForMatchLobbies(...)
CreateAndWaitForPlayers(...)
```

### 3. P2PService
**Purpose**: In-game networking between players

**Features**:
- Host-based architecture
- Reliable ordered packet delivery
- Message types: PlayerAction, ChatMessage, Emote, GameState
- Automatic connection management

**Key Methods**:
```csharp
StartHosting()
ConnectToHost(ProductUserId hostId)
SendPlayerAction(PlayerAction action)
SendChatMessage(string message)
SendToAll(byte messageType, byte[] data)
```

## Usage Examples

### Example 1: Create Party and Invite Friends

```csharp
// Get networking services
var networking = GameBootstrap.Services.NetworkingServices;

// Create party lobby
var config = new LobbyConfig
{
    LobbyName = "My Party",
    MaxPlayers = 4,
    GameMode = GameMode.Classic
};
networking.LobbyManager.CreatePartyLobby(config);

// Invite friends
networking.LobbyManager.InviteToParty(friendProductUserId);

// Subscribe to events
networking.LobbyManager.OnPartyMemberJoined += (player) =>
{
    Debug.Log($"{player.DisplayName} joined the party!");
};
```

### Example 2: Start Matchmaking

```csharp
// Start matchmaking for 4v4
networking.MatchmakingService.FindMatch(GameMode.Classic, teamSize: 4);

// Subscribe to events
networking.MatchmakingService.OnPlayersFound += (current, required) =>
{
    Debug.Log($"Players found: {current}/{required}");
};

networking.MatchmakingService.OnMatchFound += (lobbyInfo) =>
{
    Debug.Log("Match found! Starting game...");
    StartGame();
};
```

### Example 3: Solo Player Matchmaking

```csharp
// Solo player (no party)
networking.MatchmakingService.FindMatch(GameMode.Classic, teamSize: 4);

// Matchmaking will:
// 1. Search for lobbies with space
// 2. Join existing lobby OR create new one
// 3. Wait for lobby to fill
// 4. Start match when full
```

### Example 4: In-Game P2P Communication

```csharp
// Host starts P2P session
if (isHost)
{
    networking.P2PService.StartHosting();
}
else
{
    networking.P2PService.ConnectToHost(hostProductUserId);
}

// Send player action
var action = new PlayerAction
{
    ActionType = PlayerActionType.Move,
    Position = transform.position
};
networking.P2PService.SendPlayerAction(action);

// Receive player actions
networking.P2PService.OnPlayerActionReceived += (player, action) =>
{
    Debug.Log($"{player.DisplayName} moved to {action.Position}");
};
```

### Example 5: Post-Match Flow

```csharp
// When match ends
void OnMatchEnd()
{
    // Leave match lobby
    networking.LobbyManager.LeaveMatchLobby();
    
    // Check if player was in a party
    if (networking.LobbyManager.IsInParty)
    {
        // Return to party lobby (already there)
        ShowPartyLobbyUI();
        
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

## State Flow

### Party + Matchmaking Flow
```
1. Idle
   ↓ CreatePartyLobby()
2. InParty (invite friends, ready up)
   ↓ FindMatch()
3. Matchmaking (searching for opponents)
   ↓ Match found
4. InMatchLobby (all players ready)
   ↓ Start game
5. InGame (P2P session active)
   ↓ Match ends
6. PostGame
   ↓ LeaveMatchLobby()
7. InParty (back to party, can search again)
```

### Solo Player Flow
```
1. Idle
   ↓ FindMatch()
2. Matchmaking
   ↓ Match found
3. InMatchLobby
   ↓ Start game
4. InGame
   ↓ Match ends
5. PostGame
   ↓ LeaveMatchLobby()
6. Idle (back to main menu)
```

## EOS Integration

### Lobby Attributes

**Party Lobby**:
```csharp
{
    "Type": "Party",
    "PartyLeader": "ProductUserId",
    "IsSearching": "false",
    "GameMode": "Classic",
    "TeamSize": "4"
}
```

**Match Lobby**:
```csharp
{
    "Type": "Match",
    "GameMode": "Classic",
    "TeamSize": "4",
    "Status": "Filling", // Filling, Ready, InProgress
    "AvailableSlots": "4",
    "TeamA_Count": "4",
    "TeamB_Count": "4"
}
```

### Matchmaking Search Filters
```csharp
// Search for match lobbies
filters = {
    "Type" == "Match",
    "GameMode" == "Classic",
    "TeamSize" == "4",
    "Status" == "Filling",
    "AvailableSlots" >= partySize
}
```

## Events

### LobbyService Events
```csharp
// Party events
OnPartyCreated(Result, LobbyInfo)
OnPartyMemberJoined(PlayerInfo)
OnPartyMemberLeft(PlayerInfo)
OnPartyUpdated()

// Match lobby events
OnMatchLobbyCreated(Result, LobbyInfo)
OnMatchLobbyJoined(Result, LobbyInfo)
OnMatchLobbyLeft()
OnMatchLobbyUpdated()

// State events
OnLobbyStateChanged(LobbyState)
OnError(string)
```

### MatchmakingService Events
```csharp
OnMatchmakingStarted()
OnMatchmakingCancelled()
OnMatchmakingFailed(string)
OnPlayersFound(int current, int required)
OnMatchFound(LobbyInfo)
OnStateChanged(MatchmakingState)
```

### P2PService Events
```csharp
OnPlayerActionReceived(PlayerInfo, PlayerAction)
OnChatMessageReceived(PlayerInfo, string)
OnEmoteReceived(PlayerInfo, int)
OnGameStateReceived(byte[])
OnPlayerConnected(ProductUserId)
OnPlayerDisconnected(ProductUserId)
```

## Integration with GameBootstrap

```csharp
// In GameBootstrap.cs
private void InitializeNetworking()
{
    // Create networking services
    var networkingServices = new NetworkingServiceContainer();
    _services.RegisterNetworkingServices(networkingServices);
    
    // Create updater for Update loop
    var updater = gameObject.AddComponent<NetworkingServiceUpdater>();
    updater.Initialize(networkingServices);
}
```

## Best Practices

1. **Always check IsInParty** before accessing CurrentPartyLobby
2. **Subscribe to events** before calling methods (to catch immediate callbacks)
3. **Unsubscribe from events** when done to prevent memory leaks
4. **Handle errors** via OnError events
5. **Cancel matchmaking** when player leaves UI
6. **Disconnect P2P** when leaving match
7. **Leave match lobby** before leaving party

## Testing Scenarios

### Scenario 1: 4-Player Party vs 4-Player Party
- Party A (4 players) starts matchmaking
- Party B (4 players) starts matchmaking
- Matchmaking finds both parties
- Creates match lobby with 8 players
- Match starts

### Scenario 2: 2-Player Party + 2 Solos vs 4-Player Party
- Party A (2 players) starts matchmaking
- 2 solo players start matchmaking
- Party B (4 players) starts matchmaking
- Matchmaking combines Party A + 2 solos (Team A)
- Party B (Team B)
- Match starts

### Scenario 3: Solo Player Quick Match
- Solo player starts matchmaking
- Joins existing match lobby with space
- OR creates new lobby and waits
- Match starts when full

## Next Steps

1. ✅ Core data models created
2. ✅ Interfaces defined
3. ✅ Services implemented (LobbyService, MatchmakingService, P2PService)
4. ✅ NetworkingServiceContainer updated
5. ⏳ Complete EOS integration in LobbyService (CreateLobbyInternal, JoinLobbyInternal)
6. ⏳ Remove old RecipeRageNetworkManager
7. ⏳ Update UI to use new services
8. ⏳ Test with EOS

## Migration from Old System

### Old (RecipeRageNetworkManager)
```csharp
RecipeRageNetworkManager.Instance.CreateGame(...)
RecipeRageNetworkManager.Instance.JoinGame(...)
```

### New (NetworkingServiceContainer)
```csharp
var networking = GameBootstrap.Services.NetworkingServices;
networking.LobbyManager.CreatePartyLobby(...)
networking.MatchmakingService.FindMatch(...)
```

## Troubleshooting

**Issue**: Matchmaking timeout
- **Solution**: Increase SEARCH_TIMEOUT or create lobby immediately

**Issue**: P2P connection failed
- **Solution**: Check NAT traversal, ensure EOS P2P is configured

**Issue**: Lobby not found after search
- **Solution**: Check lobby attributes match search filters

**Issue**: Party disbanded after match
- **Solution**: Only call LeaveMatchLobby(), not LeaveParty()
