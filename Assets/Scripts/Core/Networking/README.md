# RecipeRage Networking System

## Overview

PUBG-style networking system with persistent party lobbies and temporary match lobbies.

## Quick Start

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

// Start matchmaking
networking.MatchmakingService.FindMatch(GameMode.Classic, teamSize: 4);

// Handle match found
networking.MatchmakingService.OnMatchFound += (lobbyInfo) =>
{
    Debug.Log("Match ready!");
    StartGame();
};
```

## Architecture

### Services
- **LobbyService**: Party + Match lobby management
- **MatchmakingService**: Search and fill lobbies
- **P2PService**: In-game networking
- **PlayerManager**: Player state management
- **TeamManager**: Team assignment

### Lobby Types
- **Party Lobby**: Persistent, invite-only, survives across matches
- **Match Lobby**: Temporary, public, disbanded after match

### Flow
```
Party Lobby → Matchmaking → Match Lobby → Game → Back to Party Lobby
```

## Key Features

✅ Persistent party lobbies (like PUBG)  
✅ Smart matchmaking (combines parties + solos)  
✅ P2P networking (no dedicated servers)  
✅ SOLID architecture  
✅ Full event system  
✅ Comprehensive error handling  

## Documentation

- **IMPLEMENTATION_GUIDE.md**: Detailed usage guide
- **REFACTORING_COMPLETE.md**: Architecture overview
- **README.md**: This file

## Status

**Phase 1**: ✅ Complete (Core architecture)  
**Phase 2**: ⏳ In Progress (EOS integration)  
**Phase 3**: ⏳ Pending (UI integration)  

## Next Steps

1. Complete EOS integration in LobbyService
2. Remove old RecipeRageNetworkManager
3. Update UI to use new services
4. Test with EOS

See **REFACTORING_COMPLETE.md** for detailed next steps.
