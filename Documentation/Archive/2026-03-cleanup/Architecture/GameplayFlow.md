# RecipeRage Gameplay Code Flow

## Architecture Overview

This document illustrates the flow and architecture of the gameplay systems in RecipeRage.

## System Architecture

```mermaid
graph TB
    subgraph "Game State Management"
        GS[GameStateManager]
        NS[NetworkGameStateManager]
        GS --> NS
    end

    subgraph "Core Gameplay Managers"
        OM[OrderManager]
        SM[ScoreManager]
        RT[RoundTimer]
        GLS[GameplayLifetimeScope]
        GLS --> OM
        GLS --> SM
        GLS --> RT
    end

    subgraph "Player System"
        PC[PlayerController]
        PSC[PlayerStateController]
        PMC[PlayerMovementController]
        PIH[PlayerInputHandler]
        PNC[PlayerNetworkController]
        PIC[PlayerInteractionController]
        
        PC --> PSC
        PC --> PMC
        PC --> PIH
        PC --> PNC
        PC --> PIC
    end

    subgraph "Station System"
        SB[StationBase]
        CS[CookingStation]
        CTS[CuttingStation]
        CPS[CounterStation]
        SS[ServingStation]
        IC[IngredientCrate]
        TB[TrashBin]
        
        SB --> CS
        SB --> CTS
        SB --> CPS
        SB --> SS
        SB --> IC
        SB --> TB
    end

    subgraph "Cooking System"
        R[Recipe]
        O[Order]
        II[IngredientItem]
        PI[PlateItem]
        SNC[StationNetworkController]
        
        O --> R
        II --> R
        PI --> II
    end

    GS --> GLS
    NS --> RT
    PC --> SB
    PC --> II
    SB --> II
    SB --> PI
    SM --> OM
    SS --> OM
    SS --> SM
```

## Game Flow Sequence

```mermaid
sequenceDiagram
    participant Host
    participant NetworkGameManager
    participant RoundStarter
    participant OrderManager
    participant ScoreManager
    participant Player

    Note over Host,Player: Game Initialization
    Host->>NetworkGameManager: RequestStartGameServerRpc()
    NetworkGameManager->>NetworkGameManager: StartPreparationPhase(10s)
    NetworkGameManager->>NetworkGameManager: Show countdown on all clients
    
    Note over Host,Player: Preparation Phase
    loop Every frame
        NetworkGameManager->>NetworkGameManager: Update phase timer
    end
    
    Note over Host,Player: Preparation Complete
    NetworkGameManager->>NetworkGameManager: Transition to Playing Phase
    NetworkGameManager->>RoundStarter: StartRoundTimer()
    
    Note over Host,Player: Gameplay Loop
    loop Order Generation
        OrderManager->>OrderManager: GenerateNewOrder()
        OrderManager->>OrderManager: Add to active orders
    end
    
    loop Player Actions
        Player->>Player: Move & Interact with stations
        Player->>StationBase: Interact()
        StationBase->>StationBase: HandleInteraction()
        StationBase->>IngredientItem: Process ingredient
    end
    
    loop Order Completion
        Player->>ServingStation: Serve dish
        ServingStation->>OrderManager: CompleteOrder(orderId)
        ServingStation->>ScoreManager: AddScoreServerRpc(points, reason)
        ScoreManager->>ScoreManager: Calculate score with bonuses
        ScoreManager->>OrderManager: Remove completed order
    end
    
    Note over Host,Player: Round End
    NetworkGameManager->>NetworkGameManager: TimeRemaining <= 0
    NetworkGameManager->>NetworkGameManager: EndGame()
    NetworkGameManager->>OrderManager: StopGeneratingOrders()
```

## Player Controller Flow

```mermaid
flowchart TD
    A[Player Input] --> B{Input Type}
    
    B -->|Movement| C[PlayerInputHandler]
    C --> D[Smooth Input]
    D --> E[PlayerStateController]
    E --> F[PlayerMovementController]
    F --> G[Apply Movement to Rigidbody]
    
    B -->|Interaction| H[PlayerInteractionController]
    H --> I[Find Nearby Station]
    I --> J{Station Available?}
    J -->|Yes| K[StationBase.Interact]
    J -->|No| L[Ignore]
    
    B -->|Ability| M[Character Ability]
    M --> N[Execute Ability Logic]
    
    G --> O[Client Prediction]
    O --> P[SendInputToServerRpc]
    P --> Q[Server Validates]
    Q --> R[ReconcileStateClientRpc]
    R --> S[Update Client State]
    
    K --> T[HandleInteraction]
    T --> U{Pickup/Drop?}
    U -->|Pickup| V[PickUpObject]
    U -->|Drop| W[DropObject]
    U -->|Process| X[Station Processing]
    
    X --> Y{Station Type}
    Y -->|Cooking| Z[CookingStation]
    Y -->|Cutting| AA[CuttingStation]
    Y -->|Serving| AB[ServingStation]
```

## Station Interaction Flow

```mermaid
flowchart TD
    A[Player Approaches Station] --> B[Within Interaction Range?]
    B -->|No| C[Cannot Interact]
    B -->|Yes| D[Player Presses Interact]
    
    D --> E[PlayerInteractionController.TryInteract]
    E --> F{Is Server?}
    F -->|No| G[Send InteractServerRpc]
    F -->|Yes| H[Check Station Lock]
    
    G --> H
    H --> I{Station Locked?}
    I -->|Yes| J[Reject Interaction]
    I -->|No| K[Lock Station for Player]
    
    K --> L[StationBase.HandleInteraction]
    L --> M{Station Type}
    
    M -->|Ingredient Crate| N[Spawn Ingredient]
    M -->|Counter| O[Pickup/Drop Item]
    M -->|Processing Station| P{Has Ingredient?}
    
    P -->|No| Q[Pickup Ingredient]
    P -->|Yes| R{Already Processing?}
    
    R -->|Yes| S[Take Processed Item]
    R -->|No| T[Start Processing]
    
    T --> U[Set Processing Timer]
    U --> V[Update Progress]
    V --> W{Processing Complete?}
    W -->|No| U
    W -->|Yes| X[Mark Processed]
    
    M -->|Serving Station| Y{Has Item?}
    Y -->|Yes| Z[Validate Dish]
    Y -->|No| C
    
    Z --> AA[Match with Active Order]
    AA --> AB{Valid Order?}
    AB -->|Yes| AC[Add Score + Complete Order]
    AB -->|No| AD[Reject Dish]
    
    K --> AE[Unlock Station]
    AE --> AF[Interaction Complete]
```

## Scoring System Flow

```mermaid
flowchart TD
    A[Order Completed] --> B[OrderManager.OnOrderCompleted]
    B --> C[ScoreManager.HandleOrderCompleted]
    
    C --> D{Is Server?}
    D -->|No| E[Return]
    D -->|Yes| F[Get Base Points]
    
    F --> G[Calculate Time Bonus]
    G --> H{Time Remaining?}
    H -->|Yes| I[Bonus = TimeBonus × Time%]
    H -->|No| J[Bonus = 0]
    
    I --> K[Check Combo]
    J --> K
    
    K --> L{TimeSinceLastOrder <= ComboWindow?}
    L -->|Yes| M[Increment Combo Count]
    L -->|No| N[Reset Combo = 1]
    
    M --> O[Calculate Combo Bonus]
    N --> O
    
    O --> P[Combo Bonus = Combo × ComboMultiplier]
    P --> Q[Total Points = Base + Time + Combo]
    
    Q --> R[Update NetworkVariable Score]
    R --> S[OnValueChanged Event]
    S --> T[UI Updates]
    
    C --> U[ServingStation.AddScoreServerRpc]
    U --> V{Dish Quality}
    V -->|Perfect| W[Add Perfect Bonus]
    V -->|Good| X[Standard Scoring]
    V -->|Poor| Y[Reduced Points]
    
    W --> Z[Add Score to Player]
    X --> Z
    Y --> Z
```

## Phase Management Flow

```mermaid
flowchart TD
    A[Game Starts] --> B[NetworkGameStateManager]
    B --> C[Waiting Phase]
    
    C --> D[Host Requests Start]
    D --> E[StartPreparationPhase 10s]
    E --> F[Show Countdown UI]
    
    F --> G{Countdown Complete?}
    G -->|No| H[Update Timer]
    H --> G
    G -->|Yes| I[StartPlayingPhase 180s]
    
    I --> J[Start Round Timer]
    J --> K[OrderManager.StartGeneratingOrders]
    
    K --> L[Gameplay Active]
    
    L --> M{Round Timer Complete?}
    M -->|No| N[Continue Orders & Scoring]
    N --> M
    M -->|Yes| O[EndGame]
    
    O --> P[Stop Order Generation]
    P --> Q[Show Results]
    Q --> R[Return to Waiting]
    
    R --> S{Play Again?}
    S -->|Yes| D
    S -->|No| T[Exit to Menu]
```

## Key Components Reference

### PlayerController (Assets/Scripts/Gameplay/Characters/PlayerController.cs:24)
- Orchestrates all player subsystems
- Manages movement, input, interaction, and network state
- Handles character abilities and object carrying

### NetworkGameStateManager (Assets/Scripts/Gameplay/App/State/NetworkGameStateManager.cs:15)
- Synchronizes game phases across network
- Controls preparation countdown and game duration
- Manages phase transitions (Waiting → Preparation → Playing → Results)

### OrderManager (Assets/Scripts/Gameplay/Cooking/OrderManager.cs:13)
- Generates recipe orders on server
- Tracks active orders and their time limits
- Handles order completion and expiration

### ScoreManager (Assets/Scripts/Gameplay/Scoring/ScoreManager.cs:12)
- Calculates scores for completed orders
- Manages combos and time bonuses
- Synchronizes score across network

### StationBase (Assets/Scripts/Gameplay/Stations/StationBase.cs:13)
- Base class for all gameplay stations
- Handles network locking for station access
- Defines interaction interface

### ServingStation (Assets/Scripts/Gameplay/Stations/ServingStation.cs:15)
- Validates completed dishes against orders
- Awards points for successful orders
- Integrates with ScoreManager for scoring

### CookingStation (Assets/Scripts/Gameplay/Stations/CookingStation.cs:10)
- Processes cooking operations on ingredients
- Handles burning mechanics
- Shows visual/audio feedback during cooking

## Data Flow Summary

1. **Initialization**: GameStateManager → NetworkGameStateManager → RoundStarter → OrderManager/ScoreManager
2. **Game Loop**: NetworkGameStateManager (phases) → OrderManager (orders) → Player actions → Station interactions → Score updates
3. **Network Sync**: Server RPCs for actions → Client RPCs for state updates → NetworkVariables for synchronized state
4. **Completion**: ServingStation → Order validation → Score calculation → Order removal

## Concurrency & Safety

- All state-changing operations happen on Server via ServerRpc
- NetworkVariables automatically synchronize state to clients
- Station locking prevents concurrent access conflicts
- Client prediction for movement with server reconciliation
