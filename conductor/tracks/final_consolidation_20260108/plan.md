# Implementation Plan - Final Modular Consolidation

## Phase 1: Auth UI Cleanup
- [x] Task: Move Auth UI to Gameplay
    - [x] Move `Assets/Scripts/Modules/Auth/UI/` to `Assets/Scripts/Gameplay/UI/Auth/`.
    - [x] Update namespaces to `Gameplay.UI.Auth`.

## Phase 2: UI Module Foundation
- [x] d78a1f5 Task: Move Generic UI Logic to Modules
    - [x] Move `Assets/Scripts/UI/Core/` to `Assets/Scripts/Modules/UI/Core/`.
    - [x] Move `Assets/Scripts/UI/Controls/` to `Assets/Scripts/Modules/UI/Controls/`.
    - [x] Move `Assets/Scripts/UI/UIService.cs` and related utility files to `Assets/Scripts/Modules/UI/`.
    - [x] Update namespaces to `Modules.UI`.

## Phase 3: Gameplay Logic Consolidation
- [x] Task: Move App States to Gameplay
    - [x] Move `Assets/Scripts/App/State/` to `Assets/Scripts/Gameplay/App/State/`.
    - [x] Update namespaces to `Gameplay.App.State`.
- [x] Task: Move App Networking to Gameplay
    - [x] Move `Assets/Scripts/App/Networking/` to `Assets/Scripts/Gameplay/App/Networking/`.
    - [x] Update namespaces to `Gameplay.App.Networking`.
- [x] Task: Move Game Screens & ViewModels to Gameplay
    - [x] Move `Assets/Scripts/UI/Screens/`, `Assets/Scripts/UI/ViewModels/`, `Assets/Scripts/UI/Components/`, `Assets/Scripts/UI/Popups/` to `Assets/Scripts/Gameplay/UI/`.
    - [x] Update namespaces to `Gameplay.UI`.

## Phase 4: Core Service Migration
- [x] Task: Move Banking Backends to Modules
    - [x] Move `Assets/Scripts/Core/Banking/Backends/` to `Assets/Scripts/Modules/Banking/Backends/`.
    - [x] Update namespaces to `Modules.Banking.Backends`.
- [x] Task: Move Session Management to Modules
    - [x] Move `Assets/Scripts/Core/Bootstrap/SessionManager.cs` to `Assets/Scripts/Modules/Session/SessionManager.cs`.
    - [x] Update namespaces to `Modules.Session`.

## Phase 5: Final Cleanup
- [x] Task: Dissolve App and UI Directories
    - [x] Ensure `Assets/Scripts/App` and `Assets/Scripts/UI` are empty and delete them.
- [x] Task: Update Assembly Definitions
    - [x] Final check of `RecipeRage.Modules` and `RecipeRage.Gameplay` asmdefs.
- [x] Task: Verify Compilation
- [x] Task: Conductor - User Manual Verification 'Phase 4: Final Cleanup'
