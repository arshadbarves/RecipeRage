# EOS Networking System

This directory contains the implementation of the networking system using Epic Online Services (EOS) for RecipeRage.

## Overview

The networking system is built on top of the Epic Online Services SDK and provides functionality for:

- Session management (create, join, find, leave)
- Peer-to-peer communication
- Player management
- Message handling

## Components

### EOSNetworkManager

The `EOSNetworkManager` is a MonoBehaviour that manages the EOS SDK initialization and login process. It provides a high-level interface for the game to interact with EOS.

### EOSNetworkService

The `EOSNetworkService` implements the `INetworkService` interface and provides the core networking functionality using EOS. It handles:

- Session creation and management
- Peer-to-peer communication
- Player tracking
- Message routing

### EOSAdapter

The `EOSAdapter` provides a simplified interface to the EOS SDK, handling authentication and other low-level operations.

### EOSConstants

The `EOSConstants` class provides constants and enums for EOS SDK compatibility.

## Usage

1. Add the `EOSNetworkManager` prefab to your scene
2. Access the network manager through the `ServiceLocator`:

```csharp
EOSNetworkManager networkManager = ServiceLocator.Instance.Get<EOSNetworkManager>();
```

3. Use the network manager to initialize EOS and login:

```csharp
networkManager.InitializeEOS();
networkManager.LoginWithDeviceID("PlayerName");
```

4. Access the network service to perform networking operations:

```csharp
EOSNetworkService networkService = networkManager.GetNetworkService();
networkService.CreateSession("My Game", 4, false);
```

## Message Handling

To send and receive messages:

1. Register a message handler:

```csharp
networkService.RegisterMessageHandler(MessageType.GameState, HandleGameState);
```

2. Send messages:

```csharp
byte[] data = SerializeGameState();
networkService.SendToAll(MessageType.GameState, data);
```

3. Handle incoming messages:

```csharp
private void HandleGameState(NetworkMessage message)
{
    GameState gameState = DeserializeGameState(message.Data);
    // Update game state
}
```

## Implementation Details

The networking system uses EOS Sessions for matchmaking and lobby management, and EOS P2P for direct communication between players. The system handles:

- Session creation and joining
- Player discovery and tracking
- Message serialization and routing
- Connection state management
- Host migration

## References

- [EOS SDK Documentation](https://dev.epicgames.com/docs/services/en-US/)
- [EOS Sessions Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/Sessions/index.html)
- [EOS P2P Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/P2P/index.html)
