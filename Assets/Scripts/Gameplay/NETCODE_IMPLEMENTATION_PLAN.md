# Netcode Implementation Plan for RecipeRage

## Overview
This document outlines the complete implementation plan for Unity Netcode for GameObjects in RecipeRage, an Overcooked-style multiplayer cooking game using P2P (Peer-to-Peer) networking via Epic Online Services (EOS).

## Architecture Summary

### Network Model
- **Type**: P2P Host-Client
- **Transport**: EOS P2P Transport
- **Host Role**: Acts as both server and client (authoritative)
- **Client Role**: Connects to host, sends requests via ServerRpc
- **Max Players**: 2-4 players per game session

### Key Principles
1. **Host Authority**: All game logic executes on host, syncs to clients
2. **Client Prediction**: Local player movement predicted, reconciled with server
3. **State Synchronization**: NetworkVariables sync state from host to clients
4. **Action Validation**: All player actions validated on host via ServerRpc
5. **Event Broadcasting**: Host broadcasts events to clients via ClientRpc

## Phase 1: Core Networking Infrastructure

### 1.1 Network Game Manager
**File**: `Core/Networking/Services/NetworkGameManager.cs`

**Purpose**: Centralized network object lifecycle management

**Responsibilities**:
- Spawn/despawn network objects (players, ingredients, stations)
- Manage game session lifecycle
- Handle player connections/disconnections
- Coordinate with existing GameStateManager

**Interface**:
```csharp
public interface INetworkGameManager
{
    void StartGame();
    void EndGame();
    void SpawnPlayer(ulong clientId, Vector3 spawnPosition);
    void DespawnPlayer(ulong clientId);
    NetworkObject SpawnNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation);
    void DespawnNetworkObject(NetworkObject networkObject);
    bool IsGameActive { get; }
    event Action<ulong> OnPlayerJoined;
    event Action<ulong> OnPlayerLeft;
}
```

**Integration Points**:
- Registers with `ServiceContainer` in `GameBootstrap`
- Works with `IGameStateManager` for state transitions
- Uses `EOSTransport` for network communication

### 1.2 Player Network Manager
**File**: `Core/Networking/Services/PlayerNetworkManager.cs`

**Purpose**: Track and manage all connected players

**Responsibilities**:
- Register/unregister players on connect/disconnect
- Provide player lookup by clientId
- Track player states (ready, playing, disconnected)
- Manage player spawn points

**Interface**:
```csharp
public interface IPlayerNetworkManager
{
    void RegisterPlayer(ulong clientId, PlayerController player);
    void UnregisterPlayer(ulong clientId);
    PlayerController GetPlayer(ulong clientId);
    IReadOnlyList<PlayerController> GetAllPlayers();
    int GetPlayerCount();
    bool IsPlayerRegistered(ulong clientId);
    event Action<PlayerController> OnPlayerRegistered;
    event Action<ulong> OnPlayerUnregistered;
}
```

**Integration Points**:
- Registers with `ServiceContainer`
- Used by `NetworkGameManager` for player spawning
- Accessed by gameplay systems to get player references

### 1.3 Network Object Pool
**File**: `Core/Networking/Services/NetworkObjectPool.cs`

**Purpose**: Efficient network object reuse (Object Pool Pattern)

**Responsibilities**:
- Pool frequently spawned objects (ingredients, plates)
- Reduce network spawn/despawn overhead
- Manage object lifecycle

**Interface**:
```csharp
public interface INetworkObjectPool
{
    NetworkObject Get(GameObject prefab, Vector3 position, Quaternion rotation);
    void Return(NetworkObject networkObject);
    void Prewarm(GameObject prefab, int count);
    void Clear();
}
```

## Phase 2: Gameplay Systems

### 2.1 Enhanced Ingredient System

#### IngredientItem Improvements
**File**: `Gameplay/Cooking/IngredientItem.cs` (Enhanced)

**Changes**:
- Implement `INetworkSerializable` for IngredientState (performance)
- Add validation in ServerRpc methods
- Improve state synchronization
- Add ownership tracking

**New NetworkVariables**:
```csharp
private NetworkVariable<IngredientState> _state; // Already exists
private NetworkVariable<ulong> _ownerId; // New - who's holding it
private NetworkVariable<bool> _isOnStation; // New - is it on a station
private NetworkVariable<ulong> _stationId; // New - which station
```

**New Methods**:
```csharp
[ServerRpc(RequireOwnership = false)]
public void RequestPickupServerRpc(ulong playerId, ServerRpcParams rpcParams = default);

[ServerRpc(RequireOwnership = false)]
public void RequestDropServerRpc(Vector3 dropPosition, ServerRpcParams rpcParams = default);

[ServerRpc(RequireOwnership = false)]
public void RequestPlaceOnStationServerRpc(ulong stationNetworkId, ServerRpcParams rpcParams = default);
```

#### Ingredient Network Spawner
**File**: `Gameplay/Cooking/IngredientNetworkSpawner.cs`

**Purpose**: Network-aware ingredient spawning

**Responsibilities**:
- Spawn ingredients on host
- Use object pool for efficiency
- Sync spawns to all clients
- Handle ingredient despawning

**Interface**:
```csharp
public class IngredientNetworkSpawner : NetworkBehaviour
{
    public NetworkObject SpawnIngredient(Ingredient ingredientData, Vector3 position);
    public void DespawnIngredient(NetworkObject ingredientObject);
    public void SpawnIngredientAtStation(Ingredient ingredientData, ulong stationNetworkId);
}
```

### 2.2 Enhanced Station System

#### Station Network Controller
**File**: `Gameplay/Stations/StationNetworkController.cs`

**Purpose**: Manage station network state and interactions

**Responsibilities**:
- Track who's using the station
- Prevent multiple players using same station
- Validate station interactions
- Sync station state changes

**NetworkVariables**:
```csharp
private NetworkVariable<ulong> _currentUserId; // Who's using it
private NetworkVariable<bool> _isLocked; // Is it in use
private NetworkVariable<StationState> _state; // Current state
```

**Methods**:
```csharp
[ServerRpc(RequireOwnership = false)]
public void RequestUseStationServerRpc(ulong playerId, ServerRpcParams rpcParams = default);

[ServerRpc(RequireOwnership = false)]
public void ReleaseStationServerRpc(ulong playerId, ServerRpcParams rpcParams = default);

[ClientRpc]
public void UpdateStationVisualsClientRpc(StationState newState);
```

#### Enhanced CookingStation
**File**: `Gameplay/Stations/CookingStation.cs` (Enhanced)

**Changes**:
- Integrate with `StationNetworkController`
- Add proper validation for all interactions
- Improve progress synchronization
- Add cooking timer network sync

**Improvements**:
```csharp
// Better progress tracking
private NetworkVariable<float> _cookingProgress;
private NetworkVariable<float> _cookingStartTime;

// Validation
private bool ValidatePlayerCanInteract(ulong playerId);
private bool ValidateIngredientCanBeProcessed(IngredientItem ingredient);
```

### 2.3 Dish Assembly System

#### Dish Assembly Validator
**File**: `Gameplay/Cooking/DishAssemblyValidator.cs`

**Purpose**: Validate assembled dishes against recipes (Strategy Pattern)

**Responsibilities**:
- Check if ingredients match recipe requirements
- Calculate dish quality score
- Determine if dish is perfect/good/acceptable
- Support different validation strategies per game mode

**Interface**:
```csharp
public interface IDishValidator
{
    bool ValidateDish(List<IngredientItem> ingredients, Recipe recipe);
    DishQuality GetDishQuality(List<IngredientItem> ingredients, Recipe recipe);
    int CalculateScore(List<IngredientItem> ingredients, Recipe recipe, float timeRemaining);
}

public enum DishQuality
{
    Perfect,    // All ingredients correct, perfect timing
    Good,       // All ingredients correct, good timing
    Acceptable, // All ingredients correct, slow timing
    Wrong       // Missing or incorrect ingredients
}
```

**Implementation**:
```csharp
public class StandardDishValidator : IDishValidator
{
    // Standard validation logic
}

public class TimedDishValidator : IDishValidator
{
    // Time-focused validation (Time Attack mode)
}

public class TeamDishValidator : IDishValidator
{
    // Team-based validation (Team Battle mode)
}
```

#### Enhanced Assembly Station
**File**: `Gameplay/Stations/AssemblyStation.cs` (Enhanced)

**Changes**:
- Use `IDishValidator` for dish validation
- Properly sync plate and ingredient states
- Add dish completion logic
- Integrate with order system

**New Methods**:
```csharp
[ServerRpc(RequireOwnership = false)]
public void RequestAssembleDishServerRpc(int recipeId, ServerRpcParams rpcParams = default);

private bool ValidateDishAssembly(List<IngredientItem> ingredients, Recipe recipe);
private void CompleteDish(Recipe recipe, DishQuality quality);
```

#### Plate System
**File**: `Gameplay/Cooking/PlateItem.cs` (New)

**Purpose**: Represent a plate that holds ingredients

**NetworkVariables**:
```csharp
private NetworkList<ulong> _ingredientIds; // Ingredients on plate
private NetworkVariable<int> _recipeId; // Target recipe
private NetworkVariable<bool> _isComplete; // Is dish complete
```

### 2.4 Network Score Manager

#### Network Score Manager
**File**: `Gameplay/Scoring/NetworkScoreManager.cs`

**Purpose**: Synchronize scores across all clients

**Responsibilities**:
- Track individual player scores
- Track team scores (for team modes)
- Calculate score bonuses
- Sync scoreboard updates
- Handle score events

**NetworkVariables**:
```csharp
private NetworkList<PlayerScore> _playerScores;
private NetworkVariable<int> _teamAScore; // For team modes
private NetworkVariable<int> _teamBScore; // For team modes
```

**Methods**:
```csharp
[ServerRpc(RequireOwnership = false)]
public void AddScoreServerRpc(ulong playerId, int points, ScoreReason reason, ServerRpcParams rpcParams = default);

[ServerRpc(RequireOwnership = false)]
public void AddTeamScoreServerRpc(int teamId, int points, ScoreReason reason, ServerRpcParams rpcParams = default);

[ClientRpc]
public void UpdateScoreboardClientRpc(PlayerScore[] scores);

[ClientRpc]
public void ShowScorePopupClientRpc(ulong playerId, int points, ScoreReason reason);
```

**Data Structures**:
```csharp
public struct PlayerScore : INetworkSerializable
{
    public ulong ClientId;
    public int Score;
    public int DishesCompleted;
    public int PerfectDishes;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter;
}

public enum ScoreReason
{
    DishCompleted,
    PerfectDish,
    TimeBonus,
    ComboBonus,
    TeamworkBonus
}
```

#### Enhanced Score Manager
**File**: `Gameplay/Scoring/ScoreManager.cs` (Enhanced)

**Changes**:
- Integrate with `NetworkScoreManager`
- Add combo system
- Add teamwork bonus calculation
- Improve score calculation logic

## Phase 3: Game Flow Integration

### 3.1 Network Game State Manager

#### Network Game State Manager
**File**: `Core/State/NetworkGameStateManager.cs`

**Purpose**: Synchronize game state across all clients

**Responsibilities**:
- Sync game phase (waiting, prep, playing, results)
- Manage round timer
- Coordinate state transitions
- Integrate with existing `GameStateManager`

**NetworkVariables**:
```csharp
private NetworkVariable<GamePhase> _currentPhase;
private NetworkVariable<float> _phaseStartTime;
private NetworkVariable<float> _phaseDuration;
```

**Methods**:
```csharp
[ServerRpc(RequireOwnership = false)]
public void RequestStartGameServerRpc(ServerRpcParams rpcParams = default);

[ClientRpc]
public void ChangePhaseClientRpc(GamePhase newPhase, float duration);

[ClientRpc]
public void ShowCountdownClientRpc(int seconds);
```

**Game Phases**:
```csharp
public enum GamePhase
{
    Waiting,      // Waiting for players
    Preparation,  // 10 second countdown
    Playing,      // Active gameplay
    Results       // Show scores
}
```

### 3.2 Round Timer

#### Round Timer
**File**: `Gameplay/RoundTimer.cs`

**Purpose**: Synchronized countdown timer

**Responsibilities**:
- Count down from round start time
- Sync time across all clients
- Trigger events at time milestones
- Handle time bonuses

**NetworkVariables**:
```csharp
private NetworkVariable<float> _timeRemaining;
private NetworkVariable<bool> _isRunning;
```

**Methods**:
```csharp
[ServerRpc(RequireOwnership = false)]
public void StartTimerServerRpc(float duration);

[ServerRpc(RequireOwnership = false)]
public void PauseTimerServerRpc();

[ClientRpc]
public void SyncTimeClientRpc(float timeRemaining);
```

### 3.3 Order Manager Integration

#### Enhanced Order Manager
**File**: `Gameplay/Cooking/OrderManager.cs` (Enhanced)

**Changes**:
- Already uses NetworkList (good!)
- Add better event handling
- Improve order completion validation
- Add order priority system

**Improvements**:
```csharp
[ServerRpc(RequireOwnership = false)]
public void CompleteOrderWithDishServerRpc(int orderId, ulong plateNetworkId, ServerRpcParams rpcParams = default);

private bool ValidateOrderCompletion(RecipeOrderState order, PlateItem plate);
private int CalculateOrderScore(RecipeOrderState order, DishQuality quality);
```

## Phase 4: P2P-Specific Considerations

### 4.1 Connection Management

#### Connection Handler
**File**: `Core/Networking/Services/ConnectionHandler.cs`

**Purpose**: Handle player connections/disconnections gracefully

**Responsibilities**:
- Detect disconnections
- Clean up disconnected player objects
- Redistribute owned objects
- Notify other players

**Methods**:
```csharp
public void OnClientConnected(ulong clientId);
public void OnClientDisconnected(ulong clientId);
private void CleanupPlayerObjects(ulong clientId);
private void RedistributeOwnedObjects(ulong clientId);
```

### 4.2 Latency Compensation

#### Client Prediction
**File**: `Core/Networking/ClientPrediction.cs`

**Purpose**: Predict local player movement to hide latency

**Responsibilities**:
- Predict player movement locally
- Reconcile with server state
- Smooth out corrections

**Implementation**:
```csharp
public class ClientPrediction : MonoBehaviour
{
    private Queue<PlayerInput> _inputHistory;
    private Vector3 _predictedPosition;
    
    public void PredictMovement(Vector2 input);
    public void ReconcileWithServer(Vector3 serverPosition);
}
```

#### Lag Compensation
**File**: `Core/Networking/LagCompensation.cs`

**Purpose**: Compensate for network latency in interactions

**Responsibilities**:
- Store historical positions
- Rewind time for hit detection
- Validate interactions with latency consideration

### 4.3 Bandwidth Optimization

#### Network Serialization
**File**: `Core/Networking/NetworkSerialization.cs`

**Purpose**: Efficient data serialization

**Implementations**:
```csharp
// Efficient IngredientState serialization
public struct IngredientState : INetworkSerializable
{
    public int IngredientId;
    public byte Flags; // Packed: IsCut, IsCooked, IsBurned
    public byte Progress; // 0-255 instead of float
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref IngredientId);
        serializer.SerializeValue(ref Flags);
        serializer.SerializeValue(ref Progress);
    }
}
```

#### RPC Batching
**File**: `Core/Networking/RPCBatcher.cs`

**Purpose**: Batch multiple RPC calls to reduce overhead

**Usage**:
```csharp
// Instead of multiple RPCs
// Batch them into one
public void BatchUpdateStations(List<StationUpdate> updates);
```

### 4.4 Host Migration (Future)

**Note**: Not implemented in Phase 1, but architecture supports it

**Requirements**:
- Serialize entire game state
- Transfer state to new host
- Reconnect all clients
- Resume gameplay

**Files** (Future):
- `Core/Networking/HostMigration/GameStateSnapshot.cs`
- `Core/Networking/HostMigration/HostMigrationManager.cs`

## Phase 5: Testing & Validation

### 5.1 Network Testing Tools

#### Network Debugger
**File**: `Editor/NetworkDebugger.cs`

**Purpose**: Debug network issues in editor

**Features**:
- Visualize RPC calls
- Monitor NetworkVariable changes
- Simulate latency
- Track bandwidth usage
- Display connection status

#### Network Simulator
**File**: `Editor/NetworkSimulator.cs`

**Purpose**: Simulate network conditions

**Features**:
- Add artificial latency (50-200ms)
- Simulate packet loss (0-10%)
- Throttle bandwidth
- Test disconnections

### 5.2 Bot Integration

#### Network Bot Controller
**File**: `Core/Networking/Bot/NetworkBotController.cs`

**Purpose**: Network-aware bot players

**Responsibilities**:
- Simulate client behavior
- Send ServerRpc calls like real players
- Test multiplayer without multiple devices
- Stress test network systems

**Integration**:
- Extends existing `BotPlayer`
- Controlled by host
- Acts as simulated client

### 5.3 Unit Tests

#### Network Tests
**File**: `Tests/NetworkTests.cs`

**Test Cases**:
- Player spawning/despawning
- Ingredient pickup/drop
- Station interactions
- Order completion
- Score synchronization
- Disconnection handling

## Implementation Checklist

### Phase 1: Core Infrastructure
- [ ] `INetworkGameManager` interface
- [ ] `NetworkGameManager` implementation
- [ ] `IPlayerNetworkManager` interface
- [ ] `PlayerNetworkManager` implementation
- [ ] `INetworkObjectPool` interface
- [ ] `NetworkObjectPool` implementation
- [ ] Register services in `GameBootstrap`
- [ ] Test player spawning

### Phase 2: Gameplay Systems
- [ ] Enhance `IngredientItem` with validation
- [ ] Create `IngredientNetworkSpawner`
- [ ] Create `StationNetworkController`
- [ ] Enhance `CookingStation` base class
- [ ] Create `IDishValidator` interface
- [ ] Implement `StandardDishValidator`
- [ ] Create `PlateItem` class
- [ ] Enhance `AssemblyStation`
- [ ] Create `NetworkScoreManager`
- [ ] Enhance `ScoreManager`
- [ ] Test ingredient interactions
- [ ] Test station interactions
- [ ] Test dish assembly
- [ ] Test scoring

### Phase 3: Game Flow
- [ ] Create `NetworkGameStateManager`
- [ ] Create `RoundTimer`
- [ ] Enhance `OrderManager`
- [ ] Integrate with existing `GameStateManager`
- [ ] Test game flow (waiting → prep → playing → results)
- [ ] Test round timer synchronization
- [ ] Test order generation and completion

### Phase 4: P2P Optimization
- [ ] Create `ConnectionHandler`
- [ ] Implement `ClientPrediction`
- [ ] Implement `LagCompensation`
- [ ] Optimize `INetworkSerializable` implementations
- [ ] Implement RPC batching where beneficial
- [ ] Test with simulated latency
- [ ] Test disconnection handling

### Phase 5: Testing
- [ ] Create `NetworkDebugger` editor tool
- [ ] Create `NetworkSimulator` editor tool
- [ ] Enhance `NetworkBotController`
- [ ] Write unit tests
- [ ] Perform integration testing
- [ ] Stress test with 4 players + bots
- [ ] Profile network bandwidth usage

## Integration with Existing Systems

### ServiceContainer Integration
```csharp
// In GameBootstrap.cs
private void RegisterNetworkServices()
{
    // Core networking
    _services.RegisterNetworkGameManager(new NetworkGameManager());
    _services.RegisterPlayerNetworkManager(new PlayerNetworkManager());
    _services.RegisterNetworkObjectPool(new NetworkObjectPool());
    
    // Gameplay networking
    _services.RegisterNetworkScoreManager(FindObjectOfType<NetworkScoreManager>());
    _services.RegisterNetworkGameStateManager(FindObjectOfType<NetworkGameStateManager>());
}
```

### GameStateManager Integration
```csharp
// NetworkGameStateManager works with existing GameStateManager
public class NetworkGameStateManager : NetworkBehaviour
{
    private IGameStateManager _gameStateManager;
    
    private void Awake()
    {
        _gameStateManager = GameBootstrap.Services.StateManager;
    }
    
    [ServerRpc]
    public void ChangeStateServerRpc(string stateName)
    {
        // Validate and change state
        _gameStateManager.ChangeState(stateName);
        
        // Sync to clients
        ChangeStateClientRpc(stateName);
    }
}
```

### UI Integration
```csharp
// UI listens to network events
public class GameplayUIManager : MonoBehaviour
{
    private void Start()
    {
        var networkScoreManager = GameBootstrap.Services.NetworkScoreManager;
        networkScoreManager.OnScoreUpdated += UpdateScoreDisplay;
        
        var roundTimer = FindObjectOfType<RoundTimer>();
        roundTimer.OnTimeUpdated += UpdateTimerDisplay;
    }
}
```

## Performance Targets

### Network Performance
- **Tick Rate**: 30 Hz (Unity Netcode default)
- **Max Latency**: 200ms acceptable, 100ms optimal
- **Bandwidth**: < 50 KB/s per client
- **RPC Calls**: < 100 per second per client
- **NetworkVariable Updates**: < 50 per second total

### Optimization Strategies
1. **Use NetworkVariable for state**, not frequent RPCs
2. **Batch updates** where possible
3. **Use unreliable delivery** for non-critical updates
4. **Implement object pooling** for frequently spawned objects
5. **Compress data** with `INetworkSerializable`
6. **Throttle updates** for non-critical NetworkVariables

## Security Considerations

### Server Authority
- **All game logic on host** - clients only send requests
- **Validate all ServerRpc calls** - check sender permissions
- **Sanitize input data** - prevent invalid values
- **Rate limit RPCs** - prevent spam

### Anti-Cheat Measures
```csharp
// Example validation
[ServerRpc(RequireOwnership = false)]
public void RequestPickupServerRpc(ulong ingredientId, ServerRpcParams rpcParams = default)
{
    ulong senderId = rpcParams.Receive.SenderClientId;
    
    // Validate player exists
    if (!_playerManager.IsPlayerRegistered(senderId))
        return;
    
    // Validate distance
    PlayerController player = _playerManager.GetPlayer(senderId);
    if (Vector3.Distance(player.transform.position, ingredient.transform.position) > MAX_INTERACTION_DISTANCE)
        return;
    
    // Validate player can carry more
    if (player.IsHoldingObject())
        return;
    
    // Valid - proceed
    ingredient.PickUp(senderId);
}
```

## Debugging Tips

### Common Issues
1. **NetworkVariable not syncing**: Check if NetworkObject is spawned
2. **ServerRpc not executing**: Check `RequireOwnership` setting
3. **ClientRpc not received**: Check if called on server
4. **Objects not spawning**: Check if prefab is in NetworkManager prefab list
5. **Desync issues**: Ensure all state changes go through host

### Debug Logging
```csharp
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    GameLogger.Network.Log($"[NetworkGameManager] Spawning player {clientId}");
#endif
```

### Network Profiler
Use Unity's Network Profiler to monitor:
- RPC calls per frame
- NetworkVariable updates
- Bandwidth usage
- Message queue size

## Future Enhancements

### Post-Launch Features
1. **Host Migration**: Seamless host transfer on disconnect
2. **Spectator Mode**: Allow players to watch ongoing games
3. **Replay System**: Record and playback matches
4. **Cross-Platform Play**: Ensure PC/Mobile compatibility
5. **Dedicated Server Support**: Optional dedicated server mode

### Scalability
- Support for 6-8 players (requires testing)
- Multiple concurrent game sessions
- Tournament mode with brackets
- Ranked matchmaking integration

## Conclusion

This implementation plan provides a complete, SOLID-compliant network architecture for RecipeRage that:
- ✅ Works with P2P host-client model
- ✅ Integrates with existing EOS Transport
- ✅ Follows established architecture patterns
- ✅ Maintains service-based design
- ✅ Supports all game modes
- ✅ Optimizes for performance
- ✅ Provides debugging tools
- ✅ Scales for future features

All implementations will follow the existing code standards and SOLID principles established in the codebase.
