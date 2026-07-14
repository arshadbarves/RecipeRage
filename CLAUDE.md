# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

RecipeRage is a Unity 6.0 (6000.3.0f1) multiplayer cooking competition game. Teams of 2v2 or 3v3 cook dishes under time pressure. Built on VContainer DI, Netcode for GameObjects (NGO), EOS services, and UI Toolkit.

## Build & Test Commands

```bash
# Build a single assembly (Unity generates .csproj files)
dotnet build RecipeRage.Core.csproj -nologo
dotnet build RecipeRage.Gameplay.csproj -nologo

# Run all EditMode unit tests
dotnet test RecipeRage.Tests.EditMode.csproj --no-build -nologo

# Run a single test class or method
dotnet test RecipeRage.Tests.EditMode.csproj --filter="ClassName.MethodName" --no-build -nologo

# Run tests in CI mode (non-interactive, single execution)
CI=true dotnet test RecipeRage.Tests.EditMode.csproj --no-build -nologo
```

Full builds and PlayMode tests require the Unity Editor (File → Build Settings → Build; Window → Testing → Test Runner).

## Source-of-Truth Hierarchy

When architecture docs conflict, resolve in this order:
1. **Current code**
2. `wiki/` — LLM-maintained wiki (authoritative design memory, updated to match confirmed decisions)
3. `Documentation/Architecture/PROJECT_MEMORY.md` — living architecture memory
4. `KitchenClash_GDD_v3.md` — implementation-facing GDD
5. `Documentation/Architecture/CURRENT_CODEBASE_AUDIT.md`
6. `Documentation/KitchenClash_GDD_v3_aspirational.docx` — future phases only

## Wiki & Drift Protocol

**Before starting any implementation task, read the relevant wiki pages first:**
- Architecture changes → `wiki/Technical.md` + `wiki/LLM-Rules.md`
- Gameplay changes → `wiki/GameplayDesign.md` + `wiki/Gameplay.md`
- Character/ability changes → `wiki/Characters.md`
- Scoring/tuning → `wiki/Gameplay.md` (RC key table)
- Anything involving forbidden patterns → `wiki/LLM-Rules.md`

**Drift Warning Rule:** If a proposed change, new feature, or implementation decision contradicts
anything documented in the `wiki/` directory, you MUST issue a drift warning in the format
defined in `wiki/DRIFT-PROTOCOL.md` and wait for user confirmation before proceeding.

The drift warning format:
```
⚠️  DRIFT WARNING
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Wiki says:      [exact quote or summary]   Source: wiki/[Page].md
You are proposing: [description of conflict]
Impact:         [what breaks or changes]
Options:  A) Keep wiki  B) Update wiki  C) Investigate
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

When the user confirms option B: update the wiki page(s), add an updated note, and append to `wiki/log.md`.

## Architecture

### Assembly Structure (Two-Bucket)

```
Assets/Scripts/
  Core/           # Engine-agnostic, project-level input/utilities
  Gameplay/       # App wiring, state machine, networking, scene management
  Tests/EditMode/ # NUnit unit tests

Assets/_KitchenClash/
  Application/    # Business logic, service interfaces, domain events
  Domain/         # Domain models
  Infrastructure/ # Unity/EOS/NGO-specific implementations
    States/       # Concrete game states
    Network/      # PlayerController, spawning, NGO objects
    Gameplay/     # Cooking, abilities, hazards, bot AI
    EOS/          # Epic Online Services integration
    Persistence/  # Save/load via EOS Player Data Storage
  Presentation/   # UI Toolkit screens, view models
  Composition/    # DI lifetime scopes (do not put business logic here)
  Data/           # ScriptableObjects, DTOs
```

### Dependency Injection (VContainer)

Three nested scopes — never inject a child-scope service into a parent scope:

**Root (`RootLifetimeScope`)** — `Assets/_KitchenClash/Composition/RootLifetimeScope.cs`
- App-lifetime singletons: `IEventBus`, `ILoggingService`, `IAuthService`, `IUIService`, `IAppFlow`, `IConfigService`, `IRemoteConfigService`
- Session boot: `SessionManager`, `ISessionContext`, `MatchmakingPhaseHost` (ITickable)
- Root networking primitives: `IPlayerNetworkManager`, `INetworkObjectPool`, `INetworkGameManager`
- Player data: `IPlayerDataService` (economy/character often menu-scoped; handlers use `TryResolve`)
- All `BaseUIScreen` subclasses registered as Transient via reflection (`[UIScreen]`)
- **No** `IGameStateManager` / `IState` — hard-purged

**Session (`MenuLifetimeScope`)** — `Assets/_KitchenClash/Composition/MenuLifetimeScope.cs`
- Active-session services: `INetworkingServices` (via `NetworkingServiceContainer`), `ILobbyManager`, `IPlayerManager`, `IMatchmakingService`, `ITeamManager`, `IGameStarter`, `IEconomyService`, `ITutorialService`
- Does NOT own root network primitives (`INetworkObjectPool`, `INetworkGameManager`)
- Does NOT re-register `SessionManager` (Root owns cold-boot instance)

**Match (`MatchLifetimeScope`)**
- Per-match: `IScoreService`, `IOrderService`, `IAbilityService`, `IHazardService`, `IMatchContext`, `BotManager`, `BotClaimRegistry`, `BotTaskPlanner`, `RecipeCatalog`

### Product Navigation

**Public API:** `IAppFlow` (from Playcenter.GameFlow) — sole navigator for UI and features.
- UI screens call `IAppFlow.RequestPlay()`, `IAppFlow.ReturnHome()`, `IAppFlow.RequestPlayAgain()`
- Side phases: `IAppFlow.EnterSidePhase` / `CompleteSidePhase` via `ISidePhasePort`
- Phase work lives in port-owned handlers under `Infrastructure/Flow/Handlers/`

### Product Flow (handlers)

```
BootSequence → (Login side phase if needed) → HomePhase → MatchmakingPhase
  → Match Intro → Countdown → MatchRuntimePhase → ResultsPhase → HomePhase
```

- Entry: `GameBootstrapper` (registered as `IStartable`) calls `IAppFlow.StartColdBoot()`
- Ports (`BootFlowPort`, `HomeFlowPort`, …) delegate Enter/Exit to handlers
- Matchmaking timeout ticks via Root `MatchmakingPhaseHost` → `MatchmakingPhase.Tick()`

### Match Runtime Bridge

Scene MonoBehaviours are never found via `FindObjectOfType`. Instead:

- `MatchRuntimeSceneBinder` discovers and registers scene objects into `IMatchRuntimeRegistry`
- Gameplay systems inject `IMatchContext` to access `OrderManager`, `ScoreManager`, `RoundTimer`, `SpawnManager`, `PlayerController`, `IngredientNetworkSpawner`
- `IngredientCrate` resolves `IngredientNetworkSpawner` through `IMatchContext`, not `FindObjectOfType`
- `BotKitchenSnapshot` consumes station data from the runtime bridge, not direct scene searches

### Networking Rules

- `NetworkObjectPool` and `NetworkGameManager` use the `NetworkManager` instance injected by `GameLifetimeScope`, **not** `NetworkManager.Singleton`
- `IngredientNetworkSpawner` receives `INetworkObjectPool`/`INetworkGameManager` from root scope directly — not via `SessionManager.SessionContainer`
- `PlayerController` registers with `PlayerNetworkManager` only when `NetworkObject.IsPlayerObject == true`
- Bots are network objects but are **not** NGO player objects
- `SpawnManager` uses injected match runtime state for server-only guards, not `NetworkManager.Singleton`
- Team size follows queue-driven 2v2/3v3 format — no legacy 4-player fallbacks

### Event Bus

```csharp
_eventBus.Publish(new ScoreChangedEvent(team, delta, scoreA, scoreB));
_eventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
_eventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
```

Type-safe generic events. Use for decoupling systems (UI ↔ gameplay, score ↔ HUD). Tests use `SpyEventBus`.

### UI System

- All screens inherit `BaseUIScreen`, annotated with `[UIScreen]` for auto-discovery at root DI setup
- `IUIService` (root-owned) manages the screen stack via `UIScreenStackManager`
- Templates in UXML, styles in USS — no code-behind layout

## Key Patterns

### Adding a new service
1. Define `IMyService` interface in `Application/Interfaces/`
2. Implement in `Infrastructure/Services/`
3. Register in the appropriate `LifetimeScope.Configure()`: `builder.Register<MyServiceImpl>(Lifetime.Singleton).As<IMyService>()`
4. Inject via constructor — VContainer resolves automatically

### Adding a new game state
1. Create `MyState : BaseState` in `Infrastructure/States/`
2. State is auto-registered as Transient by root scope reflection scan (namespace must be `KitchenClash.Infrastructure.States`)
3. Trigger with `_stateManager.ChangeState<MyState>()`

### Adding a new UI screen
1. Create UXML template + USS in `_KitchenClash/UI/`
2. Create `MyScreen : BaseUIScreen` annotated with `[UIScreen]`
3. Class is auto-registered Transient by root scope reflection scan
4. Show with `_uiService.ShowScreen<MyScreen>()`

## Testing Conventions

- Framework: NUnit, location: `Assets/Scripts/Tests/EditMode/`
- Test naming: `MethodName_Condition_ExpectedResult`
- Use `SpyEventBus` and `DictionaryConfigService` as test doubles for event bus and config
- All match-scoped services (`IScoreService` etc.) are unit-testable without Unity runtime
- Target >80% coverage for all new code

## Task Workflow (conductor)

Tasks are tracked in `plan.md`. Workflow: select task → mark `[~]` → write failing test (Red) → implement (Green) → refactor → verify coverage → commit with message format `type(scope): description` → attach git note summary → mark `[x]` with short SHA in `plan.md`.

Commit types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`  
Conductor commits: `conductor(plan): ...`, `conductor(checkpoint): ...`

Tech stack changes must be documented in `conductor/tech-stack.md` **before** implementation.

## Code Style

- 4-space indentation, CRLF line endings, UTF-8 (enforced via `.editorconfig`)
- No `this.` qualification; `var` only for obvious types; explicit accessibility modifiers; prefer `readonly` fields
- Document *why*, not *what*; keep public APIs documented
