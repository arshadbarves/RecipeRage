# Specification: Service Migration & Gameplay Separation

## Overview
Reorganize the `Assets/Scripts` directory to enforce a strict modular architecture. Services currently residing in `Core` will be moved to `Core`, grouped by functionality. Gameplay-specific logic will be moved from `Core` to `Gameplay`. `Core` will be reduced to essential bootstrapping and low-level infrastructure.

## Target Structure

### Core (`Assets/Scripts/Core/`)
Reusable, project-agnostic(ish) systems.
- `Audio/`: `AudioService`, `MusicPlayer`, etc.
- `Input/`: `InputService`, `InputProvider`.
- `Persistence/`: `SaveService`, `StorageProviders`.
- `Networking/`: `NetworkManager`, `ConnectivityService`.
- `Animation/`: `AnimationService`, `DOTween` wrappers.
- `Logging/`: `GameLogger`.
- `Localization/`: `LocalizationManager`.
- `RemoteConfig/`: `RemoteConfigService`.
- `Shared/`: Common interfaces (`IInitializable`) and events.
- `Core/`: *Renamed to avoid confusion?* Maybe keep `Banking` here or move to `Core/Economy`.

### Gameplay (`Assets/Scripts/Gameplay/`)
Game-specific logic and mechanics.
- `Characters/`: `PlayerController`, `CharacterService`.
- `GameModes/`: `GameModeService`, `MapLoader`.
- `Camera/`: Camera controllers.
- `Interaction/`: Interaction logic.
- `Cooking/`: *Existing*
- `Stations/`: *Existing*

### Core (`Assets/Scripts/Core/`)
Bootstrapping and Glue.
- `Bootstrap/`: `SessionLifetimeScope`, `AppLifetimeScope`.
- `Configuration/`: App config.
- `Backends/`: Concrete implementations that might link Core to specific infra (if strictly necessary to keep separate, otherwise move to Core).

## Namespace Convention
- `Core.<ModuleName>` (e.g., `Core.Audio`, `Core.Persistence`).
- `Gameplay.<Feature>` (e.g., `Gameplay.Characters`).
- `Core.Bootstrap`.

## Dependency Flow
`App` -> `Gameplay` -> `Core` -> `Core` (Bootstrap/Glue)? 
Actually:
`App` (Entry) -> `Core` (DI Root) -> `Gameplay` & `Core`.
`Gameplay` -> `Core`.
`Core` -> `Core` (Shared).

## Assembly Definitions
- `RecipeRage.Core`: Contains all `Core/*`. No dependency on `Gameplay` or `Core` (except maybe Bootstrap interfaces if not in Shared).
- `RecipeRage.Gameplay`: Depends on `Core`.
- `RecipeRage.Core`: Depends on `Core` (to register services) and `Gameplay` (to start game).
- `RecipeRage.App`: Entry point.
