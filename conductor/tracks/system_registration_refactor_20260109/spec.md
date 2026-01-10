# Specification: System Registration and Dependency Injection Refactor

## Overview
This track aims to formalize and complete the dependency injection (DI) setup across the RecipeRage project using VContainer. It involves registering all core and gameplay managers into their appropriate `LifetimeScope` (Pre-Auth, Auth Session, or Gameplay) to ensure a clean, decoupled architecture that adheres to the "Two-Bucket" assembly rules.

## Functional Requirements

### 1. Scope-Based Registration
Systems must be registered in the following scopes:

#### Pre-Auth / Project Scope (`GameLifetimeScope`)
- **Localization:** `ILocalizationManager` (LocalizationManager)
- **Auth:** `UGSAuthenticationManager` (and any associated interface)
- **Networking Core:** `IPlayerNetworkManager`
- **Audio:** (Already implemented)

#### Auth Session Scope (`SessionLifetimeScope`)
- **Player Data:** `IPlayerManager`
- **Lobby/Matchmaking:** `ILobbyManager`
- **Social:** `IFriendsManager` (if applicable)
- **Teams:** `ITeamManager`
- **UI Navigation:** `IUIScreenStackManager`

#### Gameplay Scope (`GameplayLifetimeScope`)
- **State:** `IGameStateManager`
- **Scoring:** `IScoreManager`
- **Orders:** `IOrderManager`
- **Spawning:** `ISpawnManager`
- **AI/Bots:** `IBotManager`
- **UI:** `IGameplayUIManager`

### 2. Registration Patterns
- **Interface-First:** All registrations MUST use `As<IInterface>()` to enforce decoupling.
- **Settings Injection:** Managers requiring configuration must have their corresponding `ScriptableObject` settings injected (loaded from `Resources/`).
- **Mixed Instantiation:**
    - Use `builder.Register<T>(Lifetime.Singleton)` for pure C# managers.
    - Use `builder.RegisterComponentInNewPrefab(prefab)` or `builder.RegisterComponentInHierarchy(instance)` for MonoBehaviour-based managers.

### 3. Assembly Adherence
- Ensure `Core` assembly managers are registered in `GameLifetimeScope` or `SessionLifetimeScope`.
- Ensure `Gameplay` assembly managers are registered in `GameplayLifetimeScope`.

## Non-Functional Requirements
- **Maintainability:** Clear separation between "What" (Interfaces) and "How" (Implementations).
- **Testability:** Decoupled classes should be easily mockable in future unit tests.

## Acceptance Criteria
- [ ] The project compiles successfully with all new registrations.
- [ ] VContainer diagnostic window (if available) shows no resolution errors.
- [ ] Core systems (Auth, Localization) are accessible via DI in the Main Menu.
- [ ] Gameplay systems (Score, Orders) are accessible via DI in the Gameplay scene.

## Out of Scope
- Implementing the logic for the managers themselves (only registration is covered).
- Migrating existing non-DI code to DI (this track focuses on registration infrastructure).
