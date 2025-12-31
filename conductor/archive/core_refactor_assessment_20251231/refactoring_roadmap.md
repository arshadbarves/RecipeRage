# Refactoring Roadmap

Based on the assessment, the following roadmap is proposed to bring the codebase to a fully modular, DI-driven architecture.

## Phase 1: Architecture Stabilization (The "Strict DI" Phase)
**Goal:** Eliminate `GameBootstrap.Instance` and all Service Locator usage.

1.  **Refactor `PlayerController`:**
    *   Create a `PlayerFactory` (registered in `SessionLifetimeScope`).
    *   Change `PlayerController` to use `[Inject]` for dependencies.
    *   Remove `GameBootstrap.Container.Inject(this)`.
2.  **Refactor Game States:**
    *   Update `GameplayState` and `GameOverState` constructors to accept `NetworkInitializer`, `OrderManager`, etc.
    *   Remove `FindFirstObjectByType` calls.
3.  **Kill the Singleton:**
    *   Remove `public static GameBootstrap Instance`.
    *   Remove `public static IObjectResolver Container`.
    *   Fix any resulting compilation errors by threading dependencies correctly.

## Phase 2: UI Decoupling (The "MVVM" Phase)
**Goal:** Move logic out of `Assets/Scripts/UI`.

1.  **Extract `GameplayInputController`:**
    *   Move raycasting/interaction logic out of `GameplayUIManager` into a new `GameplayInputController` (Core/Input).
2.  **Create ViewModels:**
    *   `ScoreViewModel`: Handles score calculation and formatting.
    *   `OrderViewModel`: Handles timer logic for orders.
3.  **Refactor `MainMenuScreen`:**
    *   Create a `MainMenuController` that orchestrates the sub-tabs and services.
    *   `MainMenuScreen` becomes a "dumb" view that just listens to the Controller.

## Phase 3: State Encapsulation
**Goal:** Protect mutable state.

1.  **Refactor `PlayerController` Stats:**
    *   Make `InteractionSpeedModifier` and `CarryingCapacity` private/readonly.
    *   Expose methods `ModifySpeed(...)` or `AddCapacity(...)` to control changes.

## Future: Gameplay Polish
*   Once Core and UI are stable, revisit `CharacterAbility.cs` to implement the missing `TODO` features.
