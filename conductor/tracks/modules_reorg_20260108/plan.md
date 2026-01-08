# Implementation Plan - Modules Reorganization

## Phase 1: Directory Structure & Core Setup [checkpoint: 5c3f62f]
- [x] a71ab97 Task: Create Directory Structure
    - [ ] Create `Assets/Scripts/Modules/`
    - [ ] Create `Assets/Scripts/Modules/Core/`
    - [ ] Create `Assets/Scripts/Modules/Networking/`
    - [ ] Create `Assets/Scripts/Modules/UI/`
- [x] 5c3f62f Task: Conductor - User Manual Verification 'Phase 1: Directory Structure & Core Setup' (Protocol in workflow.md)

## Phase 2: BankService Analysis & Refactor (Core)
- [x] Task: Analyze CurrencyService Dependencies
    - [x] Map all public methods (`AddCoins`, `SpendGems`, etc.) to the new `BankService` API.
    - [x] Identify all Event Bus events (`CurrencyChangedEvent`) that must be maintained or mapped.
    - [x] Analyze `ISaveService` usage to ensure data is correctly migrated/handled by the new Backend.
- [x] 930a4a7 Task: Define Interfaces
    - [x] Create `IBankBackend.cs` in `Modules/Core/Banking/Interfaces/`.
    - [x] Create `IBankService.cs` (extending or replacing `ICurrencyService`) to ensure API compatibility.
- [x] f8aff0d Task: Implement BankService
    - [x] Create `BankService.cs` in `Modules/Core/Banking/`.
    - [x] Implement support for multiple currencies (Coins, Gems) and Inventory (Skins).
    - [x] Ensure `IEventBus` integration triggers existing events (or compatible new ones).
    - [x] Ensure `IUIService` integration (notifications) is preserved.
- [~] Task: Migrate CurrencyService
    - [ ] Find all references to `CurrencyService`.
    - [ ] Replace usage with `BankService`.
    - [ ] Verify `CurrencyChangedEvent` is still triggered correctly for UI updates.
- [ ] Task: Conductor - User Manual Verification 'Phase 2: BankService Refactor (Core)' (Protocol in workflow.md)

## Phase 3: EOS Backend Implementation
- [ ] Task: Implement EOS Backend
    - [ ] Create `EOSBankBackend.cs` in `Modules/Core/Banking/Backends/`
    - [ ] Implement `IBankBackend`.
    - [ ] Add logic to interface with Epic Online Services Player Data Storage.
- [ ] Task: Connect EOS Backend
    - [ ] Configure `BankService` to initialize with `EOSBankBackend`.
- [ ] Task: Conductor - User Manual Verification 'Phase 3: EOS Backend Implementation' (Protocol in workflow.md)

## Phase 4: Core Utilities Migration
- [ ] Task: Analyze & Move Audio System
    - [ ] Check for hardcoded paths or game-specific dependencies in Audio scripts.
    - [ ] Move to `Assets/Scripts/Modules/Core/Audio/`.
    - [ ] Update namespaces and fix references.
- [ ] Task: Analyze & Move Input System
    - [ ] Move to `Assets/Scripts/Modules/Core/Input/`.
    - [ ] Update namespaces and fix references.
- [ ] Task: Analyze & Move Scene Management
    - [ ] Move to `Assets/Scripts/Modules/Core/Scenes/`.
    - [ ] Update namespaces and fix references.
- [ ] Task: Conductor - User Manual Verification 'Phase 4: Core Utilities Migration' (Protocol in workflow.md)

## Phase 5: Networking & UI Migration
- [ ] Task: Analyze & Move Networking Foundations
    - [ ] Verify separation of generic logic from game-specific network messages.
    - [ ] Move to `Assets/Scripts/Modules/Networking/`.
- [ ] Task: Analyze & Move UI Framework
    - [ ] Move generic UI managers to `Assets/Scripts/Modules/UI/`.
- [ ] Task: Conductor - User Manual Verification 'Phase 5: Networking & UI Migration' (Protocol in workflow.md)

## Phase 6: Final Verification
- [ ] Task: Full Project Compilation Check
    - [ ] Ensure no compile errors exist.
- [ ] Task: Game Logic Regression Test
    - [ ] Verify Currency adds/spends work in-game.
    - [ ] Verify UI updates on currency change.
- [ ] Task: Conductor - User Manual Verification 'Phase 6: Final Verification' (Protocol in workflow.md)
