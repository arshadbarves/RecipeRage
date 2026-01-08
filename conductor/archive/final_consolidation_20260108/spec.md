# Specification: Final Modular Consolidation

## Overview
This final track completes the reorganization of the codebase into a strict two-bucket system: **Modules** (Reusable, Agnostic) and **Gameplay** (Game-specific). It removes the remaining legacy top-level folders (`App`, `UI`) and streamlines `Core`.

## Final Folder Structure (Post-Migration)

### Modules (`Assets/Scripts/Modules/`)
- `Auth/`: Core authentication logic (Agnostic).
- `Banking/`: Generic banking interfaces and backends.
- `UI/`: UI foundation (Base classes, Transitions, Generic controls).
- `Session/`: Player session lifecycle.
- `Persistence/`: Save system.
- `Logging/`, `Audio/`, `Input/`, `RemoteConfig/`, etc.

### Gameplay (`Assets/Scripts/Gameplay/`)
- `App/`: Game-specific lifecycle (States, Startup networking).
- `UI/`: Game-specific screens, popups, and viewmodels.
- `Characters/`, `GameModes/`, `Interaction/`, `Cooking/`, `Stations/`, etc.

### Core (`Assets/Scripts/Core/`)
- `Bootstrap/`: The VContainer `LifetimeScope` roots and app entry configuration.
- `RecipeRage.Core.asmdef`: The main project glue.

## Namespace Policy
- All generic tools must be under `Modules.*`.
- All game mechanics and features must be under `Gameplay.*`.
- `Core.Bootstrap` remains the assembly entry point.
