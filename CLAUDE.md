# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**RecipeRage** is a Unity 6.0 (6000.3.0f1) multiplayer cooking competition game for mobile (landscape). Two teams (2v2 or 3v3) race to complete a shared recipe list within 5 minutes. Built on **manual DI (composition roots, no container)**, Netcode for GameObjects (NGO) + EOS transport, Firebase, and UI Toolkit.

The project is a **from-scratch rebuild**. The old codebase (`Assets/_KitchenClash/`, `Assets/Scripts/`, old `Assets/Playcenter/`, old `wiki/`) is **legacy — do not read, extend, or copy from it**. It will be deleted once the new code works.

## Source of Truth (READ THESE FIRST)

1. **Design spec:** `docs/superpowers/specs/2026-07-25-reciperage-rebuild-design.md` — the GDD. All gameplay, economy, UI, and architecture decisions live here.
2. **Implementation plans:** `docs/superpowers/plans/2026-07-25-reciperage-*.md` — 6 plans (Phase 0 → Slice 5), 45 tasks with exact code. Execute these task-by-task.
3. **New code:** `Assets/Playcenter/` + `Assets/Game/` (once built)

`docs/superpowers/archive/` holds pre-rebuild docs for historical reference only — **never follow them**.

## Build & Verify

No test suite initially (owner decision). Verification = compile + run:

```bash
# Compile a single assembly (Unity generates .csproj files after first Editor open)
dotnet build Playcenter.Core.csproj -nologo
dotnet build RecipeRage.Gameplay.csproj -nologo
```

In-editor: open `Assets/Scenes/Boot.unity` → Play → check console for the SDK init log sequence defined in the Phase 0 plan.

## Architecture (New)

### Two Buckets

```
Assets/Playcenter/    # SDK — ALL core logic (auth, config, storage, analytics, ads, IAP,
                      # friends, audio, save, wallet). Reusable across games.
   Core/              # DI (ServiceLocator), EventBus, Logging, Time
   Services/          # Full implementations behind interfaces (stubs where external SDKs pending)
   UI/                # UIService screen stack, BaseUIScreen (UI Toolkit)
   Net/               # INetService, EOS transport/lobby

Assets/Game/          # RecipeRage — gameplay logic ONLY (never core services)
   DI/                # GameplayCompositionRoot
   Gameplay/          # Player, Stations, Recipes, Cooking, Match, Tutorial, Indicators
   Network/           # NetworkPlayer/Station/Match, lobby flow, runtime registry
   Bots/              # Task planner, evaluators, claim registry, adaptive difficulty
   Progression/       # Chef unlock/upgrade, trophies (game-specific)
   UI/                # Screens, components, UIAnimation
```

### Dependency Injection — Manual, Two Composition Roots

**No VContainer, no container, no reflection.** Two MonoBehaviour composition roots:

1. `PlaycenterCompositionRoot` (`Assets/Playcenter/Core/DI/`) — constructs + initializes every SDK service, registers in static `ServiceLocator`, fires `OnPlaycenterInitialized`.
2. `GameplayCompositionRoot` (`Assets/Game/DI/`) — listens for that event, then constructs game services using SDK services.

Consumers call `ServiceLocator.Get<IAuthService>()` etc. Never construct services outside composition roots.

### Core Game Rules (from the spec — do not change without a drift warning)

- **Loop:** fetch (instant) → chop (tap-burst, fixed count 8/10/12) → cook (autonomous, burns after grace) → plate (arrange) → serve
- **Matches:** 5 min; 2v2 = 12 recipes, 3v3 = 18; same seeded list both teams; mirrored kitchens
- **Carry capacity:** 2 for ALL chefs (Marco ability adds +1 at L5/L10)
- **Progress bars:** cook/burn timers visible to ALL players; off-screen stations get HUD-edge indicators
- **No points in HUD** — recipe counts only
- **Chefs:** 4 at launch (Gordon speed, Julia pickup/drop speed, Marco carry, Gustavo dash) + 2 locked. Personal utility abilities only — never chop/cook speed
- **Economy:** Brawl Stars costs (17,000 coins to max a chef). Coins earned/spent only, never lost. Trophies: win +15, loss -8
- **Auth:** Facebook, Google, Guest — **no Epic account login** (EOS is transport/storage only). Friends via Unity Gaming Services
- **Maps:** themed (Beach BBQ, Forest Campfire, Pirate Ship), daily rotation, 1-2 dynamic elements each
- **Tutorial:** forced interactive map on first launch
- **UI:** landscape, UI Toolkit, flat colors + soft shadows, **NO gradients**

### Networking Rules

- Server-authoritative: stations/match mutate only on server; clients render NetworkVariables
- Inject the `NetworkManager` instance — **never** use `NetworkManager.Singleton` in gameplay code
- Bots are network objects but **not** NGO player objects
- Scene lookups via `MatchRuntimeRegistry` — **never** `FindObjectOfType` in gameplay

### Event Bus + Audio

Custom lightweight pub/sub (`EventBus`). Audio is event-driven: gameplay publishes (`IngredientChoppedEvent`, `RecipeServedEvent`, …), `GameplayAudioWiring` (game assembly) maps events to SFX. Gameplay code never calls audio directly.

## Key Patterns (New Code)

### Adding an SDK service
1. Interface + implementation in `Assets/Playcenter/Services/<Area>/` (FULL logic in SDK)
2. Construct in `PlaycenterCompositionRoot.Awake()`, register in `ServiceLocator`
3. Initialize in `InitializeSDK()` coroutine in dependency order

### Adding a game service
1. Implementation in `Assets/Game/<Area>/` (consumes SDK interfaces)
2. Construct in `GameplayCompositionRoot.OnPlaycenterReady()`

### Adding a station
1. Inherit `StationBase` in `Assets/Game/Gameplay/Station/`
2. Server-authoritative wrapper (`NetworkXxxStation`) in `Assets/Game/Network/`
3. Publish domain events; never call audio/UI directly

### Adding a screen
1. `MyScreen : BaseUIScreen` + `[UIScreen]` in `Assets/Game/UI/Screens/`
2. UXML/USS own layout — code only queries and binds
3. Register in scene's `UIScreenRegistry`, show via `IUIService.Show<T>()`
4. Animate via `UIAnimation` (USS transitions, flat colors)

## Execution Workflow

Work = executing the plan series in order:

1. `2026-07-25-reciperage-phase0-foundation.md` (10 tasks)
2. `2026-07-25-reciperage-slice1-core-gameplay.md` (8)
3. `2026-07-25-reciperage-slice2-multiplayer.md` (7)
4. `2026-07-25-reciperage-slice3-bots.md` (6)
5. `2026-07-25-reciperage-slice4-progression.md` (6)
6. `2026-07-25-reciperage-slice5-monetization-polish.md` (8)

Each task has exact files, code, verification, and a commit step. Commit format: `type(scope): description` — types: `feat`, `fix`, `docs`, `style`, `refactor`, `chore`.

## Drift Protocol

If a proposed change contradicts the spec (`docs/superpowers/specs/2026-07-25-reciperage-rebuild-design.md`), issue this warning and wait for confirmation:

```
⚠️  DRIFT WARNING
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Spec says:        [exact quote]   Source: spec section [name]
You are proposing: [description of conflict]
Impact:           [what breaks or changes]
Options:  A) Keep spec  B) Update spec  C) Investigate
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

On option B: update the spec first, then implement.

## Legacy Cleanup (do this only when instructed)

Delete after new code is verified working: `Assets/_KitchenClash/`, `Assets/Scripts/`, old `wiki/`, `CODEBASE_ANALYSIS.md`, `ANALYSIS_INDEX.md`, `QUICK_REFERENCE.md`, `KitchenClash_GDD_v3.md`, `_bmad*/`, `Documentation/`, `results.xml`, `test_log.txt`, `etc/`, `MockTests.csproj`, old `conductor/` contents.

## Code Style

- 4-space indentation, CRLF line endings, UTF-8 (enforced via `.editorconfig`)
- No `this.` qualification; `var` only for obvious types; explicit accessibility modifiers; prefer `readonly` fields
- Namespaces: `Playcenter.*` (SDK), `RecipeRage.*` (game)
- Document *why*, not *what*; keep public APIs documented
