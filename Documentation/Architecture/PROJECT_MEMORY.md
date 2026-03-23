# RecipeRage Project Memory

This file is the living architecture memory for the current RecipeRage codebase.

Use it as the first architecture reference for implementation work in this repository. If this file conflicts with older architecture notes, prefer this file and current code, then update the older docs later if needed.

## Purpose

- Keep a short, current memory of how the project is actually wired now.
- Reduce drift between implementation, plans, and historical documentation.
- Give agents one current architecture entry point before they start changing code.

## Source-of-Truth Rule

When architecture docs conflict:

1. Current code wins.
2. This file should be updated to match the code.
3. Historical docs may remain for context, but should not override current implementation.

GDD rule:

- `KitchenClash_GDD_v3.md` is the implementation-facing GDD.
- `Documentation/KitchenClash_GDD_v3_aspirational.docx` is the phase-development target.
- If they disagree, implementation work follows current code plus `KitchenClash_GDD_v3.md`.

## Current Architecture Anchors

### Root application scope

Root DI is configured in:

- `Assets/Scripts/Gameplay/Bootstrap/GameLifetimeScope.cs`

This scope owns app-level services, including:

- logging, event bus, localization, UI service
- session manager and session context
- match context / match runtime registry
- auth, maintenance, connectivity, remote config
- game state manager and game states
- root networking primitives:
  - `IPlayerNetworkManager`
  - `INetworkObjectPool`
  - `INetworkGameManager`

### Session scope

Session-scoped DI is configured in:

- `Assets/Scripts/Gameplay/Bootstrap/SessionLifetimeScope.cs`

This scope owns active-session services, including:

- `INetworkingServices` via `NetworkingServiceContainer`
- `ILobbyManager`
- `IPlayerManager`
- `IMatchmakingService`
- `ITeamManager`
- `IGameStarter`

Current ownership rule:

- `CharacterService`
- `EconomyService`
- `PlayerDataService`

are root-owned services exposed through `SessionContext`.

Important constraint:

- `SessionLifetimeScope` is not the place for root network object lifecycle services like `INetworkObjectPool` or `INetworkGameManager`.

### Match runtime scene bridge

Gameplay-scene runtime objects are published into match context through:

- `Assets/Scripts/Gameplay/Shared/MatchRuntimeSceneBinder.cs`

This binder resolves scene references and registers them into `IMatchRuntimeRegistry`, so gameplay systems do not need ad-hoc scene lookups everywhere.

### Gameplay runtime networking

Current gameplay networking assumptions:

- `IngredientNetworkSpawner` is a scene `MonoBehaviour` that receives `INetworkObjectPool` and `INetworkGameManager` from the root `GameLifetimeScope`.
- It should not resolve those services from `SessionManager.SessionContainer`.
- Ingredient assets provide their own prefab references.
- `PlayerController` should only register with `PlayerNetworkManager` when `NetworkObject.IsPlayerObject` is true.
- Bots are network objects, but they are not NGO player objects.
- Generic lobby and matchmaking team-size fallbacks should align to the queue-driven 2v2 / 3v3 format, not legacy 4-player assumptions.

## Current State Flow

The active gameplay flow is state-driven.

Current flow:

`BootstrapState -> LoginState/MainMenuState -> MatchmakingState -> GameplayState -> GameOverState`

Current matchmaking entry:

- `LobbyTabComponent.OnPlayClicked()` calls `LobbyViewModel.Play()`
- `LobbyViewModel.Play()` calls `IGameStateManager.ChangeState<MatchmakingState>()`
- `MatchmakingState` owns matchmaking startup, timeout, bot filling, and transition to gameplay

Important note:

- `Documentation/Architecture/FINAL_ARCHITECTURE.md` is a historical reference and is only useful when it agrees with current code.
- Use current code and this file as truth if those docs disagree.

## Current Gameplay Initialization Flow

### Matchmaking

- `MatchmakingState.Enter()` checks maintenance and starts matchmaking.
- `MatchmakingState.StartMatchmaking()` resolves mode/team size and starts `IMatchmakingService.FindMatch(...)`.
- On timeout, `MatchmakingState.Update()` triggers `FillMatchWithBots()`.

### Gameplay

- `GameplayState.Enter()` loads `Game` scene if needed.
- `GameplayState.InitializeGameplayAsync()`:
  - initializes camera systems
  - loads the selected map scene additively
  - calls `_sessionContext.GameStarter?.StartGame()`
  - hides modal/popup UI and shows gameplay HUD
- `GameplayHudViewModel` transitions to `GameOverState` when `RoundTimer` expires.
- Non-timer end conditions still need explicit runtime verification to ensure they converge on the same game-over flow.

## Documentation Set

Use these files for current RecipeRage implementation work:

- Documentation index:
  - `Documentation/README.md`
- Architecture memory:
  - `Documentation/Architecture/PROJECT_MEMORY.md`
- Current-code audit:
  - `Documentation/Architecture/CURRENT_CODEBASE_AUDIT.md`
- GDD alignment matrix:
  - `Documentation/Architecture/GDD_ALIGNMENT_MATRIX.md`
- Scene/manual setup:
  - `Documentation/Guides/gameplay-scene-setup.md`
- Current-state GDD:
  - `KitchenClash_GDD_v3.md`
- Phase-development GDD:
  - `Documentation/KitchenClash_GDD_v3_aspirational.docx`
- Historical/secondary references:
  - `Documentation/Architecture/FINAL_ARCHITECTURE.md`
  - `Documentation/Architecture/PLAYER_CONTROLLER_ARCHITECTURE.md`
  - `Documentation/Architecture/STATE_TRANSITION_FLOW.md`
  - `Documentation/Archive/2026-03-cleanup/`

## Update Rules

Update this file in the same task when any of these change:

- DI ownership or service scope
- state flow or state responsibilities
- matchmaking ownership
- gameplay scene binding rules
- network object ownership / registration rules
- source-of-truth doc paths

Do not update this file for isolated logic fixes that do not affect architecture, ownership, flow, or project conventions.

## Agent Operating Rule

For RecipeRage tasks:

1. Read this file first.
2. Read `Documentation/Guides/gameplay-scene-setup.md` if the task touches scene/prefab/asset/manual setup.
3. Decide whether the task changed project architecture or memory.
4. If yes, update this file before finishing.
5. If no, say explicitly in the final response that the architecture memory did not need updating.
