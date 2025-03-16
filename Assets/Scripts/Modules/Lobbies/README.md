# RecipeRage Lobby and Matchmaking System

This module provides a comprehensive lobby and matchmaking system for RecipeRage, allowing players to create, join, and manage game lobbies as well as find matches through automated matchmaking.

## Architecture Overview

The system is built with a modular, service-oriented architecture:

```
LobbyModule (Main Entry Point)
├── LobbyService
│   └── LobbyProviders (EOS, Steam, etc.)
└── MatchmakingService
```

### Key Components

- **LobbyModule**: Main entry point that initializes and manages all lobby-related services
- **LobbyService**: Core service for creating and managing lobbies
- **MatchmakingService**: Service for finding matches using the lobby system
- **LobbyProviders**: Platform-specific implementations (EOS, Steam, etc.)

## Getting Started

### 1. Initialize the Lobby Module

Add the `LobbyModule` component to a GameObject in your scene:

```csharp
// In your game initialization code
LobbyModule.Instance.Initialize(success =>
{
    if (success)
    {
        Debug.Log("Lobby system initialized successfully");
    }
    else
    {
        Debug.LogError($"Failed to initialize lobby system: {LobbyModule.Instance.LastError}");
    }
});
```

### 2. Creating and Joining Lobbies

```csharp
// Get the lobby service
ILobbyService lobbyService = LobbyModule.Instance.LobbyService;

// Create a lobby
LobbySettings settings = new LobbySettings
{
    Name = "My Game Lobby",
    MaxPlayers = 4,
    IsPublic = true,
    JoinPermission = LobbyPermission.Public
};

lobbyService.CreateLobby(settings, (success, lobbyInfo) =>
{
    if (success)
    {
        Debug.Log($"Created lobby: {lobbyInfo.Name} ({lobbyInfo.LobbyId})");
    }
});

// Join a lobby by ID
lobbyService.JoinLobby("lobby-id-here", (success, lobbyInfo) =>
{
    if (success)
    {
        Debug.Log($"Joined lobby: {lobbyInfo.Name}");
    }
});
```

### 3. Using Matchmaking

```csharp
// Get the matchmaking service
IMatchmakingService matchmakingService = LobbyModule.Instance.MatchmakingService;

// Create matchmaking options
MatchmakingOptions options = new MatchmakingOptions
{
    GameMode = "casual",
    MinPlayers = 2,
    MaxPlayers = 4,
    UseSkillBasedMatching = true,
    TimeoutSeconds = 120
};

// Add preferred regions
options.PreferredRegions.Add("us-east");
options.PreferredRegions.Add("us-west");

// Start matchmaking
matchmakingService.StartMatchmaking(options, success =>
{
    if (success)
    {
        Debug.Log("Matchmaking started successfully");
    }
    else
    {
        Debug.LogError($"Failed to start matchmaking: {matchmakingService.LastError}");
    }
});

// Cancel matchmaking
matchmakingService.CancelMatchmaking();
```

### 4. Handling Events

Subscribe to events to respond to lobby and matchmaking changes:

```csharp
// Lobby events
lobbyService.OnLobbyCreated += HandleLobbyCreated;
lobbyService.OnLobbyJoined += HandleLobbyJoined;
lobbyService.OnLobbyLeft += HandleLobbyLeft;
lobbyService.OnLobbyUpdated += HandleLobbyUpdated;
lobbyService.OnMemberJoined += HandleMemberJoined;
lobbyService.OnMemberLeft += HandleMemberLeft;

// Matchmaking events
matchmakingService.OnMatchmakingStarted += HandleMatchmakingStarted;
matchmakingService.OnMatchmakingCanceled += HandleMatchmakingCanceled;
matchmakingService.OnMatchmakingComplete += HandleMatchmakingComplete;
matchmakingService.OnMatchmakingFailed += HandleMatchmakingFailed;
matchmakingService.OnMatchmakingStatusUpdated += HandleMatchmakingStatusUpdated;
```

## UI Integration

The module includes a `MatchmakingUIController` that can be used to quickly integrate matchmaking into your UI:

1. Add the `MatchmakingUIController` component to your UI canvas
2. Assign the required UI elements in the inspector
3. The controller will automatically connect to the LobbyModule and handle UI updates

## Platform Support

The system is designed to work with multiple platforms through the provider system:

- **EOS (Epic Online Services)**: Default provider
- **Steam**: Planned for future implementation
- **Custom**: Can be extended for custom backends

## Advanced Usage

### Custom Lobby Attributes

```csharp
// Set custom lobby attributes
Dictionary<string, string> attributes = new Dictionary<string, string>
{
    { "GameMode", "deathmatch" },
    { "MapName", "arena" },
    { "Difficulty", "hard" }
};

lobbyService.UpdateLobbyAttributes(lobbyService.CurrentLobby.LobbyId, attributes);
```

### Player Attributes

```csharp
// Set player attributes
Dictionary<string, string> playerAttributes = new Dictionary<string, string>
{
    { "Role", "healer" },
    { "Level", "42" },
    { "Character", "wizard" }
};

lobbyService.UpdatePlayerAttributes(lobbyService.CurrentLobby.LobbyId, playerAttributes);
```

### Skill-Based Matchmaking

```csharp
// Set player skill rating
matchmakingService.SetSkillRating(1500.0f);

// Create options with skill-based matching
MatchmakingOptions options = new MatchmakingOptions
{
    UseSkillBasedMatching = true,
    MaxSkillRatingDifference = 300.0f
};
```

## Troubleshooting

### Common Issues

1. **Initialization Failures**: Ensure the platform SDK (EOS, Steam) is properly initialized before initializing the LobbyModule.

2. **Connection Issues**: Check network connectivity and platform service status.

3. **Permission Errors**: Verify that the user is logged in and has the necessary permissions.

### Logging

The system uses `LogHelper` for logging. Enable debug logs to see detailed information:

```csharp
LogHelper.SetLogLevel(LogLevel.Debug);
```

## Best Practices

1. **Always check for initialization** before using the services.

2. **Handle all events** to ensure your UI stays in sync with the lobby state.

3. **Implement proper error handling** for all service calls.

4. **Clean up event subscriptions** when your scene unloads to prevent memory leaks.

5. **Use the shutdown method** when exiting the game to ensure clean disconnection.

## License

This module is part of the RecipeRage project and is subject to the same license terms.

## Support

For issues or questions, please contact the RecipeRage development team. 