# Architecture Hardening — GameFlow-Quality Systems

**Date:** 2026-07-14  
**Branch:** `architecture-cleanup`  
**Status:** Design locked under autopilot (user unavailable; decisions below are provisional product intent)  
**Related:** GameFlow hard purge (complete), logging fix (complete), `wiki/Technical.md`, `docs/superpowers/plans/2026-07-14-playcenter-module-extract-candidates.md` (superseded for shell systems)

---

## 1. Problem

GameFlow fixed **product navigation**. The rest of the shell and match stack is still messy:

| Symptom | Evidence |
|---------|----------|
| Clean Architecture inverted | `KitchenClash.Presentation.asmdef` references `KitchenClash.Infrastructure`; **19** Presentation files `using KitchenClash.Infrastructure.*` |
| Session facade in wrong layer | `ISessionContext` lives in `Infrastructure/DI` but is the primary UI session API |
| Application leaks vendor types | `ILobbyManager` uses `Epic.OnlineServices.Result`; `ITeamManager` / `IPlayerManager` take `PlayEveryWare...Lobby` |
| Mega-Infrastructure assembly | One asmdef owns Network + EOS + Flow + Gameplay + Audio + Localization + Persistence — no compile-time walls |
| God files | `PlayerController` ~995, `EOSLobbyService` ~877, `EOSMatchmakingService` ~606, `UIService` ~523, `CookingStation` ~534 |
| Application → Infrastructure | `BotTaskPlanner` and similar sit in Application but depend on Network/Gameplay types |
| Wiki vs intent | Extract policy deferred Shell/MM; product now wants **robust separate systems like GameFlow** |

**Goal:** Production-grade, GameFlow-quality boundaries for the product shell and match surface — **enforceable by assembly references**, not documentation alone. Get rid of wrong-direction deps and vendor leaks once each phase’s delete gate passes.

---

## 2. Assumptions (locked under autopilot)

1. **Priority order:** Session/Matchmaking/Lobby shell → UI navigation purity → Infrastructure assembly splits → Match gameplay ports. Full multi-system program, phased.
2. **In-repo modules, not UPM:** Prefer `Assets/_KitchenClash/*` assembly splits + Domain/Application ports. New `Assets/Playcenter/*` only when a subsystem is engine-free **and** has a clear second consumer or legal-transition role (GameFlow pattern). Do **not** extract cooking/economy IP into Playcenter.
3. **GameFlow stays as-is:** `IAppFlow` remains sole product navigator. This program does not re-open navigation.
4. **No big-bang rewrite:** Each phase has a compile gate + delete gate. Ship-safe incremental hardening.
5. **Out of scope for this program:** Map content, combat vertical slice, untracked WIP, `gh` PR auth, new gameplay features.
6. **Wiki drift:** Update `wiki/Technical.md` extract policy to match this design (option B — update wiki).

---

## 3. Approaches considered

### A — Dependency-law first, then ports, then assembly splits (recommended)

1. Move session/UI-facing contracts into Application/Domain.  
2. Strip Presentation → Infrastructure references until asmdef can drop Infra.  
3. De-EOS Application interfaces.  
4. Split Infrastructure into focused asmdefs.  
5. Port match HUD/gameplay behind Application facades; shrink god files.

- **Pros:** Each step is verifiable; matches GameFlow “ports + adapters”; compile-time enforcement arrives early.  
- **Cons:** Multi-phase; intermediate commits still messy until gates pass.

### B — Full Playcenter.Shell / Matchmaking / UI modules now

Extract engine-free shells into `Assets/Playcenter/*` like GameFlow.

- **Pros:** Strongest brand of “systems like GameFlow.”  
- **Cons:** High churn; Session/MM still EOS-shaped; premature UPM-style packaging for a single title.

### C — God-file refactors only (no assembly laws)

Split `PlayerController` / EOS services in place.

- **Pros:** Local readability wins.  
- **Cons:** Does not fix inverted deps; mess returns; rejected as insufficient.

**Decision: Approach A.**

---

## 4. Target architecture

```
Presentation (UI Toolkit, ViewModels)
        │  Application + Domain + Playcenter.GameFlow only
        ▼
Application (ports, use cases, session facade, pure services)
        │  Domain only (+ UniTask/VContainer as today)
        ▼
Domain (models, shell ports, cooking models — may later split Kernel vs Game)
        ▲
Infrastructure.* adapters (EOS, NGO, Flow, Persistence, …)
        │
Composition (Root/Menu/Match LifetimeScopes) wires everything
```

### Dependency laws (non-negotiable end state)

| From → To | Allowed? |
|-----------|----------|
| Presentation → Application, Domain, GameFlow | Yes |
| Presentation → Infrastructure.* | **No** |
| Application → Domain | Yes |
| Application → Epic/PlayEveryWare/NGO | **No** |
| Application → Infrastructure | **No** |
| Infrastructure → Application, Domain, GameFlow | Yes |
| Composition → all | Yes (composition root) |

### Session shell (Phase 1 target)

```
UI / ViewModels
    → ISessionContext (Application)     // facade over session-scoped services
    → IMatchmakingService, ILobbyManager, IFriendsService, …
    → IAppFlow for navigation intents

Infrastructure.EOS / Network
    → implement Application ports (no EOS types on port surface)
```

`ISessionContext` moves from `KitchenClash.Infrastructure.DI` → `KitchenClash.Application` (or `Application.Services`). Implementation may stay in Infrastructure or Composition; Presentation only sees the interface.

Concrete types on the facade today (`EconomyService`, `PlayerDataService`) become interfaces only (`IEconomyService`, `IPlayerDataService`).

### Match surface (later phase)

```
GameplayHudViewModel
    → IMatchHudPort / IMatchContext (Application) exposing orders, scores, local player view models
    NOT OrderManager, PlayerController concrete NetworkBehaviours
```

---

## 5. Phased program

### Phase 1 — Session shell + dependency law foundation

**Intent:** Make Presentation compile without Infrastructure; de-vendor Application lobby/team ports.

| Work item | Detail |
|-----------|--------|
| 1.1 Move `ISessionContext` | Interface → Application; keep `SessionContext` impl in Infrastructure/Composition |
| 1.2 Facade interfaces only | `IEconomyService` / `IPlayerDataService` on context; drop concrete service types from facade |
| 1.3 Remove dead Infra usings | Many Presentation files import EOS/Network but only need `ISessionContext` |
| 1.4 Relocate misplaced Application ports | `ILocalizationManager` already Application — fix usings to Application, not Infrastructure.Localization |
| 1.5 De-EOS `ILobbyManager` | Replace `Epic.OnlineServices.Result` with Domain/Application result type (e.g. `LobbyOpResult`) |
| 1.6 De-EOS team/player ports | `UpdateTeamsFromLobby` / `SetCurrentLobby` take Domain DTOs, not PlayEveryWare `Lobby` |
| 1.7 Drop Presentation → Infrastructure asmdef ref | **Delete gate** when zero `using KitchenClash.Infrastructure` remain |
| 1.8 Application.asmdef | Remove `com.Epic.OnlineServices` / playeveryware refs when ports are clean |

**Success criteria:**

- `KitchenClash.Presentation.asmdef` does **not** reference Infrastructure or Netcode (unless a temporary exception is documented and ticketed — prefer zero).  
- `KitchenClash.Application.asmdef` does **not** reference EOS packages.  
- Lobby/MM UI still works via ports + `IAppFlow`.  
- EditMode tests for ports/result types green.

### Phase 2 — UI navigation purity

| Work item | Detail |
|-----------|--------|
| 2.1 `IUIService` surface audit | Ensure all screens open via Application `IUIService` only |
| 2.2 Split `UIService` | Stack manager already separate; extract transition/animation behind `IUITransitionPort` in Application if Presentation still needs Infra.Animation |
| 2.3 Animation/Localization adapters | Presentation uses Application ports; Infrastructure implements |
| 2.4 Firebase maintenance | Maintenance screen uses Application connectivity/config ports only |

**Success criteria:** UIService responsibilities documented; no Presentation file imports Infrastructure; god-file line count for UIService reduced or justified.

### Phase 3 — Infrastructure assembly walls

Split mega-asmdef into focused assemblies (names illustrative):

| Assembly | Owns |
|----------|------|
| `KitchenClash.Infrastructure.Flow` | GameFlow ports/handlers |
| `KitchenClash.Infrastructure.EOS` | Auth, lobby, matchmaking, friends adapters |
| `KitchenClash.Infrastructure.Network` | NGO, PlayerController, spawn, cooking net |
| `KitchenClash.Infrastructure.Persistence` | Player data storage |
| `KitchenClash.Infrastructure.Platform` | Firebase, localization impl, animation, audio |

Composition references all. Cross-infra deps only via Application ports where possible.

**Success criteria:** Network cannot reference EOS UI helpers accidentally; Flow stays thin; build still green.

### Phase 4 — Match gameplay ports + god-file shrink

| Work item | Detail |
|-----------|--------|
| 4.1 `IMatchContext` / HUD ports in Application | Orders, scores, timer, local chef state as DTOs/events |
| 4.2 `GameplayHudViewModel` | Depends only on Application match ports |
| 4.3 `PlayerController` split | Input / carry / interact / net sync partials or collaborators |
| 4.4 `BotTaskPlanner` | Move to Infrastructure or depend only on Application kitchen ports |
| 4.5 Cooking station net | Ports for station state; shrink `CookingStation` |

**Success criteria:** Presentation has zero Network usings; PlayerController primary file &lt; ~400 lines or split into named collaborators; BotTaskPlanner not in Application with Infra deps.

### Phase 5 — Domain kernel hygiene (optional follow-on)

Split Domain into shell ports vs cooking models only if Phase 1–4 still leave Domain noisy. Not required to claim “hardened shell.”

---

## 6. What we will **not** do

- Extract EventBus/Logging/Config into Playcenter without a second title (logging already fixed via `LoggingBootstrap`).  
- UPM-package GameFlow or other modules in this program.  
- Rewrite EOS matchmaking algorithm or NGO cooking net protocol.  
- Delete working features to force purity — adapt behind ports first, then delete vendor types from public surfaces.

---

## 7. Error handling & testing

- Port result types: explicit success/failure enums/structs (no silent `Result` from EOS on Application surface).  
- Unit tests: Domain/Application result mapping; session facade resolve behavior with fakes.  
- Compile gates: asmdef reference removal is the primary gate (stronger than grep).  
- Manual Unity smoke after Phase 1 and Phase 4: boot → login → home → play → MM → match HUD → results.

---

## 8. Wiki / docs updates (with Phase 1)

- `wiki/Technical.md` — replace deferred extract policy with this phased hardening program.  
- `wiki/log.md` — note design + policy update.  
- Supersede guidance in `docs/superpowers/plans/2026-07-14-playcenter-module-extract-candidates.md` with pointer to this design.  
- CLAUDE.md assembly notes if Presentation/Infrastructure rules change.

---

## 9. Risk register

| Risk | Mitigation |
|------|------------|
| Presentation still needs a Network type for HUD | Introduce Application DTO/port before dropping asmdef ref; temporary `[Obsolete]` bridge only inside Infrastructure |
| Session resolve returns null outside session | Keep existing null-safe patterns; document lifecycle |
| Large EOS service files | Phase 1 only changes port surfaces; god-file split is Phase 4 / EOS-local later |
| Asmdef split breaks Unity meta/GUID | Phase 3 careful; one assembly at a time |

---

## 10. Definition of done (program)

1. Presentation → Infrastructure reference **gone**.  
2. Application → EOS package references **gone**.  
3. Session/MM/Lobby UI depends on Application ports + `IAppFlow` only.  
4. Infrastructure split into ≥3 focused assemblies (Flow, EOS, Network minimum).  
5. Match HUD does not reference `PlayerController` / `OrderManager` concretes.  
6. Wiki + CLAUDE match code.  
7. EditMode tests green; Unity smoke checklist signed off.

Phase 1 alone is a valuable shippable milestone (dependency law foundation).

---

## 11. Immediate next step

Write implementation plan: `docs/superpowers/plans/2026-07-14-architecture-hardening.md` focused on **Phase 1** with TDD-style tasks, then execute Phase 1.
