# Specification: Service Migration & Gameplay Separation

## Overview
Reorganize the `Assets/Scripts` directory to enforce a strict modular architecture. Services currently residing in `Core` will be moved to `Modules`, grouped by functionality. Gameplay-specific logic will be moved from `Core` to `Gameplay`. `Core` will be reduced to essential bootstrapping and low-level infrastructure.

## Target Structure

### Modules (`Assets/Scripts/Modules/`)
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
- `Core/`: *Renamed to avoid confusion?* Maybe keep `Banking` here or move to `Modules/Economy`.

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
- `Backends/`: Concrete implementations that might link Modules to specific infra (if strictly necessary to keep separate, otherwise move to Modules).

## Namespace Convention
- `Modules.<ModuleName>` (e.g., `Modules.Audio`, `Modules.Persistence`).
- `Gameplay.<Feature>` (e.g., `Gameplay.Characters`).
- `Core.Bootstrap`.

## Dependency Flow
`App` -> `Gameplay` -> `Modules` -> `Core` (Bootstrap/Glue)? 
Actually:
`App` (Entry) -> `Core` (DI Root) -> `Gameplay` & `Modules`.
`Gameplay` -> `Modules`.
`Modules` -> `Modules` (Shared).

## Assembly Definitions
- `RecipeRage.Modules`: Contains all `Modules/*`. No dependency on `Gameplay` or `Core` (except maybe Bootstrap interfaces if not in Shared).
- `RecipeRage.Gameplay`: Depends on `Modules`.
- `RecipeRage.Core`: Depends on `Modules` (to register services) and `Gameplay` (to start game).
- `RecipeRage.App`: Entry point.
