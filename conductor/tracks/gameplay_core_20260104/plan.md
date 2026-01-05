# Plan: Gameplay Mechanics & Architecture

This plan covers the implementation of the core gameplay loop, character ability system, and advanced multiplayer systems (fairness, bots, session recovery) for RecipeRage.

## Phase 1: Architecture & Foundation
Establish the core data structures and dependency injection setup.
- [ ] Task: Define `CharacterData` and `AbilityData` ScriptableObjects
- [ ] Task: Implement `GameplayLifeScope` for VContainer dependency injection
- [ ] Task: Create `StationBase` and `ItemBase` abstract classes for extensible gameplay objects
- [ ] Task: Conductor - User Manual Verification 'Architecture & Foundation' (Protocol in workflow.md)

## Phase 2: Core Gameplay Interactions
Implement the physical interactions and basic kitchen stations.
- [ ] Task: Implement `PlayerController` with movement and context-sensitive interaction logic
- [ ] Task: Create `IngredientCrate` and `EmptyCrate` stations
- [ ] Task: Create `CuttingBoard` with progress-based interaction
- [ ] Task: Create `Stove` with time-based cooking logic
- [ ] Task: Implement `DeliveryBelt` for order fulfillment
- [ ] Task: Conductor - User Manual Verification 'Core Gameplay Interactions' (Protocol in workflow.md)

## Phase 3: Character Abilities & Perks
Add the unique character-specific mechanics.
- [ ] Task: Implement Character-specific stat overrides (Speed, Action Speed)
- [ ] Task: Create `AbilitySystem` for cooldown management and ability execution
- [ ] Task: Implement "Dash" as a reference active ability
- [ ] Task: Conductor - User Manual Verification 'Character Abilities & Perks' (Protocol in workflow.md)

## Phase 4: Networking, Prediction & Fairness
Ensure a responsive and fair P2P experience.
- [ ] Task: Refactor `PlayerController` for NGO Server-Authoritative movement
- [ ] Task: Implement Client-Side Prediction for movement
- [ ] Task: Implement Client-Side Prediction for Item Pick-up/Drop feedback
- [ ] Task: Implement Server Reconciliation to handle latency conflicts
- [ ] Task: Conductor - User Manual Verification 'Networking, Prediction & Fairness' (Protocol in workflow.md)

## Phase 5: AI/Bot System
Develop the hybrid RL-based bot system for filling teams.
- [ ] Task: Implement `BotController` using A* for basic navigation
- [ ] Task: Implement high-level task prioritization logic (The "RL" decision layer)
- [ ] Task: Integrate dynamic bot spawning/removal in Matchmaking/Lobby logic
- [ ] Task: Conductor - User Manual Verification 'AI/Bot System' (Protocol in workflow.md)

## Phase 6: Game Mode & Session Management
Finalize the 3v3 competitive loop and session recovery.
- [ ] Task: Implement `GameModeManager` for Timed Match and Sudden Death logic
- [ ] Task: Implement `OrderSystem` for random recipe generation and scoring
- [ ] Task: Implement Host Migration logic using EOS
- [ ] Task: Implement Player Rejoin logic (Replacing bot with reconnected player)
- [ ] Task: Conductor - User Manual Verification 'Game Mode & Session Management' (Protocol in workflow.md)
