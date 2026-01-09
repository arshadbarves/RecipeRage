# Implementation Plan - Service Migration & Gameplay Separation

## Phase 1: Preparation & Structure [checkpoint: e1ba0b2]
- [x] e6bfe35 Task: Create Module Directory Structure
    - [ ] Create `Assets/Scripts/Core/Audio/`
    - [ ] Create `Assets/Scripts/Core/Input/`
    - [ ] Create `Assets/Scripts/Core/Persistence/` (for SaveSystem)
    - [ ] Create `Assets/Scripts/Core/Networking/` (Expand existing)
    - [ ] Create `Assets/Scripts/Core/Animation/`
    - [ ] Create `Assets/Scripts/Core/Localization/`
    - [ ] Create `Assets/Scripts/Core/RemoteConfig/`
    - [ ] Create `Assets/Scripts/Core/Logging/`
    - [ ] Create `Assets/Scripts/Core/Analytics/` (if Stats exist)
- [x] Task: Conductor - User Manual Verification 'Phase 1: Preparation & Structure'

## Phase 2: Service Migration (Core -> Core) [checkpoint: 416fc48]
- [x] e41670f Task: Migrate Logging Service
- [x] 85711e6 Task: Migrate Save System
- [x] 8d6739b Task: Migrate Audio Service
- [x] a333f92 Task: Migrate Input Service
- [x] 533c15d Task: Migrate Animation Service
- [x] 3bd556c Task: Migrate Localization
- [x] be726b6 Task: Migrate RemoteConfig
- [x] 6c5dea1 Task: Migrate Networking (Core portions)
- [x] 8c18a32 Task: Migrate Remaining Core Services
    - [x] Move `Core/Update` & `Core/Maintenance` to `Core/RemoteConfig`.
    - [x] Move `Core/Reactive` to `Core/Shared`.
    - [x] Move `Core/Extensions` to `Core/Shared/Extensions`.
    - [x] Move `Core/Events/EventBus.cs` to `Core/Shared/Events`.
    - [x] Move `Core/Skins` to `Core/Skins`.
    - [x] Move `Core/SDK` to `Core/Auth` or `Core/Core/Backends`.
- [x] Task: Conductor - User Manual Verification 'Phase 2: Service Migration'

## Phase 3: Gameplay Separation (Core -> Gameplay) [checkpoint: f0670b0]
- [x] 64df672 Task: Move Character Logic
- [x] 77ecd82 Task: Move Game Modes
- [x] 94b897e Task: Move Camera Logic
- [x] 6418fab Task: Move Interaction Logic
- [x] f0670b0 Task: Move Stats/Enums/Utils
- [x] Task: Conductor - User Manual Verification 'Phase 3: Gameplay Separation'

## Phase 4: Final Cleanup & Assembly Check [checkpoint: 20e7ac6]
- [x] 20e7ac6 Task: Update Assembly Definitions
    - [x] Ensure `RecipeRage.Core` includes all new folders.
    - [x] Ensure `RecipeRage.Gameplay` includes all new folders.
    - [x] Clean up `RecipeRage.Core` (should be minimal: Bootstrap, Backends?).
- [x] Task: Verify Compilation
    - [x] Fix any lingering namespace issues.
- [x] Task: Conductor - User Manual Verification 'Phase 4: Final Cleanup'
