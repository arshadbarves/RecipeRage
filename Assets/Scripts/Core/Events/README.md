# Recipe Rage Event System

## Overview
A high-performance event system designed for P2P multiplayer games, handling both local and networked events with optimized performance for mobile platforms.

## Core Components

### 1. GameEventSystem
Main event system manager handling both local and network events:
```csharp
// Get singleton instance
GameEventSystem eventSystem = GameEventSystem.Instance;

// Listen for events
eventSystem.AddListener<GameStartEvent>(listener);

// Raise events (local or network)
eventSystem.RaiseEvent(new GameStartEvent { PlayerCount = 4 });
```

### 2. Event Types
- Network Events: Game state, player actions, gameplay events
- Local Events: UI, input, audio, animations
- Support for immediate or queued execution

### 3. Event Targeting
```csharp
public enum EventTarget
{
    All,        // Send to all clients
    Server,     // Send to server only
    Owner,      // Send to object owner
    Others      // Send to all except sender
}
```

## Features

1. Network Optimization:
- Event targeting
- Network serialization
- Bandwidth optimization
- Server authority

2. Local Performance:
- Event queuing
- Immediate execution option
- Memory pooling
- Frame budgeting

3. Mobile Optimization:
- Memory efficient
- Battery friendly
- Low overhead
- Efficient serialization

## Integration Tests

Automated integration tests covering:
```csharp
[UnityTest]
public class EventSystemIntegrationTests
{
    - Local event delivery
    - Network event delivery
    - Event targeting
    - Event queueing
    - Event priority
}
```

## Usage Examples

### 1. Network Events
```csharp
// Game events
eventSystem.RaiseEvent(new GameStartEvent 
{ 
    PlayerCount = 4,
    RoundTime = 300f 
});

// Player events
eventSystem.RaiseEvent(new PlayerReadyEvent
{
    PlayerId = NetworkManager.LocalClientId,
    IsReady = true
});
```

### 2. Local Events
```csharp
// UI events (immediate)
eventSystem.RaiseEvent(new UIButtonClickEvent 
{ 
    ButtonId = "StartButton" 
});

// Animation events (queued)
eventSystem.RaiseEvent(new AnimationStartEvent
{
    AnimationId = "PlayerJump",
    Duration = 1.0f
});
```

## Best Practices

1. Event Design:
- Keep events small and focused
- Use appropriate targeting
- Validate event data
- Handle errors gracefully

2. Performance:
- Use queued events for non-critical updates
- Batch related events
- Clean up listeners
- Monitor event frequency

3. Network Usage:
- Validate server authority
- Handle disconnections
- Monitor bandwidth
- Use appropriate targets

## Mobile Considerations

1. Battery Life:
- Queue non-critical events
- Batch network messages
- Optimize serialization
- Minimize wake-ups

2. Memory Usage:
- Pool event objects
- Clean up listeners
- Monitor queue size
- Limit log entries

## Testing

1. Run Integration Tests:
```bash
# Via Unity Test Runner
Window > General > Test Runner > Run All
```

2. Test Coverage:
- Event delivery
- Network sync
- Error handling
- Performance
- Memory usage

## Notes
- Always validate server authority
- Clean up listeners on destroy
- Handle disconnections gracefully
- Test with multiple clients
- Monitor performance

_Last Updated: [Current Date]_
