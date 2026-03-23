# Kitchen Clash GDD v3

Current-state rewrite based on the RecipeRage repository as of March 23, 2026.

## Status Legend

- `Implemented` means the behavior exists in the current repo.
- `Partial` means the repo contains some of the pieces, but not the full target flow.
- `Planned` means the older design intent exists in docs, but the current repo does not fully implement it.

## Game Overview

- Genre: competitive multiplayer kitchen game
- Engine: Unity 2022.3 LTS
- Networking: NGO with EOS-backed services
- UI: UI Toolkit screens and view models, managed through `UIService`
- DI: VContainer
- Remote config: Firebase-backed provider abstractions and RecipeRage config models

## Match Format

- Queue definitions and game mode assets currently support `quick_2v2` and `quick_3v3`
- Generic `teamSize = 4` fallbacks were removed from matchmaking and lobby defaults
- Status: `Partial`

The design target is smaller team-based kitchen matches, and the obvious legacy 4-per-team fallbacks have been removed. This remains `Partial` until runtime verification confirms queue-selected team size drives the full matchmaking and lobby flow end to end.

## Current Runtime Architecture

### Composition Scopes

- Root scope: `GameLifetimeScope`
- Session scope: `SessionLifetimeScope`
- Status: `Implemented`

There is no active `Root/Menu/Match` scope split in the current repo. A historical `GameplayLifetimeScope` path existed in code but was not wired into scenes or prefabs and has been removed as dead infrastructure.

### State Flow

- `BootstrapState`
- `LoginState`
- `MainMenuState`
- `MatchmakingState`
- `GameplayState`
- `GameOverState`

Status: `Implemented`

Matchmaking is state-owned. The play button leads into `MatchmakingState`, not a UI-owned matchmaking loop.

### UI Architecture

- UI Toolkit templates and USS styles
- screen creation through `UIService`
- layered screen management through `UIScreenStackManager`
- view-model-backed screen logic in `Assets/Scripts/Gameplay/UI/Features`
- Status: `Implemented`

The repository does not currently use the `RouterService` push/pop stack described by the older aspirational GDD.

## Networking And Session Model

### Implemented

- EOS-oriented networking services created through `NetworkingServiceContainer`
- root-owned networking primitives:
  - `IPlayerNetworkManager`
  - `INetworkObjectPool`
  - `INetworkGameManager`
- session-owned matchmaking/lobby/team/game-start services
- gameplay scene references published through `MatchContext` and `MatchRuntimeSceneBinder`

### Partial

- the repo still uses some global/static access patterns such as:
  - `EOSManager.Instance`
  - `NetworkManager.Singleton`
  - `CoroutineRunner`

This means the older “zero static singletons” goal is not yet true in current code.

## Authentication

### Implemented

- guest/EOS-first flow is active
- login boot flow is state-driven and integrates with app startup

### Partial / Planned

- the old GDD promised full direct Google/Facebook/Apple token linkage into EOS Connect
- the current repo does not show that full provider set implemented end-to-end

Current truth: authentication is still centered on the implemented EOS-backed login path, with external-provider ambition documented but not fully realized in code.

## Core Gameplay Loop

### Implemented

1. Bootstrap builds the app shell.
2. Session loading prepares player-facing services.
3. Main menu exposes game mode, character, shop, and social UI.
4. Matchmaking searches or creates a lobby and fills with bots after timeout.
5. Gameplay loads `Game.unity`, optionally loads the selected map additively, starts the network session, then exposes runtime objects to HUD and app code through `MatchContext`.
6. Kitchen systems handle stations, ingredients, orders, scoring, and bots.

### Partial

- `GameOverState` is now reached from timer expiry through `GameplayHudViewModel`
- non-timer end conditions still need explicit runtime verification

## Documentation Policy

The current source of truth is:

1. current code
2. `Documentation/Architecture/PROJECT_MEMORY.md`
3. `Documentation/Architecture/CURRENT_CODEBASE_AUDIT.md`
4. `Documentation/Guides/gameplay-scene-setup.md`
5. `KitchenClash_GDD_v3.md`

Phase-development target:

- `Documentation/KitchenClash_GDD_v3_aspirational.docx`

When the implementation-facing markdown GDD and the aspirational phase doc disagree:

- implementation work follows the markdown GDD plus current code
- the aspirational `.docx` remains the planning reference for future phases

Historical docs remain for context only and are archived or marked as legacy references.
