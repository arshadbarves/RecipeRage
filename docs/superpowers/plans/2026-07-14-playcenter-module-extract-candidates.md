> **Superseded for shell cleanliness:** For dependency laws, Presentation purity, and Infrastructure splits, follow `docs/superpowers/specs/2026-07-14-architecture-hardening-design.md` and `docs/superpowers/plans/2026-07-14-architecture-hardening.md`.
>
> **Superseded for Shell timing:** `Playcenter.Shell` (EventBus + Logging + Connectivity) was extracted and hard-cutover completed — see `docs/superpowers/specs/2026-07-14-playcenter-shell-extract-design.md` and `docs/superpowers/plans/2026-07-14-playcenter-shell-extract.md`.
>
> **Superseded for Services timing:** `Playcenter.Services` (config/analytics/ads/IAP/auth/encryption/maintenance contracts) was extracted and hard-cutover completed — see `docs/superpowers/specs/2026-07-14-playcenter-services-extract-design.md`. Remaining candidates (Audio, Async, Platform, UI) stay deferred (Unity-bound).

# Playcenter Module Extract Candidates — Decision Plan

> **For agentic workers:** This is a **decision + deferred-extract** plan, not an implement-now plan.
> Do **not** create new Playcenter assemblies unless the user explicitly greenlights a Phase-2 extract
> after a second title is real. REQUIRED if implementing later: superpowers:subagent-driven-development.

**Goal:** Decide whether any other gameplay/product logic should be extracted into reusable modules the same way GameFlow was — and if so, what, when, and how.

**Architecture:** Mirror GameFlow: engine-free (or Unity-thin) assembly under `Assets/Playcenter/<Module>/`, ports for game work, policies for product rules, **adapters stay in KitchenClash**. No extract for extract’s sake (YAGNI).

**Tech Stack:** Unity 6, VContainer, existing KitchenClash Domain/Application/Infrastructure, Playcenter.GameFlow

## Global Constraints

- **Second game does not exist yet** → default is **do not extract**.
- GameFlow is the only proven extract; keep its rules: zero KitchenClash refs, ports, fail-closed policies.
- Do not move cooking, chefs, recipes, bots, NGO, EOS concrete, maps into Playcenter.
- Do not reintroduce `IGameStateManager`.
- Domain already holds many ports (`IEventBus`, `ILoggingService`, …) — that is enough until multi-title reuse is real.
- Prefer **interface hygiene in Domain** over new assemblies when reuse is theoretical.

---

## Executive decision (production-grade)

| Question | Answer |
|----------|--------|
| Extract more modules **now**? | **No.** |
| Is GameFlow-style extract needed for other systems today? | **No** — Domain ports + Infrastructure adapters already separate “what” from “how”. |
| When extract? | When a **second Brawl-class title** needs the same shell, or when a subsystem is proven portable and painful to copy. |
| What would extract first then? | `Playcenter.Shell` (EventBus + Logging + Connectivity contracts) — not economy, matchmaking, cooking. |

**Why GameFlow was special (and others are not):**

1. It owns **legal product transitions** shared by any Brawl shell (Splash→…→Results).
2. It had a **dual navigator** problem (IAppFlow vs IGameStateManager) that forced a clean public API.
3. It is **policy-heavy** (AlwaysResolve, SoftPopup, RememberedQueue) with zero cooking types.
4. Other “generic” services are already **ports in Domain** with one game’s adapters — extracting them now only moves files and breaks asmdef graphs for zero second consumer.

---

## Current module map

```
Assets/Playcenter/
  GameFlow/          ✅ Shipped (IAppFlow, ports, policies)
  Shell/             ✅ Shipped (logging, EventBus, connectivity)
  Services/          ✅ Shipped (config, analytics, ads, IAP, auth, encryption, maintenance)

Assets/_KitchenClash/
  Domain/            Cooking domain + game events (shell/services ports moved to Playcenter)
  Application/       Use-case interfaces + pure services (economy, match, remote-config orchestration)
  Infrastructure/    EOS/Firebase/NGO/Flow handlers/adapters implementing Playcenter ports
  Presentation/      UI Toolkit screens (game skins)
  Composition/       Root/Menu/Match DI
```

GameFlow pattern (keep for any future module):

```
Playcenter.<X>          → contracts + pure controller/policies
KitchenClash.Infra.*    → port adapters + handlers
KitchenClash.Presentation → screens that call IAppFlow / shell APIs only
```

---

## Candidate inventory

### A) Already modular enough — **do not extract**

| Area | Where it lives | Why keep in game |
|------|----------------|------------------|
| **Cooking / match** | Order, Score, Hazard, Ability, Stations, Recipes | Core IP; second game won’t share |
| **Chefs / characters** | ChefId, ChefRegistry, ICharacterService | Game roster |
| **Bots** | BotManager, BotTaskPlanner | Kitchen AI |
| **NGO / net objects** | PlayerController, spawners, pool | Transport + game objects |
| **EOS concrete** | Infrastructure/EOS/* | Backend choice per title |
| **Economy / trophies / streak** | EconomyService, currencies, chef unlocks | Reward tables are game-specific |
| **Matchmaking service** | IMatchmakingService + EOS + bot fill | Queue rules + bot fill are game ports of GameFlow |
| **Lobby / team / friends** | ILobbyManager, ITeamManager | Session shape is game-specific |
| **Flow handlers** | Infrastructure/Flow/Handlers/* | RecipeRage phase work (already correct place) |
| **UI screens / UXML** | Presentation + Resources/UI | Product skin |

### B) Generic ports — **Domain is enough until game #2** (option D)

These look “extractable” but are **already interfaces** with game adapters. Moving them to `Playcenter.Core` today is churn without a second consumer.

| Port | Interface location | Impl location | Extract now? |
|------|-------------------|---------------|--------------|
| Event bus | `Domain/Interfaces/IEventBus.cs` | `Application/Services/EventBus.cs` | **No** |
| Logging | `Domain/Interfaces/ILoggingService.cs` | `Infrastructure/Logging/*` + `GameLogger` | **No** (just wired via LoggingBootstrap) |
| Connectivity | `Domain` + `IConnectivityService` | `NetworkConnectivityService` | **No** |
| Analytics | `IAnalyticsService` | Firebase adapter | **No** |
| Remote config | `IConfigService` / remote config | Composite/Firebase | **No** |
| Ads / IAP | `IAdsService`, `IIAPService` | stubs/adapters | **No** |
| UI stack | `Application/Services/IUIService.cs` | `Presentation/Common/UIService.cs` | **No** (Unity UI Toolkit–bound) |
| Auth | `IAuthService` | EOS/Firebase auth | **No** (interface generic; impl not) |
| Maintenance | `IMaintenanceService` | game gate | **No** (trivial) |

### C) Future extract shortlist (only when second title is real)

| Future module | Contents | Effort | Trigger |
|---------------|----------|--------|---------|
| **`Playcenter.Shell`** (or `Playcenter.Core`) | `IEventBus` + `EventBus`, `ILoggingService` + facade contract, `IConnectivityService` + state enum, optional thin `IAnalyticsService` / `IRemoteConfigService` **interfaces only** | M | Second game shares shell |
| **`Playcenter.UI` (optional)** | `IUIService` + screen stack **contracts** only; no UXML | L | Second game uses same UI Toolkit stack pattern |
| **`Playcenter.Session` (optional, high risk)** | Abstract session/lobby ports without mode/map names | L | Only if lobby model is truly shared |

**Never** put in Playcenter: Economy with chef SKUs, Matchmaking with bot spawner, Recipe/Order/Score.

---

## Comparison: GameFlow vs “extract EventBus”

| Criterion | GameFlow | EventBus / Logging / Connectivity |
|-----------|----------|-----------------------------------|
| Product-defining state machine | Yes | No |
| Dual-API pain forcing cutover | Yes (SM purge) | No |
| Second consumer today | No, but unique shell | No |
| Already clean ports in Domain | N/A (new module) | Yes |
| Risk of wrong boundary | Medium (done) | High (Domain split pain) |
| ROI now | High (shipped) | **Near zero** |

---

## What to do **now** (hygiene only — no new modules)

These improve readiness for a future extract **without** creating assemblies.

### Task 1: Document the decision (this plan + wiki)

**Files:**
- Create: `docs/superpowers/plans/2026-07-14-playcenter-module-extract-candidates.md` (this file)
- Modify: `wiki/GameFlow-SDK.md` or `wiki/Technical.md` — short “Playcenter modules” section
- Modify: `wiki/log.md`

- [x] **Step 1: Add wiki “Playcenter modules” section** (`wiki/Technical.md`)
- [x] **Step 2: wiki/log entry**
- [x] **Step 3: Commit docs only**
### Task 2: Optional Domain namespace hygiene (only if touching Domain soon)

**Do not do as a standalone mega-refactor.** If editing Domain files anyway:

- Prefer folders/namespaces:
  - `KitchenClash.Domain` — game models (Chef, Recipe, Order, …)
  - Keep generic ports where they are **or** group under `KitchenClash.Domain.Shell` **without** a new asmdef
- **Do not** split `KitchenClash.Domain.asmdef` until extract day.

### Task 3: Explicit non-goals (agents must not “helpfully” extract)

Agents **must not**:

1. Create `Assets/Playcenter/Shell` / `Core` / `UI` without user approval.
2. Move `EventBus`, `UIService`, `SessionManager` into Playcenter “for cleanliness”.
3. Extract EOS wrappers into Playcenter (backend ≠ product shell).
4. Put Flow **handlers** into GameFlow (handlers are game-specific).

---

## Deferred implementation sketch (game #2 only)

When a second title is greenlit, extract **one** module first:

### Future Task F1: `Playcenter.Shell` — **SHIPPED** (2026-07-14)

Implemented early (user-directed full cutover) via:
- Spec: `docs/superpowers/specs/2026-07-14-playcenter-shell-extract-design.md`
- Plan: `docs/superpowers/plans/2026-07-14-playcenter-shell-extract.md`
- Module: `Assets/Playcenter/Shell` (`noEngineReferences`, zero KitchenClash refs)
- Hard cutover: Domain/Application originals deleted; `GameLogger` fail-closed

### Future Task F2: `Playcenter.Services` — **SHIPPED** (2026-07-14)

User-directed multi-title service contracts extract + hard cutover:
- Spec: `docs/superpowers/specs/2026-07-14-playcenter-services-extract-design.md`
- Module: `Assets/Playcenter/Services` (`noEngineReferences`, zero KitchenClash refs)
- Hard cutover: Domain/Application originals deleted; consumers use `Playcenter.Services`
- Commits: `3de46c19` (module), `06006678` (cutover)

**Remaining for game #2 only:** UPM/git package extract if a second title needs the same binaries.

---

## Verification (post Services cutover)

| Check | Expected |
|-------|----------|
| `Assets/Playcenter/*` | `GameFlow` + `Shell` + `Services` |
| Product navigation | Still `IAppFlow` only |
| GameFlow/Shell → Services | No references |
| Domain originals for moved ports | Deleted |
| Builds | Domain → Application → Infrastructure → Composition green |

---

## Self-review

1. **Spec coverage:** User asked multi-title reuse extracts → GameFlow + Shell + Services shipped; Unity-bound leaves deferred.
2. **Placeholders:** None for shipped modules; Audio/UI/Platform remain deferred by design.
3. **YAGNI:** Did not extract cooking/economy/EOS concrete.
4. **Consistency:** Same hard-cutover rules as Shell (no dual APIs).

---

## Summary for humans

- **Shipped Playcenter modules:** `GameFlow` + `Shell` + `Services`.
- **Still game ports/leaves (Unity-bound):** UI service, Audio, Async, Platform; remote-config UniTask orchestration.
- **Keep forever in game:** cooking, chefs, bots, economy tables, EOS/NGO adapters, matchmaking service, flow handlers.
- **Do not extract next** unless a second title needs Unity-bound leaves as packages.
