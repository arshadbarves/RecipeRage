# Plan: Core Refactor - Phase 1 (Architecture Stabilization)

## Phase 1: Preparation & State Refactoring
Clean up the "easy" dependencies first to isolate the harder ones.

- [x] Task: Refactor Game States
    -   Update `GameplayState` constructor to accept `NetworkInitializer`, `OrderManager`, `ScoreManager`.
    -   Update `GameOverState` constructor to accept `ScoreManager`.
    -   Register these managers in `GameLifetimeScope` or `SessionLifetimeScope` if they aren't already.
    -   Remove `FindFirstObjectByType` calls from these states.

## Phase 2: Player Controller Injection
Handle the dynamic instantiation of players properly.

- [x] Task: Create PlayerFactory
    -   Create `IPlayerFactory` and `PlayerFactory` implementation.
    -   Register `PlayerFactory` in `SessionLifetimeScope`.
    -   Update `PlayerController` to use `[Inject]` for its dependencies.
    -   Update `PlayerNetworkManager` (or whoever spawns players) to use the `PlayerFactory` (or ensure `NetworkObject` spawning works with VContainer - *Note: NGO often handles spawning, so we may need to use `NetworkPrefab` injection or a VContainer extension for NGO. For now, we'll try to use `IObjectResolver.InjectGameObject` immediately after spawn if a Factory pattern isn't viable with NGO's spawning mechanism.*).
    -   *Correction:* Since it's NGO, we might need to stick to `Inject(this)` but use a scoped resolver passed to the spawner, NOT the global static one.
    -   Goal: Remove `GameBootstrap.Container` reference from `PlayerController`.

## Phase 3: The Big Kill
Remove the global access point.

- [x] Task: Remove GameBootstrap Statics
    -   Delete `Instance` and `Container` properties from `GameBootstrap`.
    -   Fix any remaining compilation errors (hunt down any other files using `GameBootstrap.Container`).
    -   Verify the project compiles and runs.
