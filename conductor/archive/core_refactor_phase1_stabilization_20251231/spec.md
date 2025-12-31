# Specification: Core Refactor - Phase 1 (Architecture Stabilization)

## Goal
To eliminate legacy "Service Locator" patterns and enforce strict Dependency Injection (DI) using VContainer. The primary target is the removal of the global `GameBootstrap.Instance` and its static `Container` property, which currently allows any class to bypass the dependency graph.

## Scope
1.  **PlayerController Refactoring:**
    -   Convert `PlayerController` from a manually injected MonoBehaviour to a properly constructed entity.
    -   Implement a `PlayerFactory` (registered in `SessionLifetimeScope`) to handle player instantiation and dependency resolution.
    -   Remove `GameBootstrap.Container.Inject(this)` from `PlayerController.Awake()`.

2.  **Game State Refactoring:**
    -   Update `GameplayState` and `GameOverState` to receive all necessary dependencies via their constructors.
    -   Remove usage of `Object.FindFirstObjectByType` for finding managers (`NetworkInitializer`, `OrderManager`, `ScoreManager`).

3.  **Removal of Static Access:**
    -   Delete `public static GameBootstrap Instance`.
    -   Delete `public static IObjectResolver Container`.
    -   Fix all compilation errors resulting from this removal by ensuring dependencies are passed down the chain or resolved via the active scope.

## Success Criteria
-   `GameBootstrap.cs` has NO static properties.
-   `PlayerController.cs` uses `[Inject]` or constructor injection (via factory) and has NO reference to a static container.
-   `GameplayState` and `GameOverState` have NO `FindFirstObjectByType` calls.
-   The game compiles and runs with the same functionality as before.
