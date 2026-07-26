# Architecture Cleanup Design — Docs → Dead Code → DI

**Date:** 2026-07-12  
**Status:** Approved for implementation planning  
**Track:** Architecture Cleanup (Layered Freeze-and-Clean)  
**Repo:** RecipeRage / Kitchen Clash  
**Supersedes for this track:** Ad-hoc cleanup; does not replace `wiki/GameplayDesign.md` product design

---

## 1. Context

RecipeRage is a Unity 6 multiplayer cooking-competition game moving from a stand-and-cook loop to **Kitchen Brawler v2** (combat-first, autonomous stations, KO-loot economy, Triplet A modes).

### Current reality

| Area | State |
|------|--------|
| Foundation | Solid: VContainer (Root/Session/Match), state machine, NGO+EOS, UI Toolkit, event bus, match runtime bridge |
| Kitchen Brawler v2 | Design locked in `wiki/GameplayDesign.md`; **scaffold already in tree** (`PlayerCombatController`, `LootPickup`, `AutonomousCookingStation`, `MatchWinConditionCoordinator`, mode assets/maps, archetype abilities) |
| Wiki drift | `GameplayDesign.md` still says implementation has not started |
| Working tree | Large uncommitted WIP across combat, stations, modes, auth, UI, maps |
| Roadmap drift | `PHASE_ROADMAP.md` still centers Phase 2 verification of the **legacy** cooking loop |
| Conductor | `system_registration_refactor` partially done; plan language may say `GameplayLifetimeScope` while code uses `MatchLifetimeScope` |

### Problem

Agents and humans cannot trust docs, can still extend a legacy cooking path, and DI ownership is inconsistent — while feature work is mid-flight. Cleanup must freeze product scope and restore a single trustworthy baseline.

---

## 2. Goals

1. **Truth alignment** — wiki, roadmap, `PROJECT_MEMORY`, GDD matrix describe Kitchen Brawler v2 as the active design and accurately reflect DI scopes, auth, and UI navigation.
2. **Single gameplay path** — remove legacy stand-and-cook surface where v2 already owns the same responsibility.
3. **Correct DI ownership** — Root / Session / Match boundaries match architecture rules; no wrong-scope injections; reduce avoidable singleton leaks without a full aspirational migration.
4. **Green gate** — project compiles; existing EditMode tests pass (updated only for deleted APIs).

### Success definition

> A developer (or agent) opening the repo can trust the wiki/roadmap, cannot accidentally extend the legacy cooking loop, and resolves services from the correct scope — without any new gameplay features having been added.

---

## 3. Non-Goals (Hard Boundary)

- No new Kitchen Brawler features (combat feel, mode balance, new abilities, playable vertical slice completion).
- No aspirational migrations: RouterService as production nav, zero-singleton utopia, full Google/Facebook/Apple EOS Connect.
- No art/content production beyond deleting unused assets tied to the legacy path.
- No PlayMode/runtime polish as a required gate (feature track owns playability).
- No new test frameworks or coverage crusades.

Kitchen Brawler feature work is a **later track**, planned after this cleanup.

---

## 4. Approach

**Layered Freeze-and-Clean (Approach A)** on the current dirty working tree.

| Phase | Name | Risk | Outcome |
|-------|------|------|---------|
| 0 | WIP inventory snapshot | Low | Classified map: Keep-v2 / Shared / Legacy-delete / Unknown |
| 1 | Docs truth pass | Low | Wiki + architecture docs match reality |
| 2 | Dead-code purge | Medium | Legacy cooking path gone; v2 WIP untouched |
| 3 | DI ownership pass | Medium–High | Scopes/registrations correct; wrong-scope fixed |
| 4 | Verification gate | Low | Build + EditMode tests green |

**Rejected alternatives**

- **Big-Bang Purge:** too high risk of breaking mid-flight v2 WIP; hard to review/rollback.
- **Branch-only isolation without layered order:** isolation is optional hygiene; phase order still required. Not chosen as primary approach.

**WIP policy:** Treat the current uncommitted tree as the starting state. Inventory it, **keep v2 WIP**, purge legacy **around** it. Do not require a clean tree before planning or execution.

---

## 5. Phase 0 — Inventory & Classification

### Buckets

| Bucket | Meaning | Action |
|--------|---------|--------|
| **Keep-v2** | Kitchen Brawler v2 systems | Keep; do not finish features |
| **Shared** | Infrastructure used by old and new (or only by live app shell) | Keep; may fix ownership in Phase 3 |
| **Legacy-delete** | Only serves stand-and-cook / old modes / removed v1 mechanics | Delete in Phase 2 |
| **Unknown** | Unclear or dual-use | Investigate; **default keep** until proven legacy-only |

### Classification rules

1. **v2 surface wins** — if a v2 type owns the same responsibility as an old type, the old type is Legacy-delete only when nothing in Keep-v2/Shared still references it.
2. **Reference gate** — no delete without repo-wide reference check (code, prefabs, scenes, ScriptableObjects, Resources, tests).
3. **Dual-path default** — live match-path code that is not replaced stays Shared or Unknown.
4. **Assets follow code** — orphaned prefabs/SOs/maps for deleted modes/stations go with their code; shared art stays.
5. **Tests follow production** — tests for deleted APIs are deleted or rewritten only to compile; no new coverage goals.
6. **Wiki drift is Phase 1** — docs that claim “v2 not started” while code exists are documentation bugs, not code bugs.

### Keep-v2 anchors (present at design time)

- `PlayerCombatController`, `LootPickup`, `AutonomousCookingStation`
- `MatchWinConditionCoordinator`, `IModeWinCondition` / `ModeWinConditions`
- `ArchetypeAbilities`, `ChefArchetype`, `StationPhase`, `CombatEvents`
- Mode assets: Rush Service / Hell’s Kitchen / Last Plate Standing + related map scenes
- Related prefabs under `_KitchenClash/Prefabs/Stations/AutonomousCookingStation_*`

### Legacy candidates (verify before delete; not pre-approved)

- Old game mode assets already removed in WIP (FreeForAll, RankedMode, TeamBattle, classicMode) — confirm no remaining refs
- Stand-and-cook station types/prefabs fully superseded by autonomous stations
- Combo chain / heat challenge / desperation aura / hand-off / commit-window remnants
- Dead RouterService stack remnants
- Stale analysis text treating Unity 2022 / wrong scope names / RouterService-as-current as truth (prefer update in Phase 1)

### Phase 0 deliverable

A short inventory artifact listing files per bucket with one-line rationale. Phase 2 deletes **only** from Legacy-delete.

---

## 6. Phase 1 — Docs Truth Pass

Docs only. Do not delete production code in this phase.

### Wiki (`wiki/` — design memory)

| Page | Change |
|------|--------|
| `GameplayDesign.md` | Status: **implementation in progress (scaffold present; not playable/complete)** — not “has not started” |
| `index.md`, `README.md` | Reflect v2 WIP reality |
| `Technical.md` | Unity 6; Root/Session/Match; EOS auth (not Firebase Auth as current); `UIService` as production navigation |
| `Gameplay.md`, `Characters.md` | Mark legacy vs v2 where they conflict; no silent dual-truth |
| `log.md` | Append drift-resolved / truth-pass entries per `DRIFT-PROTOCOL.md` |

### Architecture docs

| Doc | Change |
|-----|--------|
| `Documentation/Architecture/PROJECT_MEMORY.md` | Align paths/scopes with current `_KitchenClash` + Scripts layout |
| `Documentation/Architecture/PHASE_ROADMAP.md` | Reframe current work: **cleanup track → Kitchen Brawler vertical slices**; stop prioritizing legacy cooking-loop verification as the product path |
| `Documentation/Architecture/GDD_ALIGNMENT_MATRIX.md` | Add Kitchen Brawler rows (combat, autonomous stations, modes, loot) as Partial/In-progress; keep aspirational items Planned |

### Root analysis docs

Update `CODEBASE_ANALYSIS.md`, `QUICK_REFERENCE.md`, and `README.md` for stale engine/scope/nav claims, **or** explicitly demote them as secondary to wiki + `PROJECT_MEMORY`.

### Drift protocol

Intentional wiki corrections that contradict prior text use option **B** (update wiki) and are logged in `wiki/log.md`. Code is not changed to match outdated docs in this phase.

### Exit criteria

- [ ] No wiki page claims v2 “has not started”
- [ ] Roadmap no longer prioritizes verifying the legacy cooking loop as the product path
- [ ] DI / auth / UI navigation descriptions match current code
- [ ] `wiki/log.md` records the truth pass

---

## 7. Phase 2 — Dead-Code Purge

Aggressive delete of **Legacy-delete** only. Keep-v2 and Shared untouched. No new features.

### Delete procedure (per candidate)

1. Confirm bucket = Legacy-delete from Phase 0 inventory.
2. Repo-wide reference check (C#, prefabs, scenes, SOs, Resources, tests).
3. If any Keep-v2/Shared reference remains → reclassify; **do not delete**.
4. Delete code + `.meta` + orphaned assets together.
5. Fix compile breaks only by removing/updating callers that were themselves legacy.
6. Prefer small, reviewable delete commits over one mega-delete.

### In-scope purge categories

| Category | Examples (verify first) |
|----------|-------------------------|
| Legacy modes | Remaining FreeForAll / Ranked / classic paths and assets |
| Stand-and-cook stations | Types/prefabs only used by cook-in-place loop if fully replaced |
| Removed v1 mechanics | Combo, heat challenges, desperation aura, hand-offs, commit-window serving |
| Dead UI navigation | RouterService remnants; unused splash/loading UXML already partially deleted |
| Stale Firebase project code | Only if fully superseded and unreferenced (EOS is auth path); package removal is not required in this track |
| Orphan non-SoT docs | Prefer update over delete for architecture docs in the source-of-truth hierarchy |

### Out of scope for purge

- Anything Keep-v2 (including incomplete stubs)
- Shared networking / DI / state / UI shell
- “Ugly but live” match path code still on the current runtime path
- Third-party plugin trees under Samples/packages unless project code no longer references them **and** package removal is scheduled separately

### Safety rails

- No reintroduction of `FindObjectOfType` while deleting
- No drive-by refactors outside the delete blast radius
- If unsure → Unknown → keep
- After each delete batch: build affected projects when feasible

### Exit criteria

- [ ] No Legacy-delete items remain in inventory
- [ ] No dangling references to deleted types
- [ ] v2 WIP files still present and unchanged in intent
- [ ] Build does not fail due to missing legacy types

---

## 8. Phase 3 — DI Ownership Pass

Fix ownership and registration so the cleaned tree matches architecture rules. Still no new gameplay features.

### Target scope model

```
RootLifetimeScope
  app singletons: EventBus, Logging, Auth, UI, StateManager, Config/RC
  root network primitives: IPlayerNetworkManager, INetworkObjectPool, INetworkGameManager
  player / economy / character services

SessionLifetimeScope
  lobby, matchmaking, team, player manager, game starter, networking services container
  does NOT own root network primitives

MatchLifetimeScope
  score, orders, abilities, hazards, match context, bots, recipe catalog
```

### In-scope fixes

1. **Wrong-scope registrations** — move services to the correct LifetimeScope.
2. **Interface exposure** — cross-layer managers registered `As<IInterface>`.
3. **Resolution paths** — remove `SessionManager.SessionContainer` / `FindObjectOfType` / `NetworkManager.Singleton` **only where a correct injected path already exists** (no new architecture).
4. **Auto-registration consistency** — reflection-discovered states/screens remain Transient; no dual manual registration.
5. **Match runtime bridge** — scene objects via `MatchRuntimeSceneBinder` → `IMatchRuntimeRegistry` / `IMatchContext` only.
6. **Compile-only wiring for v2 types** — if a Keep-v2 type is unregistered and breaks compose-time resolve, register it correctly; **do not** implement missing gameplay behavior.

### Explicitly out of scope

- Full zero-singleton migration
- RouterService reintroduction
- Aspirational Root/Menu/Match rename from older GDD text
- Inventing a new `GameplayLifetimeScope` when code already uses `MatchLifetimeScope`

### Conductor reconciliation

Reconcile `system_registration_refactor` plan language to **current code names** (`MatchLifetimeScope`). Close or rewrite obsolete tasks that invent scopes the code does not use. Finish only registration work that matches Root/Session/Match.

### Exit criteria

- [ ] No parent-scope service depends on child-only types
- [ ] Root network primitives not owned by Session
- [ ] Match services not registered only at Root “for convenience” when they are match-scoped
- [ ] v2 types constructed via DI are registered in the correct scope
- [ ] Build green after registration moves

---

## 9. Phase 4 — Verification Gate

| Check | Action | Pass bar |
|-------|--------|----------|
| Build Core | `dotnet build RecipeRage.Core.csproj -nologo` | 0 errors |
| Build gameplay assemblies | build affected `.csproj` set | 0 errors |
| EditMode tests | `dotnet test RecipeRage.Tests.EditMode.csproj --no-build -nologo` | all pass |
| DI review | scan LifetimeScopes for Phase 3 violations | no known remaining violations |
| Doc spot-check | wiki status + roadmap phase text | matches Phase 1 exit criteria |

PlayMode / full Unity playthrough is **optional** and not a gate for this track.

---

## 10. Risks & Mitigations

| Risk | Mitigation |
|------|------------|
| Delete live match path by misclassifying Shared as Legacy | Reference gate + Unknown→keep default |
| Docs claim v2 complete when only scaffold exists | Status language: **in progress / scaffold**, not done |
| DI moves break runtime resolve | Small batches; existing interfaces only; build after each batch |
| Dirty WIP mixes cleanup with feature work | Separated commit prefixes (below) |
| Conductor plan names diverge from code | Reconcile plan to `MatchLifetimeScope` reality |

---

## 11. Commit Strategy

| Phase | Message pattern |
|-------|-----------------|
| 1 | `docs(...): align wiki/roadmap to Kitchen Brawler WIP + current architecture` |
| 2 | `chore(purge): remove legacy cooking/mode path X` (one or more commits) |
| 3 | `refactor(di): correct Root/Session/Match ownership for Y` |
| 4 | `test: update for removed legacy APIs` |

Include the session Co-authored-by trailer when committing unless the user opts out.

---

## 12. Deliverables

1. Updated wiki + architecture docs (truth pass)
2. Phase 0 inventory artifact
3. Legacy path removed; v2 WIP preserved
4. DI ownership corrected for known violations
5. Green build + EditMode tests
6. This design under `docs/superpowers/specs/`
7. Follow-on (out of scope): Kitchen Brawler vertical-slice implementation track

---

## 13. Follow-On Track (Not This Spec)

After cleanup verification:

- Plan a **Kitchen Brawler vertical slice** (recommended first: Rush Service 2v2) to playable
- Use `wiki/GameplayDesign.md` as product source of truth
- Do not reopen legacy cooking path

---

## 14. Approval

- Design sections §§1–6 reviewed conversationally and approved 2026-07-12
- Approach: Layered Freeze-and-Clean
- Boundary: cleanup/alignment only; no new gameplay features
- Purge posture: aggressive on legacy-only surface with reference gates
- WIP: included as starting state
