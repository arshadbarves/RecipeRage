# Specification: Core Reorganization and Scalability Refactor

## Overview
This track focuses on reorganizing the `Assets/Scripts` directory to extract common, reusable systems into a high-level "Core" structure. The goal is to create broad, scalable categories (Core, Networking, UI) that can be reused across projects. A central component is the refactoring of `CurrencyService` into a comprehensive `BankService` (located within the Core module) that handles all player persistence data, including currency, inventory (skins), and progression, using an abstracted backend (EOS Player Storage).

## Functional Requirements
- **Directory Restructuring**:
    - Create `Assets/Scripts/Core/` root.
    - Define broad module categories: `Core`, `Networking`, `UI`.
- **Core Module (`Assets/Scripts/Core/Core/`)**:
    - **BankService (Refactor)**:
        - Rename/Refactor `CurrencyService` to `BankService`.
        - **Scope**: Handles Currency, Inventory (Skins/Items), and Progression data.
        - **Architecture**: Use `IBankBackend` (or similar interface) to decouple logic from storage.
        - **Implementation**: Create an EOS-specific backend using Player Storage to save/load this data.
    - **Utilities**: Move Audio, Input, Scene management, and other foundational systems here.
- **Networking Module (`Assets/Scripts/Core/Networking/`)**:
    - Move generic connection logic and networking foundations here.
- **UI Module (`Assets/Scripts/Core/UI/`)**:
    - Move generic window management, popups, and UI framework utilities here.
- **Naming Convention**: Standardize naming to be project-agnostic.

## Non-Functional Requirements
- **Reusability**: `Core` systems must not depend on game-specific logic (e.g., specific recipes or gameplay mechanics).
- **Scalability**: The `BankService` must be designed to easily add new item types or currencies without rewriting the storage logic.
- **Abstraction**: Data persistence must be backend-agnostic (Interface-driven).

## Acceptance Criteria
- [ ] `Assets/Scripts/Core/` contains only high-level folders: `Core`, `Networking`, `UI`.
- [ ] `BankService` is implemented in `Core/Core/` and successfully handles:
    - [ ] Currency balances.
    - [ ] Inventory items (Skins).
    - [ ] Progression data.
- [ ] An EOS implementation for `BankService` persistence is functioning.
- [ ] Core utilities (Audio, Input, etc.) are successfully moved to `Core/Core/`.
- [ ] The project compiles and runs with the new structure.
