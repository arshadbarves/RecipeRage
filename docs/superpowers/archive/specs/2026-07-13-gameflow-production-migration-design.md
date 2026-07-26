# GameFlow Production Migration — Design

**Date:** 2026-07-13  
**Branch:** `architecture-cleanup` (or follow-on `gameflow-migration`)  
**Status:** Design approved for implementation (product intent: complete cutover; retire dual navigation)  
**Related:** `wiki/GameFlow-SDK.md`, `Assets/Playcenter/GameFlow/`, Architecture Cleanup (complete)

---

## 1. Problem

RecipeRage has two product navigators:

| Layer | Status today |
|-------|----------------|
| **`IGameStateManager` + `*State`** | **Live.** Bootstrap, UI, matchmaking, and HUD call `ChangeState<T>()` directly. |
| **`IAppFlow` / `AppFlowController`** | **Compile-only.** Ports and policies exist; **not registered** in `RootLifetimeScope`. UI does not call it. |

That dual path is not production-grade: legal transitions live in two places, Intro/Countdown beats are skipped (MM → Gameplay), and wiki/`GameFlow-SDK` already defines the target that code does not enforce.

**Goal:** Make `IAppFlow` the **only** public product navigator. Delete dual navigation once cutover criteria pass. Keep Root/Menu/Match DI and match gameplay systems; this is a **shell/navigation** migration, not a combat/map vertical slice.

---

## 2. Assumptions (locked)

1. **In scope:** Product shell flow (cold boot → home → play → MM → intro → countdown → match → results → home), DI registration, UI/feature intent cutover, port completion, tests, wiki alignment, deletion of dual public navigation.
2. **Out of scope:** Kitchen Brawler combat stations, map content, untracked v2 prefabs, EOS feature work, PR auth (`gh login`).
3. **`IGameStateManager` end state:** Remains as an **internal phase worker** used only by Flow ports (and optionally thin state classes). Public UI/features **must not** inject or call it after cutover. Full deletion of the state machine is a **follow-on** only if ports fully absorb Enter/Exit without workers — not required for “migration complete.”
4. **Intro + Countdown are required** in the production path (no permanent MM → Match shortcut for shipping). Temporary legal edge `Matchmaking → Match` may remain in `AppFlowController` for tests/debug only, not for product UI.
5. **Always-resolve matchmaking** and **soft-popup policy** stay as implemented in Playcenter.GameFlow.
6. **Autopilot / product intent** treats “complete migration and get rid of the old things once done” as approval of Approach A below.

---

## 3. Approaches considered

### A — Phased cutover with delete gates (recommended)

Wire `IAppFlow` in Root DI → flip entry + UI intents → rewire states to **notify** flow instead of `ChangeState` → enable Intro/Countdown → delete dual call sites → document.

- **Pros:** Ship-safe; each gate is verifiable; matches existing port adapters; no big-bang risk.
- **Cons:** Short dual-path window during intermediate commits (ports still call `ChangeState` internally — that is intentional, not dual public API).

### B — Big-bang single change set

Replace all navigation in one commit/PR.

- **Pros:** No intermediate dual public API.
- **Cons:** High regression risk on boot/auth/MM/match end; hard to bisect.

### C — Adapter forever

Keep both public APIs; ports wrap states indefinitely.

- **Pros:** Lowest short-term risk.
- **Cons:** Explicitly rejected — user wants production cutover and removal of old dual paths.

**Decision: Approach A.**

---

## 4. Target architecture

```
UI / ViewModels / Features
        │  intents only
        ▼
   IAppFlow  (Playcenter.GameFlow — legal transitions + policies)
        │  port Enter/Exit
        ▼
   Flow ports (KitchenClash.Infrastructure.Flow)
        │  may drive workers
        ▼
   Phase workers (existing *State via IGameStateManager)  [internal]
        │
        ▼
   Scenes / UIService / Match services / Network
```

### Rules (enforced by review + tests)

1. **Public navigation API = `IAppFlow` only.**  
   Forbidden after cutover in Presentation and feature code: `IGameStateManager.ChangeState*`, injecting `IGameStateManager` for navigation.
2. **States report outcomes upward:** e.g. match found → `NotifyMatchResolved`; match end → `NotifyMatchCompleted`; cancel → `CancelMatchmaking` / `ReturnHome` as appropriate. States do **not** choose the next product phase via `ChangeState` to sibling product states.
3. **Ports own phase presentation:** show/hide screens, load scenes, start round after countdown.
4. **Fail-closed:** illegal or failed paths use `ReturnHome` / side phases via `IAppFlow`, not ad-hoc state jumps from UI.
5. **DI:** `IAppFlow` is a **Root** singleton. Construction uses `AppFlowProxy` for ports that need `IAppFlow` during controller build (cycle break), per `wiki/GameFlow-SDK.md`.

---

## 5. Components

### 5.1 Playcenter.GameFlow (keep; minor only if needed)

| Type | Role |
|------|------|
| `IAppFlow` | Public intents |
| `AppFlowController` | Legal graph + phase orchestration |
| Ports interfaces | Splash, Boot, Home, MM, Intro, Countdown, Match, Results, Popup, Analytics |
| Policies | AlwaysResolve, SoftPopup, RememberedQueue |
| DTOs | `PlayRequest`, `MatchResolvedInfo`, `MatchResultInfo`, `FlowContext` |

No KitchenClash types inside this assembly (already `noEngineReferences: true`).

### 5.2 Infrastructure.Flow adapters (complete + register)

| Port | Implementation | Notes |
|------|----------------|-------|
| Splash | **New** `SplashFlowPort` (or Boot-combined) | Today Null; Bootstrap owns splash dwell — split or map Boot+Splash to existing bootstrap work |
| Boot | **New** `BootFlowPort` | Auth, RC, NTP, gates → side phases / Home via flow callbacks |
| Home | `HomeFlowPort` | Exists → MainMenuState worker |
| Matchmaking | `MatchmakingFlowPort` | Exists; state must notify flow on resolve/cancel/fail |
| MatchIntro | `MatchIntroFlowPort` | Exists; real beat |
| Countdown | `CountdownFlowPort` | Exists; real beat |
| MatchRuntime | `MatchRuntimeFlowPort` | Exists; `StartRound` only after countdown |
| Results | `ResultsFlowPort` | Exists → GameOverState + Results UI |
| Analytics | `AnalyticsFlowPort` | Exists |
| Popup | `SoftPopupPolicy` | Module default OK |

`Null*Port` types remain only for unit tests of `AppFlowController`, not production DI.

### 5.3 Composition

- **`RootLifetimeScope`:** Register `IAppFlow` factory (wiki snippet). Register concrete ports as needed (inline factory is fine for cycle control).
- **`GameBootstrapper`:** Call `IAppFlow.StartColdBoot()` instead of `Initialize(BootstrapState)` **or** initialize state machine empty and let Splash/Boot ports enter workers. Preferred: bootstrap starts flow; Boot/Splash ports drive existing bootstrap sequence once.
- **Menu / Match scopes:** Unchanged ownership of session/match services.

### 5.4 Presentation cutover (delete dual public nav)

| Call site (today) | After |
|-------------------|--------|
| `LobbyViewModel.Play()` → `ChangeState<MatchmakingState>` | `IAppFlow.RequestPlay(PlayRequest)` |
| `GameplayHudViewModel` → `ChangeState<GameOverState>` | `IAppFlow.NotifyMatchCompleted(MatchResultInfo)` (prefer match runtime / evaluator, not HUD) |
| Results / Play Again / Home buttons | `RequestPlayAgain` / `ReturnHome` |
| Cancel matchmaking UI | `CancelMatchmaking` |
| Soft offer gates | `CanShowSoftPopup()` |

### 5.5 State workers (internal rewire)

| State | Change |
|-------|--------|
| `BootstrapState` | Invoked from Splash/Boot ports; completion → flow side-phase or Home (not raw `ChangeState` chain for product phases where flow owns the graph). Practical path: Boot port runs bootstrap work **or** BootstrapState calls into `IAppFlow` for next phase only. |
| `MatchmakingState` | On match found → `NotifyMatchResolved` (not `ChangeState<GameplayState>`). Cancel/fail → flow cancel/home. |
| `GameplayState` | Match end path → `NotifyMatchCompleted`. No direct GameOver transition from HUD. |
| `GameOverState` | Entered by Results port; Play Again/Home via `IAppFlow` only. |
| Side states (Login, Maintenance, Tutorial, …) | Entered via `EnterSidePhase` / Boot decisions; complete via `CompleteSidePhase` or explicit flow APIs. |

Internal `ChangeState` **inside ports** to enter a worker remains allowed until a later “delete workers” pass.

### 5.6 What gets deleted (definition of “get rid of old things”)

**Delete / ban when gates pass:**

1. All **Presentation** and **feature** references to `IGameStateManager` for navigation.
2. Product-level `ChangeState` between shell phases from states (MM→Gameplay, MM→MainMenu, HUD→GameOver, etc.) — replaced by `IAppFlow` notifies/intents.
3. Production use of `NullSplashPort` / `NullBootPort` / `NullMatchIntroPort` / `NullCountdownPort`.
4. Docs that describe `ChangeState` as the public product API (update wiki + CLAUDE patterns).
5. Optional: obsolete comments / dead dual-entry helpers.

**Do not delete in this migration:**

- `IGameStateManager`, `GameStateManager`, `*State` classes (workers).
- Match/session DI, networking, EOS.
- Legal `Matchmaking → Match` edge in controller (debug/tests) if documented as non-product.

---

## 6. Player journey (production)

```
StartColdBoot
  → StudioSplash (port)
  → Boot (auth, config, profile; gates)
      → side: ForceUpdate | Maintenance | NoConnection | Login | Tutorial | AccountUpgrade
      → Home
  → RequestPlay (RememberedQueuePolicy)
  → Matchmaking (AlwaysResolve → bots)
  → NotifyMatchResolved
  → MatchIntro (card + load signal)
  → NotifyMatchIntroReady
  → Countdown 3-2-1-GO
  → NotifyCountdownComplete
  → Match (EnterMatch + StartRound)
  → NotifyMatchCompleted
  → Results
  → RequestPlayAgain | ReturnHome
```

---

## 7. Data flow

### Play request

- UI builds optional `PlayRequest` (modeId, teamSize, chefId) or empty → `RememberedQueuePolicy` fills from `FlowContext`.
- `RequestPlay` → MM port `EnterMatchmaking(context, request)`.

### Match resolved

- MM service finds lobby / bot fill.
- `MatchmakingState` (or port subscriber) builds `MatchResolvedInfo` via `FlowMatchInfoFactory` → `NotifyMatchResolved`.
- Controller → Intro → (optional parallel map enter) → Countdown → Match `StartRound`.

### Match completed

- Authoritative match end (phase/result sync or score service) builds `MatchResultInfo` → `NotifyMatchCompleted`.
- Prefer **Infrastructure match end path** over HUD ViewModel to avoid presentation owning flow.

### Return home

- Any phase: `ReturnHome()` fail-closed; ports Exit; Home port enters hub worker.

---

## 8. Error handling

| Failure | Behavior |
|---------|----------|
| Illegal `IAppFlow` call for current phase | No-op or ignore (controller already guards); log in debug |
| Boot/auth failure | Side phase Login / NoConnection via flow |
| Maintenance during MM | Cancel queue; Maintenance or Home via flow |
| Matchmaking fail | `ReturnHome` (or re-home after toast) |
| Intro/Countdown exception | Ports already fail-open to next notify; keep that |
| Match disconnect | Existing network handlers → `ReturnHome` or Results per current product rules; must call `IAppFlow`, not raw MainMenu state from random services long-term (`GameStarter` included in cutover list) |

---

## 9. Testing strategy

1. **Unit (EditMode):** `AppFlowController` legal transitions; policies; port fakes; `FlowMatchInfoFactory`.
2. **Migration tests:**  
   - UI/ViewModel tests: Play calls `IAppFlow.RequestPlay` (mock), never `IGameStateManager`.  
   - Matchmaking notify path: match found → `NotifyMatchResolved` invoked.  
   - Match end → `NotifyMatchCompleted`.
3. **Compile gates:** Presentation must not need `Infrastructure.States` for navigation (prefer removing those usings from ViewModels).
4. **Manual / PlayMode (checklist):** cold boot → home → play → bot fill → intro → countdown → match start → results → play again / home.
5. **Regression:** existing EditMode suite still builds; fix any tests that asserted `ChangeState` as public API.

---

## 10. Implementation phases (delete gates)

### Phase 0 — Inventory freeze

- List every product `ChangeState` site (Presentation, States, GameStarter, Bootstrapper).  
- Confirm port completeness vs Null.

### Phase 1 — DI + cold boot

- Register `IAppFlow` + real ports in `RootLifetimeScope`.  
- `GameBootstrapper` → `StartColdBoot()`.  
- Implement Splash/Boot ports mapping existing bootstrap behavior (no behavior regression on auth/RC).  
- **Gate:** App launches to Home (or Login side phase) via flow; no Null boot in production graph.

### Phase 2 — UI intents

- Lobby Play → `RequestPlay`.  
- Results / cancel / home buttons → flow intents.  
- Soft popup checks → `CanShowSoftPopup`.  
- **Gate:** Presentation has zero `ChangeState` / zero nav via `IGameStateManager`.

### Phase 3 — State notify rewire

- Matchmaking → `NotifyMatchResolved` / cancel paths.  
- Match end → `NotifyMatchCompleted` (move off HUD if needed).  
- Side states complete through flow.  
- `GameStarter` / session return paths use `ReturnHome` where they currently `ChangeState<MainMenuState>`.  
- **Gate:** No product-phase `ChangeState` outside Flow ports and state-machine internals.

### Phase 4 — Intro + Countdown product path

- Ensure resolve goes Intro → Countdown → StartRound (not MM→Gameplay).  
- Verify screens/templates exist (Resources already partially present).  
- **Gate:** One full queue-to-GO path observed; StartRound only after countdown.

### Phase 5 — Delete dual public API + docs

- Remove dead dual helpers; update `wiki/GameFlow-SDK.md`, `wiki/Technical.md`, CLAUDE navigation snippets.  
- Grep gate: no Presentation `IGameStateManager` nav; production DI has no Null intro/countdown/boot.  
- **Gate:** Design “delete” list satisfied; builds green.

### Phase 6 — Hardening

- Analytics events smoke; failure paths; test coverage for new notify paths.  
- Optional follow-on ticket: absorb workers into ports and delete `IGameStateManager` (out of band).

---

## 11. Success criteria

Migration is **complete** when:

1. `IAppFlow` is registered and is the only public product navigator.  
2. Cold boot and PLAY loop run through AppFlowController phases including Intro + Countdown.  
3. Presentation does not inject `IGameStateManager` for navigation.  
4. Matchmaking and match-end report through `Notify*` APIs.  
5. Production DI does not use Null ports for Splash/Boot/Intro/Countdown/Home/MM/Match/Results.  
6. Wiki matches code.  
7. Domain/Application/Playcenter.GameFlow/Infrastructure/Presentation/Composition build with 0 errors.  
8. New/updated EditMode tests cover flow intents and notify rewires.

---

## 12. Risks and mitigations

| Risk | Mitigation |
|------|------------|
| Boot regression (auth/RC order) | Port wraps existing Bootstrap/SessionLoading sequence; keep order; feature-flag only if needed (prefer no flag — full cutover) |
| Double navigation (state + flow) | Phase order: DI first, then strip state self-transitions before enabling intro |
| Circular DI (ports need IAppFlow) | `AppFlowProxy` + factory registration |
| HUD owns match end | Move notify to Infrastructure match-end evaluator / GameplayState |
| Scope creep into v2 combat | Explicit out-of-scope; do not block migration on maps/combat |

---

## 13. Non-goals

- Deleting `IGameStateManager` entirely in this effort.  
- Implementing full Kitchen Brawler combat/maps.  
- Changing NGO/EOS contracts.  
- Visual redesign of Home beyond wiring existing screens.

---

## 14. Open points resolved by this design

| Question | Resolution |
|----------|------------|
| GameFlow-only vs delete state machine entirely | GameFlow public API + keep workers internal |
| Include v2 vertical slice? | No |
| Intro/Countdown required? | Yes for production path |
| Parallel forever? | No — delete dual **public** navigation |

---

## 15. Next step

Implementation plan via **writing-plans** skill: task-level checklist under `docs/superpowers/plans/2026-07-13-gameflow-production-migration.md`, then execute phases 0–5 with commits per phase gate.
