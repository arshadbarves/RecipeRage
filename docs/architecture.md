# RecipeRage Architecture Document

## Table of Contents
1. [System Overview](#system-overview)
2. [Core Components](#core-components)
3. [Game Framework](#game-framework)
4. [Multiplayer Services](#multiplayer-services)
5. [Gameplay Systems](#gameplay-systems)
6. [UI Framework](#ui-framework)
7. [Data Management](#data-management)
8. [Dependencies and Integrations](#dependencies-and-integrations)

## System Overview

RecipeRage is a mobile multiplayer cooking game built with Unity, using Epic Online Services (EOS) for networking. The architecture follows a modular, component-based design with clear separation of concerns.

### High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                       RecipeRage Architecture                    │
└─────────────────────────────────────────────────────────────────┘
                                  │
          ┌─────────────────────┬┴┬─────────────────────┐
          │                     │ │                     │
┌─────────▼─────────┐ ┌─────────▼─▼─────────┐ ┌─────────▼─────────┐
│   Game Framework  │ │ Multiplayer Services│ │  Gameplay Systems │
└─────────┬─────────┘ └─────────┬─┬─────────┘ └─────────┬─────────┘
          │                     │ │                     │
┌─────────▼─────────┐ ┌─────────▼─▼─────────┐ ┌─────────▼─────────┐
│   Game State      │ │     EOS Integration  │ │    Task System    │
│   Player Controller│ │     Lobby System    │ │    Object System  │
│   Game Loop       │ │     Matchmaking     │ │    Time System    │
└─────────┬─────────┘ └─────────┬─┬─────────┘ └─────────┬─────────┘
          │                     │ │                     │
          └─────────────────────┘ └─────────────────────┘
                                  │
                      ┌───────────▼───────────┐
                      │    Cross-Cutting      │
                      │      Concerns         │
                      └───────────┬───────────┘
                                  │
          ┌─────────────────────┬┴┬─────────────────────┐
          │                     │ │                     │
┌─────────▼─────────┐ ┌─────────▼─▼─────────┐ ┌─────────▼─────────┐
│   UI Framework    │ │  Monetization       │ │  Data Persistence │
│   Shop System     │ │  Ad Integration     │ │  Analytics        │
│   Inventory       │ │  IAP System         │ │  Security         │
└───────────────────┘ └───────────────────┘ └───────────────────┘
```

## Core Components

### GameBootstrap
- **Purpose**: Central initialization point for the entire game
- **Responsibilities**:
  - Initialize core systems (logging, authentication, EOS)
  - Manage application lifecycle
  - Coordinate startup sequence

### ServiceLocator
- **Purpose**: Dependency injection and service management
- **Responsibilities**:
  - Register and provide access to game services
  - Manage service lifecycle
  - Decouple service implementations from consumers

### EventSystem
- **Purpose**: Communication between decoupled components
- **Responsibilities**:
  - Event registration and dispatching
  - Type-safe event handling
  - Cross-system communication

## Game Framework

### GameState Management
- **Purpose**: Manage the overall state of the game
- **Components**:
  - GameStateManager: Controls state transitions
  - IGameState: Interface for all game states
  - Concrete states: MainMenu, Lobby, InGame, GameOver, etc.

### Player Controller
- **Purpose**: Handle player input and movement
- **Components**:
  - InputManager: Process and map input
  - PlayerController: Manage player movement and actions
  - PlayerState: Track player data and status

### Game Loop
- **Purpose**: Manage the core gameplay loop
- **Components**:
  - GameModeBase: Base class for all game modes
  - GameMode implementations: ClassicMode, TimeAttackMode, TeamBattleMode
  - Round management and scoring

## Multiplayer Services

### EOS Integration
- **Purpose**: Connect to Epic Online Services
- **Components**:
  - EOSManager: Initialize and manage EOS SDK
  - AuthManager: Handle player authentication
  - SessionManager: Manage game sessions

### Lobby System
- **Purpose**: Allow players to gather before a game
- **Components**:
  - LobbyManager: Create and manage lobbies
  - LobbyUI: User interface for lobby interactions
  - PlayerSlot: Manage player status in lobby

### Matchmaking
- **Purpose**: Find and connect players for games
- **Components**:
  - MatchmakingService: Handle matchmaking requests
  - MatchmakingRules: Define matching criteria
  - SessionJoiner: Connect players to sessions

## Gameplay Systems

### Task System
- **Purpose**: Manage recipes and orders
- **Components**:
  - RecipeManager: Track available recipes
  - OrderGenerator: Create and manage customer orders
  - ScoreCalculator: Calculate points based on order completion

### Object System
- **Purpose**: Manage interactive game objects
- **Components**:
  - InteractableBase: Base class for interactive objects
  - CookingStation: Handle food preparation
  - Container: Store ingredients and dishes
  - PickupableItem: Items that can be carried

### Time System
- **Purpose**: Manage game timing and events
- **Components**:
  - GameTimer: Track overall game time
  - OrderTimer: Manage individual order timers
  - TimedEvents: Schedule and execute timed events

## UI Framework

### Screen Management
- **Purpose**: Manage UI screens and navigation
- **Components**:
  - UIScreenManager: Control screen transitions
  - ScreenBase: Base class for all screens
  - ScreenTransition: Handle screen animations

### HUD System
- **Purpose**: Display in-game information
- **Components**:
  - HUDManager: Manage HUD elements
  - OrderDisplay: Show current orders
  - TimerDisplay: Show game and order timers
  - ScoreDisplay: Show player scores

### Input Visualization
- **Purpose**: Display touch controls on mobile
- **Components**:
  - TouchInputVisualizer: Show touch areas
  - JoystickVisualizer: Display virtual joystick
  - ButtonVisualizer: Show action buttons

## Data Management

### ScriptableObject Data
- **Purpose**: Store game configuration data
- **Components**:
  - RecipeData: Define recipes and ingredients
  - CharacterData: Define character stats and abilities
  - GameModeData: Configure game mode parameters

### Save System
- **Purpose**: Persist player progress and settings
- **Components**:
  - SaveManager: Handle save/load operations
  - PlayerProgressData: Store player achievements and unlocks
  - PlayerPreferencesData: Store player settings

### Analytics
- **Purpose**: Track game usage and performance
- **Components**:
  - AnalyticsManager: Collect and send analytics
  - EventTracker: Track specific game events
  - PerformanceMonitor: Track game performance

## Dependencies and Integrations

### Epic Online Services
- Authentication
- Matchmaking
- Lobbies
- P2P Networking
- Cloud Storage

### Unity Services
- Analytics
- Remote Config
- Cloud Save

### Third-Party SDKs
- Advertisement SDKs
- IAP Integration
- Social Media Integration

## Implementation Guidelines

1. **Modularity**: Each system should be self-contained with clear interfaces
2. **Testability**: Systems should be designed for unit testing
3. **Scalability**: Architecture should support adding new features
4. **Performance**: Mobile-first optimization for all systems
5. **Maintainability**: Clear documentation and consistent patterns
