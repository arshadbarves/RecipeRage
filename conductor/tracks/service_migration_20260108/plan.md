# Implementation Plan - Service Migration & Gameplay Separation

## Phase 1: Preparation & Structure [checkpoint: e1ba0b2]
- [x] e6bfe35 Task: Create Module Directory Structure
    - [ ] Create `Assets/Scripts/Modules/Audio/`
    - [ ] Create `Assets/Scripts/Modules/Input/`
    - [ ] Create `Assets/Scripts/Modules/Persistence/` (for SaveSystem)
    - [ ] Create `Assets/Scripts/Modules/Networking/` (Expand existing)
    - [ ] Create `Assets/Scripts/Modules/Animation/`
    - [ ] Create `Assets/Scripts/Modules/Localization/`
    - [ ] Create `Assets/Scripts/Modules/RemoteConfig/`
    - [ ] Create `Assets/Scripts/Modules/Logging/`
    - [ ] Create `Assets/Scripts/Modules/Analytics/` (if Stats exist)
- [x] Task: Conductor - User Manual Verification 'Phase 1: Preparation & Structure'

## Phase 2: Service Migration (Core -> Modules)
- [x] e41670f Task: Migrate Logging Service
    - [x] Move `Core/Logging` to `Modules/Logging`.
    - [x] Update namespaces (`Core.Logging` -> `Modules.Logging`).
    - [x] Fix references in `Core` and `Modules` (using IDE tools or grep).
    - [x] *Note: GameLogger is widely used, this will be a big change.*
- [x] 85711e6 Task: Migrate Save System
    - [x] Move `Core/SaveSystem` to `Modules/Persistence`.
    - [x] Update namespaces.
    - [x] Fix references (especially in `Backends`).
- [x] 8d6739b Task: Migrate Audio Service
    - [x] Move `Core/Audio` to `Modules/Audio`.
    - [x] Update namespaces.
- [x] a333f92 Task: Migrate Input Service
    - [x] Move `Core/Input` to `Modules/Input`.
    - [x] Update namespaces.
- [x] 533c15d Task: Migrate Animation Service
    - [x] Move `Core/Animation` to `Modules/Animation`.
    - [x] Update namespaces.
- [x] 3bd556c Task: Migrate Localization
    - [x] Move `Core/Localization` to `Modules/Localization`.
    - [x] Update namespaces.
- [x] be726b6 Task: Migrate RemoteConfig
    - [x] Move `Core/RemoteConfig` to `Modules/RemoteConfig`.
    - [x] Update namespaces.
- [x] 6c5dea1 Task: Migrate Networking (Core portions)
    - [x] Move `Core/Networking` to `Modules/Networking`.
    - [x] Update namespaces.
- [ ] Task: Migrate Remaining Core Services
    - [ ] Move `Core/Update` & `Core/Maintenance` to `Modules/RemoteConfig`.
    - [ ] Move `Core/Reactive` to `Modules/Shared`.
    - [ ] Move `Core/Extensions` to `Modules/Shared/Extensions`.
    - [ ] Move `Core/Events/EventBus.cs` to `Modules/Shared/Events`.
    - [ ] Move `Core/Skins` to `Modules/Skins`.
    - [ ] Move `Core/SDK` to `Modules/Auth` or `Modules/Core/Backends`.
- [x] Task: Conductor - User Manual Verification 'Phase 2: Service Migration' [checkpoint: 416fc48]

## Phase 3: Gameplay Separation (Core -> Gameplay) [checkpoint: f0670b0]
- [x] 64df672 Task: Move Character Logic
    - [x] Move `Core/Characters` to `Assets/Scripts/Gameplay/Characters`.
    - [x] Update namespaces (`Core.Characters` -> `Gameplay.Characters`).
- [x] 77ecd82 Task: Move Game Modes
    - [x] Move `Core/GameModes` to `Assets/Scripts/Gameplay/GameModes`.
    - [x] Update namespaces.
- [x] 94b897e Task: Move Camera Logic
    - [x] Move `Core/Camera` to `Assets/Scripts/Gameplay/Camera` (Merge if exists).
    - [x] Update namespaces.
- [x] 6418fab Task: Move Interaction Logic
    - [x] Move `Core/Interaction` to `Assets/Scripts/Gameplay/Interaction`.
    - [x] Update namespaces.
- [x] f0670b0 Task: Move Stats/Enums/Utils
    - [x] Analyze `Core/Stats`, `Core/Enums`, `Core/Utilities`.
    - [x] Move to `Modules/Shared` or `Gameplay/Shared` depending on usage.
- [x] Task: Conductor - User Manual Verification 'Phase 3: Gameplay Separation'

## Phase 4: Final Cleanup & Assembly Check
- [ ] Task: Update Assembly Definitions
    - [ ] Ensure `RecipeRage.Modules` includes all new folders.
    - [ ] Ensure `RecipeRage.Gameplay` includes all new folders.
    - [ ] Clean up `RecipeRage.Core` (should be minimal: Bootstrap, Backends?).
- [ ] Task: Verify Compilation
    - [ ] Fix any lingering namespace issues.
- [ ] Task: Conductor - User Manual Verification 'Phase 4: Final Cleanup'
