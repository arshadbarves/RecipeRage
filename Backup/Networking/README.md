# Modular Networking System

This directory contains a modular networking system that can be used for multiplayer games. The system is designed to be flexible and reusable across different games.

## Directory Structure

- **Common**: Contains common networking classes and interfaces that are not specific to any particular networking implementation.
- **EOS**: Contains the Epic Online Services implementation of the networking system.
- **Interfaces**: Contains interfaces that define the contract for networking implementations.

## Components

### Common

- **NetworkManager**: The main entry point for the networking system. It manages the network service and provides a high-level API for the game.
- **NetworkTypes**: Contains common networking types such as NetworkPlayer, NetworkSessionInfo, NetworkConnectionState, and NetworkMessage.

### Interfaces

- **INetworkService**: Defines the contract for network service implementations. Any network service implementation must implement this interface.

### EOS

- **EOSNetworkService**: Implements the INetworkService interface using Epic Online Services.
- **EOSNetworkManager**: A MonoBehaviour that manages the EOS SDK initialization and login process.
- **EOSAdapter**: Provides a simplified interface to the EOS SDK.
- **EOSConstants**: Contains constants and enums for EOS SDK compatibility.

## Usage

1. Add the NetworkManager prefab to your scene.
2. Access the network manager through the ServiceLocator:

```csharp
NetworkManager networkManager = ServiceLocator.Instance.Get<NetworkManager>();
```

3. Use the network manager to create or join sessions:

```csharp
// Create a session
networkManager.CreateSession("My Game", 4, false);

// Join a session
networkManager.JoinSession("session-id");

// Find sessions
networkManager.FindSessions(sessions => {
    foreach (var session in sessions) {
        Debug.Log($"Found session: {session.SessionName}");
    }
});
```

4. Send and receive messages:

```csharp
// Register a message handler
networkManager.RegisterMessageHandler(MessageType.GameState, HandleGameState);

// Send a message to all players
byte[] data = SerializeGameState();
networkManager.SendToAll(MessageType.GameState, data);

// Handle incoming messages
private void HandleGameState(NetworkMessage message) {
    GameState gameState = DeserializeGameState(message.Data);
    // Update game state
}
```

## Extending the System

To add support for a new networking implementation:

1. Create a new directory for your implementation (e.g., `Steam`).
2. Create a new class that implements the `INetworkService` interface.
3. Update the `NetworkManager` to use your new implementation.

## Implementation Details

### EOS Implementation

The EOS implementation uses EOS Sessions for matchmaking and lobby management, and EOS P2P for direct communication between players. The system handles:

- Session creation and joining
- Player discovery and tracking
- Message serialization and routing
- Connection state management
- Host migration

## References

- [EOS SDK Documentation](https://dev.epicgames.com/docs/services/en-US/)
- [EOS Sessions Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/Sessions/index.html)
- [EOS P2P Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/P2P/index.html)
