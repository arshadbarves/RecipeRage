# Playcenter Shared-Stack Architecture — Design

**Date:** 2026-07-16  
**Branch:** `architecture-cleanup`  
**Status:** Proposed (awaiting user review of this written spec)  
**Supersedes (partially):** `2026-07-15-playcenter-foundation-extract-design.md` §2.2 engine-free-only rule for *implementation* modules; ports-only UI/Services remain valid for pure contracts  
**Related:** GameFlow / Shell / Services / UI already shipped under `Assets/Playcenter/`

---

## 1. Problem

Playcenter today is **mostly ports + pure logic**:

| Module | What it has | Gap |
|--------|-------------|-----|
| **GameFlow** | Real controller + policies | Good reference for “logic in module” |
| **Shell** | Real `EventBus` + `GameLogger` facade | Good; others cannot officially depend on it yet |
| **Services** | Interfaces only | Correct for engine-free contracts; adapters stay game-side |
| **UI** | Interfaces + enums only | Stack manager, BaseUIScreen, UIService (~700 LOC) still KitchenClash |

You will ship **multiple titles on the same stack**:

- Unity 6 + **UI Toolkit**
- **DOTween** for animation
- **EOS** (PlayEveryWare) for auth / lobby / storage / friends (where used)
- Same product shell patterns (boot → home → matchmaking → match → results)

**Ports-only is correct for contracts.** It is **not** enough for multi-title reuse of *shared implementation*. Without Unity-thin shared modules, every new title re-copies UIService, DOTween wrappers, and EOS glue — the mess you want to escape.

**Also required:** modules may **interlink in a restricted DAG** (e.g. everyone may use Shell logging). Free-for-all mesh is forbidden.

---

## 2. Locked assumptions (autopilot defaults)

These are design defaults. Change them only by revising this spec.

| # | Assumption | Rationale |
|---|------------|-----------|
| A1 | **All studio titles share Unity 6 + UI Toolkit + DOTween + VContainer + UniTask** | Your stated stack |
| A2 | **EOS is the default online backend** for titles that need online services; not every title must use every EOS surface | Auth/storage/lobby are multi-title; match rules stay game |
| A3 | **Two Playcenter tiers:** pure (`noEngineReferences`) + Unity-thin (engine + plugins allowed) | Keeps GameFlow/Shell testable without Unity; allows real shared UI/animation/EOS logic |
| A4 | **Restricted dependency DAG only** — no cycles, no peer mesh | Logger usable everywhere; UI ⟂ Services; GameFlow does not own UI |
| A5 | **Hard cutover** when promoting code — no dual namespaces, aliases, obsolete stubs, Console fallbacks | Same bar as prior extracts |
| A6 | **Game IP stays in KitchenClash** — cooking, chefs, maps, NGO match rules, title UXML/USS, title sound tables | Playcenter is product shell + shared tech, not game content |
| A7 | **VContainer / UniTask stay out of pure modules**; Unity-thin modules may use UniTask + Unity APIs; DI registration stays game Composition (or a thin Playcenter.Unity.DI helper later — **out of scope this program**) | Avoid coupling pure product flow to a DI container |
| A8 | **Obsolete `EOSAuthService` is deleted** during EOS extract (already marked obsolete; `AuthenticationService` is the live path) | No legacy dual auth |

---

## 3. Approaches considered

### Approach A — Stay ports-only (status quo)

Keep pure contracts; all UI Toolkit / DOTween / EOS logic stays per-game.

| Pros | Cons |
|------|------|
| Zero Unity in Playcenter | Every title reimplements UIService + animators + EOS |
| Simple DAG | Does not fix “messy architecture / no logic in modules” |
| Already shipped | Contradicts multi-title reuse goal |

**Reject** for this program.

### Approach B — One mega `Playcenter.Unity` assembly

Dump UI Toolkit host, DOTween, EOS, platform helpers into one Unity-referenced module.

| Pros | Cons |
|------|------|
| Fast to move code | Unrestricted internal coupling; hard to test slices |
| One reference for games | Forces EOS on titles that only need UI |
| | Becomes a junk drawer |

**Reject** — violates restricted interlink and multi-title optionality.

### Approach C — Layered pure + Unity-thin modules with explicit DAG (**recommended**)

Keep pure modules (GameFlow, Shell, Services ports, UI ports). Add **optional Unity-thin** modules that hold shared *implementations* for the shared stack. Dependencies only **down** the DAG.

| Pros | Cons |
|------|------|
| Real reusable logic (GameFlow-quality) | More asmdefs to maintain |
| Optional modules (title without EOS skips EOS module) | Must carefully split pure vs Unity-thin |
| Restricted interlinks documented and enforceable | Larger first extract than ports-only |
| Matches “logger usable by others, not free mesh” | |

**Choose Approach C.**

---

## 4. Architecture

### 4.1 Two tiers

```
┌─────────────────────────────────────────────────────────────┐
│  TIER 0 — Pure (noEngineReferences: true)                   │
│  Playcenter.Shell  ·  Playcenter.GameFlow                   │
│  Playcenter.Services (ports)  ·  Playcenter.UI (ports)      │
│  Optional later: Playcenter.Animation.Abstractions (ports)  │
└─────────────────────────────────────────────────────────────┘
                              ▲
                              │ references only downward
┌─────────────────────────────────────────────────────────────┐
│  TIER 1 — Unity-thin shared (engine + plugins OK)           │
│  Playcenter.UI.Toolkit   — stack host, BaseUIScreen core    │
│  Playcenter.Animation    — DOTween UI/Transform adapters    │
│  Playcenter.EOS          — shared EOS adapters for ports    │
│  (future) Playcenter.Unity.Logging — optional; today game   │
└─────────────────────────────────────────────────────────────┘
                              ▲
                              │
┌─────────────────────────────────────────────────────────────┐
│  GAME — KitchenClash (and future titles)                    │
│  Composition, Flow handlers, screens, cooking IP, NGO,      │
│  title UXML/USS, title config, match rules                  │
└─────────────────────────────────────────────────────────────┘
```

### 4.2 Restricted dependency DAG (allowed edges only)

```
                    ┌──────────────┐
                    │ Shell (L0)   │  logging, EventBus, connectivity ports
                    └──────▲───────┘
           ┌───────────────┼────────────────┐
           │               │                │
    ┌──────┴──────┐ ┌──────┴──────┐  ┌──────┴──────┐
    │  GameFlow   │ │  Services   │  │  UI (ports) │
    │  (pure)     │ │  (ports)    │  │  (pure)     │
    └──────▲──────┘ └──────▲──────┘  └──────▲──────┘
           │               │                │
           │        ┌──────┴──────┐  ┌──────┴──────────┐
           │        │ Playcenter  │  │ Playcenter.UI   │
           │        │ .EOS (T1)   │  │ .Toolkit (T1)   │
           │        └──────▲──────┘  └──────▲──────────┘
           │               │                │
           │        ┌──────┴────────────────┴──────┐
           │        │ Playcenter.Animation (T1)    │
           │        │  → Shell only (+ DOTween)    │
           │        └──────────────▲───────────────┘
           │                       │  UI.Toolkit may → Animation
           └───────────────────────┴── KitchenClash.* / future titles
```

### 4.3 Forbidden edges (hard rules)

| From → To | Why forbidden |
|-----------|----------------|
| Shell → GameFlow / Services / UI / any T1 | Shell is foundation; no upward deps |
| GameFlow → UI / Services / Animation / EOS | Flow stays pure; talks via **ports implemented by the game** |
| UI (ports) → Services | Screens must not pull product services through UI module |
| Services → UI | Services are not a UI layer |
| UI.Toolkit → Services / GameFlow / EOS | Toolkit host is presentation infrastructure only |
| Animation → GameFlow / Services / EOS | Animation is a leaf utility |
| EOS → UI / UI.Toolkit / Animation / GameFlow | EOS implements Services ports + game session ports; not UI |
| Any Playcenter → KitchenClash | Portability wall |
| Cycles of any kind | Asmdef + review gate |

### 4.4 Allowed interlinks (explicit)

| Consumer | May reference | Typical use |
|----------|---------------|-------------|
| GameFlow | Shell | **No this program** — stays zero-ref; analytics via `IFlowAnalyticsPort` only |
| Services (ports) | Shell | **No** — pure ports have no log call sites |
| UI (ports) | Shell | **No** — pure ports stay zero-ref |
| UI.Toolkit | Shell, UI (ports), Animation | Host logs via Shell; implements UI ports; transitions may use Animation |
| Animation | Shell only (plus DOTween/UniTask/Unity) | No Playcenter.UI / Services / GameFlow / EOS refs |
| EOS | Shell, Services (ports) | Implements auth/storage ports; logs via Shell |
| KitchenClash.* | Any Playcenter module it needs | Composition wires everything |

**Summary policy in one line:**  
**Only Shell is a shared dependency hub. Peer product modules (GameFlow ⟂ Services ⟂ UI) stay independent. Unity-thin modules depend downward only.**

---

## 5. Module designs

### 5.1 Unchanged pure modules (this program)

| Module | Action |
|--------|--------|
| **Playcenter.Shell** | Keep. Remains the only pure shared hub. |
| **Playcenter.GameFlow** | Keep pure. No UI/Services/EOS refs. |
| **Playcenter.Services** | Keep ports-only. New implementations may live in T1 modules (EOS) or game. |
| **Playcenter.UI** | Keep ports-only (`IUIService`, categories, notification types). |

### 5.2 NEW: `Playcenter.UI.Toolkit` (Unity-thin)

**Purpose:** Shared UI Toolkit **implementation** of the screen stack — the missing “logic” behind `IUIService`.

**Owns (promote from KitchenClash.Presentation.Common):**

| Type | Notes |
|------|-------|
| `UIScreenStackManager` + `IUIScreenStackManager` | Pure C# stack (no Unity types) — **lives in `Playcenter.UI`**, not Toolkit |
| `UIService` (partial) | Document/layer setup, navigation, screen ops — **generalized**: DI resolve via `Func<Type, object>` or small `IScreenResolver` port (no VContainer types in public API) |
| `BaseUIScreen` | Lifecycle without `[Inject]` attribute dependency — use method inject or resolver |
| `UIScreenController` | Show/hide/animate shell |
| `UIScreenRegistry` / `UIScreenAttribute` | Type discovery metadata |
| `UIScreenPriority` (Presentation numeric enum) | Layering helper |
| `UITransitionHandler` / `UITransitionType` | Shared transitions; may call Animation module |
| Toast host plumbing | Generic; title toast UXML stays game |

**Does not own:**

- Concrete screens (`HomeScreen`, `LobbyScreen`, …)
- Title UXML/USS/themes
- VContainer `LifetimeScope` registration (game Composition)
- GameFlow ports / match HUD content

**Asmdef:**

```
Playcenter.UI.Toolkit
  references: Playcenter.UI, Playcenter.Shell, Unity.UIElements (engine)
  noEngineReferences: false
  NO: Playcenter.Services, Playcenter.GameFlow, Playcenter.EOS, KitchenClash.*
```

**DI surface change (important):**

Today `UIService` takes `VContainer.IObjectResolver`. Shared module must not hard-depend on VContainer.

```csharp
// Playcenter.UI.Toolkit
public interface IScreenInstanceFactory
{
    object Create(Type screenType);
    // or: T Create<T>() where T : class;
}
```

Game registers a VContainer-backed factory. Hard cutover: KitchenClash `UIService` **moves** (not duplicated).

### 5.3 NEW: `Playcenter.Animation` (Unity-thin)

**Purpose:** Shared DOTween-backed animation service used by UI Toolkit transitions and gameplay juice.

**Owns (promote from KitchenClash.Infrastructure.Animation):**

| Type | Notes |
|------|-------|
| `IAnimationService` | Move **contract** to pure module **or** keep interface in Animation assembly public API. **Decision:** put **`IAnimationService` in `Playcenter.Animation`** (Unity-thin is OK — interface uses `VisualElement` / `Transform`). Do **not** force a pure Animation.Abstractions unless a non-Unity consumer appears. |
| `AnimationService` | Facade |
| `IUIAnimator` / `DOTweenUIAnimator` | UI Toolkit DOTween |
| `ITransformAnimator` / `DOTweenTransformAnimator` | World transforms |
| `TweenExtensions`, `SlideDirection` | Shared helpers |

**Async:** Prefer `Task` + `CancellationToken` on new public API; internal UniTask OK if isolated. **Decision for hard cutover:** public methods use **`UniTask` only if all consumers already UniTask** — KitchenClash is UniTask-heavy. **Lock:** keep **UniTask** on `IAnimationService` in Playcenter.Animation (Unity-thin + UniTask ref allowed). Document that pure modules must not reference Animation.

**Asmdef:**

```
Playcenter.Animation
  references: Playcenter.Shell, DOTween.Modules, UniTask
  noEngineReferences: false
  NO: Playcenter.UI, Playcenter.Services, Playcenter.GameFlow, Playcenter.EOS, KitchenClash.*
```

(UI.Toolkit **may** reference Animation for transitions — allowed edge Toolkit → Animation.)

Update DAG: **UI.Toolkit → Animation** is allowed; Animation does **not** reference UI.Toolkit.

### 5.4 NEW: `Playcenter.EOS` (Unity-thin)

**Purpose:** Shared EOS adapters that implement **Playcenter.Services ports** (and small EOS utilities), not KitchenClash match IP.

**Promote (shared):**

| Type | Maps to |
|------|---------|
| `AuthenticationService` | `Playcenter.Services.IAuthService` |
| `EOSCloudStorageProvider` | `ICloudStorageProvider` / storage ports |
| `EosResultMapper` | Shared result mapping |
| Thin EOS bootstrap helpers if title-agnostic | Product/client IDs still from game config |

**Stay game-side (KitchenClash.Infrastructure.EOS or session layer):**

| Type | Why |
|------|-----|
| `EOSLobbyService`, `EOSMatchmakingService` | Queue/team/format rules are product-specific (2v2/3v3 cooking) |
| `EOSTeamManager`, `EOSPlayerManager` | Match session shape |
| `EOSFriendsService` (+ factory) | Evaluate: if API is generic friends list, **promote**; if tied to KitchenClash social UX, keep. **Default:** promote **generic friends port adapter** only if an `IFriendsService` port exists or is added under Services; else keep game-side this program. |
| `EOSClientTransportConfigurator`, NGO glue | Netcode transport is game networking |
| `EOSPlayerDataService` | Title save DTO shape |
| Obsolete `EOSAuthService` | **Delete** |

**Asmdef:**

```
Playcenter.EOS
  references: Playcenter.Shell, Playcenter.Services, com.playeveryware.eos*, UniTask (as needed)
  noEngineReferences: false
  NO: Playcenter.UI*, Playcenter.GameFlow, Playcenter.Animation, KitchenClash.*
```

**Config:** EOS product/sandbox/deployment IDs injected via constructor or `IEOSConfig` interface defined in Playcenter.EOS; game supplies values from ScriptableObject / env.

### 5.5 Explicitly out of scope (this program)

| Item | Why |
|------|-----|
| Clip-based audio (`IAudioService`, SFX tables) | Title content + AudioClip |
| NGO / Netcode session stack | Game networking topology |
| Full lobby/matchmaking extract | Product rules differ per title; ports already on GameFlow |
| VContainer Playcenter module | Composition stays per-game |
| Firebase / alternate backends | Not shared stack |
| Cooking, chefs, bots, maps, economy DTOs | Game IP |
| Free-for-all module mesh | Forbidden by §4.3 |
| Re-opening pure Services/UI to hold Unity types | Ports stay pure |

---

## 6. Target tree

```
Assets/Playcenter/
  Shell/                 # unchanged pure
  GameFlow/              # unchanged pure
  Services/              # unchanged pure ports
  UI/                    # pure ports + pure stack manager types (moved)
    Runtime/
      IUIService.cs
      NotificationType.cs
      UIScreenCategory.cs
      UIScreenStackManager.cs      # NEW here (pure)
      IUIScreenStackManager.cs
  UI.Toolkit/            # NEW Unity-thin
    Runtime/
      Playcenter.UI.Toolkit.asmdef
      UIService.cs
      BaseUIScreen.cs
      UIScreenController.cs
      UIScreenRegistry.cs
      UIScreenAttribute.cs
      UIScreenPriority.cs
      UITransitionHandler.cs
      IScreenInstanceFactory.cs
      ...
  Animation/             # NEW Unity-thin
    Runtime/
      Playcenter.Animation.asmdef
      IAnimationService.cs
      AnimationService.cs
      DOTweenUIAnimator.cs
      ...
  EOS/                   # NEW Unity-thin
    Runtime/
      Playcenter.EOS.asmdef
      AuthenticationService.cs
      EOSCloudStorageProvider.cs
      EosResultMapper.cs
      IEOSConfig.cs
```

KitchenClash keeps: screens, flow handlers, composition, cooking, NGO, lobby/matchmaking EOS, title assets.

---

## 7. Hard cutover rules

1. **Move, don’t copy** — delete KitchenClash originals after consumers switch.
2. **No** type aliases, obsolete dual types, dual namespaces, or `#if PLAYCENTER` shims.
3. **No** Console logging fallback in Shell (already fail-closed).
4. Namespace renames:
   - `KitchenClash.Presentation.Common.*` (shared host) → `Playcenter.UI.Toolkit` / `Playcenter.UI`
   - `KitchenClash.Infrastructure.Animation.*` → `Playcenter.Animation`
   - Shared EOS types → `Playcenter.EOS`
5. Game screens stay `KitchenClash.Presentation.Screens` but inherit `Playcenter.UI.Toolkit.BaseUIScreen`.
6. Asmdef references updated in the same commit as moves (per module slice).
7. EditMode tests that covered moved types move with them or re-home under a Playcenter test asmdef if one exists; else keep tests in KitchenClash referencing Playcenter modules.

---

## 8. Implementation phases (for later plan — not this doc’s execution)

| Phase | Deliverable | Exit criteria |
|-------|-------------|----------------|
| **0** | Spec approved + plan written | This doc reviewed; `writing-plans` produces task plan |
| **1** | Pure stack types → `Playcenter.UI` | `UIScreenStackManager` in pure UI; tests green |
| **2** | `Playcenter.Animation` module + cutover | All `IAnimationService` consumers on Playcenter.Animation; old asmdef deleted or emptied |
| **3** | `Playcenter.UI.Toolkit` module + cutover | UIService/BaseUIScreen moved; screens compile; no VContainer types in Toolkit public API |
| **4** | `Playcenter.EOS` shared slice + cutover | Auth + cloud storage + mapper moved; obsolete EOSAuthService deleted; lobby/MM stay game |
| **5** | Docs / wiki / candidates supersession | `wiki/Technical.md`, module READMEs, dependency diagram |

Phases 2 and 3 may swap if Toolkit transitions need Animation first — **plan should do Animation before Toolkit transitions wire-up**.

---

## 9. Testing strategy

| Layer | How |
|-------|-----|
| Pure `UIScreenStackManager` | EditMode NUnit, no Unity |
| `AnimationService` | EditMode with DOTween safe mode / mock animators if existing tests; else smoke via game tests |
| `UI.Toolkit` | EditMode for stack navigation with fake `IScreenInstanceFactory`; PlayMode smoke for UIDocument host |
| `EOS` | Existing auth tests retargeted; no live EOS in CI unless already present |
| Regression | `dotnet build` Composition + Presentation + Infrastructure slices; EditMode suite |

---

## 10. Error handling & logging

- All T1 modules use **`Playcenter.Shell.GameLogger`** / `ILoggingService` — never raw `Debug.Log` in new shared code except inside a dedicated Unity logging adapter (game-owned `UnityLoggingService` stays).
- EOS failures map through `EosResultMapper` → Services result types (`AuthResult`, etc.).
- UI.Toolkit fails closed on missing UIDocument / factory (log error, no silent empty UI).

---

## 11. Success criteria

1. A second game could reference `Playcenter.*` modules and get: flow controller, logging/events, service ports, UI stack **implementation**, DOTween animation service, EOS auth/storage adapters — without copying KitchenClash.
2. Dependency graph matches §4.2–4.3 (verifiable by asmdef inspection).
3. Zero dual APIs / legacy shims after each phase cutover.
4. KitchenClash game IP unchanged in behavior (screens, match, lobby).
5. Builds: Presentation, Infrastructure, Composition, EditMode green.

---

## 12. Risks & mitigations

| Risk | Mitigation |
|------|------------|
| VContainer baked into UIService | Introduce `IScreenInstanceFactory` before move |
| BaseUIScreen `[Inject]` couples to VContainer | Replace with factory-set properties or optional inject adapter in game |
| EOS lobby accidentally promoted | Explicit stay-game list in §5.4 |
| Scope explosion (friends, NGO, audio) | Out-of-scope table; YAGNI |
| UniTask in Animation blocks pure tests | Animation is T1 only; pure modules never reference it |
| Large PR / broken mid-cutover | Phase per module; hard cutover per phase, not big-bang all three T1 modules in one commit series without green builds |

---

## 13. Decision log

| Decision | Choice |
|----------|--------|
| Architecture style | Approach C — layered pure + Unity-thin + restricted DAG |
| Shared dependency hub | Shell only |
| GameFlow → Shell | No this program (zero-ref retained) |
| UI stack pure types | Live in `Playcenter.UI` |
| UI host implementation | `Playcenter.UI.Toolkit` |
| Animation | `Playcenter.Animation` with UniTask + DOTween |
| EOS shared | Auth + cloud storage + mapper only |
| Hard cutover | Yes, always |
| Ports-only Services/UI contracts | Retained |

---

## 14. What this does *not* claim

- Does not make KitchenClash “done” architecture-wide (network mega-split, audio clips, etc. remain deferred).
- Does not extract GameFlow ports’ game handlers into Playcenter.
- Does not require every title to take EOS or Animation references.

---

**Next step after approval of this file:** invoke `writing-plans` to produce `docs/superpowers/plans/2026-07-16-playcenter-shared-stack.md` with bite-sized tasks (Animation → UI pure stack → UI.Toolkit → EOS → docs).
