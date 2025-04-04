# Game State Management System

## Overview
The Game State Management System provides a structured approach to managing different states of the game, such as loading, main menu, matchmaking, and gameplay. It uses a state machine pattern to handle transitions between states and ensures that each state has clear entry and exit points.

## Components

### IState Interface
The base interface for all game states, defining the core methods and events:
- `Enter()`: Called when the state is entered
- `Update()`: Called every frame while the state is active
- `Exit()`: Called when the state is exited
- `CanTransitionTo(IState)`: Determines if a transition to another state is allowed

### IStateMachine Interface
Defines the contract for the state machine:
- `Initialize(IState)`: Sets up the state machine with an initial state
- `ChangeState(IState)`: Transitions to a new state if allowed
- `Update()`: Updates the current state

### StateMachine
Implementation of the IStateMachine interface that manages state transitions and updates.

### GameState
Abstract base class for all game states, implementing the IState interface and providing common functionality:
- Manages allowed transitions between states
- Handles state activation/deactivation
- Provides event notifications for state changes

### Specific Game States
- **LoadingState**: Handles asset loading and system initialization
- **MainMenuState**: Manages the main menu UI and navigation
- **MatchmakingState**: Handles player matchmaking and lobby management
- **GameplayState**: Controls the active gameplay session

### GameStateManager
Singleton manager that:
- Creates and initializes the state machine
- Provides methods to transition between states
- Handles state-specific events and transitions
- Coordinates between game states and other game systems

## State Flow
1. **Initial State**: The game starts in the LoadingState
2. **Main Menu**: After loading completes, transitions to MainMenuState
3. **Matchmaking**: When the player chooses to play, transitions to MatchmakingState
4. **Gameplay**: After successful matchmaking, transitions to GameplayState
5. **End Game**: When the game ends, returns to MainMenuState

## Usage Example
```csharp
// Get the GameStateManager instance
var stateManager = GameStateManager.Instance;

// Change to matchmaking state
stateManager.ChangeToMatchmakingState();

// Start matchmaking for a specific game mode and region
var matchmakingState = stateManager.CurrentState as MatchmakingState;
if (matchmakingState != null)
{
    matchmakingState.StartMatchmaking("ClassicMode", "US-East");
}
```

## Integration with Other Systems
- **UI System**: Each state manages its own UI elements
- **Game Mode System**: The GameplayState coordinates with the active GameModeBase
- **Network System**: The MatchmakingState interfaces with Epic Online Services for matchmaking
- **Loading System**: The LoadingState manages asset loading and initialization

## Future Enhancements
- Add more specialized states for tutorials, character selection, etc.
- Implement state persistence for handling application pausing/resuming
- Add analytics tracking for state transitions and time spent in each state
