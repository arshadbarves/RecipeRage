# Architecture Cleanup Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restore a trustworthy baseline so agents and humans can trust wiki/roadmap, cannot accidentally extend the legacy cooking loop, and resolve services from the correct DI scope — without adding any new gameplay features.

**Architecture:** Layered Freeze-and-Clean on the current dirty working tree: inventory → docs truth → aggressive legacy-only purge with reference gates → DI ownership fixes → build + EditMode verification. Keep Kitchen Brawler v2 WIP untouched; purge legacy around it.

**Tech Stack:** Unity 6.0 (6000.3.0f1), VContainer, Netcode for GameObjects, EOS, UI Toolkit, NUnit EditMode tests, `dotnet build` / `dotnet test` for assembly gates.

## Global Constraints

- **No new gameplay features** — combat feel, mode balance, abilities, vertical-slice playability are out of scope.
- **No aspirational migrations** — no RouterService production nav, no zero-singleton utopia, no full Google/Facebook/Apple EOS Connect.
- **No PlayMode gate** — EditMode + compile only.
- **WIP policy** — current dirty tree is the starting state; keep v2 WIP; purge legacy around it.
- **Classification safety** — Unknown defaults to keep; no delete without repo-wide refs; assets follow code.
- **Scope names (code truth)** — use `RootLifetimeScope`, `MenuLifetimeScope` (session/menu child), `MatchLifetimeScope`. Do **not** invent `GameplayLifetimeScope` or resurrect `GameLifetimeScope` / `SessionLifetimeScope` file names unless code is renamed (it is not in this track).
- **Commit prefixes** — `docs(...)`, `chore(purge):`, `refactor(di):`, `test:`; include Co-authored-by trailer:
  `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>`
- **Source-of-truth hierarchy** — current code → `wiki/` → `Documentation/Architecture/PROJECT_MEMORY.md` → GDD → audits.
- **Drift protocol** — intentional wiki corrections use option B and append `wiki/log.md` per `wiki/DRIFT-PROTOCOL.md`.

### Spec reference

`docs/superpowers/specs/2026-07-12-architecture-cleanup-design.md` (commit `c0f1c120`)

### Keep-v2 anchors (never delete; do not finish features)

| Type / asset | Path |
|--------------|------|
| `PlayerCombatController` | `Assets/_KitchenClash/Infrastructure/Network/PlayerCombatController.cs` |
| `LootPickup` | `Assets/_KitchenClash/Infrastructure/Network/LootPickup.cs` |
| `AutonomousCookingStation` | `Assets/_KitchenClash/Infrastructure/Network/Stations/AutonomousCookingStation.cs` |
| `MatchWinConditionCoordinator` | `Assets/_KitchenClash/Infrastructure/Network/MatchWinConditionCoordinator.cs` |
| `ModeWinConditions` / `IModeWinCondition` | `Assets/_KitchenClash/Application/Services/ModeWinConditions.cs` |
| `ArchetypeAbilities` | `Assets/_KitchenClash/Infrastructure/Gameplay/Abilities/ArchetypeAbilities.cs` |
| Mode assets | `Assets/Resources/ScriptableObjects/GameModes/{RushService,HellsKitchen,LastPlateStanding}.asset` |
| Autonomous station prefabs | `Assets/_KitchenClash/Prefabs/Stations/AutonomousCookingStation_T{1,2,3}.prefab` |

### Live DI scopes (code)

| Scope | File | Role |
|-------|------|------|
| Root | `Assets/_KitchenClash/Composition/RootLifetimeScope.cs` | App singletons, UI, state machine, root network primitives |
| Menu (session child) | `Assets/_KitchenClash/Composition/MenuLifetimeScope.cs` | Lobby/matchmaking/team/player, economy, game starter, networking container |
| Match | `Assets/_KitchenClash/Composition/MatchLifetimeScope.cs` | Score/order/ability/hazard, match context, bots, recipe catalog |

### File map for this track

| Artifact | Responsibility |
|----------|----------------|
| `docs/superpowers/plans/2026-07-12-architecture-cleanup-inventory.md` | Phase 0 classification table (Keep-v2 / Shared / Legacy-delete / Unknown) |
| `wiki/*`, `Documentation/Architecture/*`, root analysis docs | Phase 1 truth alignment |
| Legacy code/assets listed in inventory | Phase 2 deletes only |
| `RootLifetimeScope.cs`, `MenuLifetimeScope.cs`, `MatchLifetimeScope.cs`, callers of `SessionContainer` / scene finds | Phase 3 ownership |
| `RecipeRage.*.csproj`, EditMode tests | Phase 4 gate |

---

## Phase 0 — Inventory & Classification

### Task 1: Create inventory artifact and classify the tree

**Files:**
- Create: `docs/superpowers/plans/2026-07-12-architecture-cleanup-inventory.md`
- Read (do not modify yet): Keep-v2 anchors above; `Assets/_KitchenClash/Infrastructure/Network/Stations/*`; `Assets/Prefabs/Gameplay/Stations/*`; `Assets/Scripts/Editor/GameModeGenerator.cs`; `Assets/Scenes/Game.unity`; composition scopes

**Interfaces:**
- Consumes: design buckets from spec §5
- Produces: inventory markdown with every candidate row: `path | bucket | one-line rationale | refs-checked (yes/no)`

- [ ] **Step 1: Create the inventory file skeleton**

Write this exact structure:

```markdown
# Architecture Cleanup Inventory — 2026-07-12

> Phase 2 may delete **only** rows with bucket `Legacy-delete` and `refs-checked: yes` with zero Keep-v2/Shared refs.

## Legend

| Bucket | Action |
|--------|--------|
| Keep-v2 | Keep; do not finish features |
| Shared | Keep; may fix ownership in Phase 3 |
| Legacy-delete | Delete in Phase 2 after reference gate |
| Unknown | Default keep until proven legacy-only |

## Keep-v2

| Path | Rationale | refs-checked |
|------|-----------|--------------|
| | | |

## Shared

| Path | Rationale | refs-checked |
|------|-----------|--------------|
| | | |

## Legacy-delete

| Path | Rationale | refs-checked |
|------|-----------|--------------|
| | | |

## Unknown

| Path | Rationale | refs-checked |
|------|-----------|--------------|
| | | |

## Dual-path notes

- `Game.unity` still references legacy station instance names (`CookingPot`, `CuttingStation`, `AssemblyStation`). Treat live match-path stations as Shared/Unknown until a v2 scene fully replaces them.
- `MenuLifetimeScope` is the session/menu child scope in code (docs historically said `SessionLifetimeScope`).
```

- [ ] **Step 2: Seed Keep-v2 rows**

Add every Keep-v2 anchor from the table above with rationale `Kitchen Brawler v2 scaffold — do not delete` and `refs-checked: n/a`.

- [ ] **Step 3: Classify stations**

Run:

```bash
rg -n "CookingStation|ProcessingStation|CuttingStation|ServingStation|StationBase|AutonomousCookingStation|CounterStation|SinkStation|PlateDispenser|IngredientCrate|DeliveryZone|TrashBin" \
  Assets/_KitchenClash Assets/Scripts Assets/Scenes Assets/Prefabs Assets/Resources \
  -g '!Library/**' | head -200
```

Classification rules for this step:
- `AutonomousCookingStation*` → Keep-v2
- Types still referenced from `PlayerController`, bots, `Game.unity`, or match path → Shared or Unknown (default keep)
- Types with **zero** production refs outside their own file/tests and fully superseded by autonomous stations → Legacy-delete candidate only after Step 5 reference gate

At design time, expect most of `Assets/_KitchenClash/Infrastructure/Network/Stations/{CookingStation,ProcessingStation,CuttingStation,ServingStation,StationBase,...}.cs` and `Assets/Prefabs/Gameplay/Stations/*` to land in **Shared** or **Unknown** because `Game.unity` still names CookingPot / CuttingStation / AssemblyStation instances. Do **not** pre-mark them Legacy-delete without a zero-ref proof.

- [ ] **Step 4: Classify modes, editor tools, and v1 mechanic remnants**

Run:

```bash
rg -n "FreeForAll|RankedMode|TeamBattle|classicMode|ComboChain|DesperationAura|HeatChallenge|HandOff|CommitWindow|RouterService|IRouterService" \
  Assets Documentation wiki -g '!Library/**' | head -200
```

Expected starting classification (verify, do not assume delete):
- `Assets/Scripts/Editor/GameModeGenerator.cs` FreeForAll/TeamBattle helpers → Unknown or Legacy-delete only if nothing loads those modes
- Mode assets under `Assets/Resources/ScriptableObjects/GameModes/` for Triplet A → Keep-v2
- RouterService: code search at design time found **docs only** → doc fix in Phase 1, not a code purge unless a dead C# type appears

- [ ] **Step 5: Reference-gate every Legacy-delete candidate**

For each candidate path `CANDIDATE` (basename without extension):

```bash
# Code
rg -n "CANDIDATE" Assets Documentation wiki -g '*.cs' -g '!Library/**'
# Assets / scenes / SOs
rg -n "CANDIDATE" Assets -g '*.{prefab,unity,asset,uxml,uss,asmdef}' -g '!Library/**'
```

If any Keep-v2 or Shared production reference remains → reclassify to Shared/Unknown. Only when both searches show no live consumers (or only self + doomed tests) set `refs-checked: yes` and keep bucket Legacy-delete.

- [ ] **Step 6: Commit inventory**

```bash
git add docs/superpowers/plans/2026-07-12-architecture-cleanup-inventory.md
git commit -m "$(cat <<'EOF'
docs(plan): add architecture cleanup Phase 0 inventory

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

## Phase 1 — Docs Truth Pass

Docs only. Do not delete production code.

### Task 2: Fix wiki GameplayDesign status + log entry

**Files:**
- Modify: `wiki/GameplayDesign.md` (status banner lines 3–6)
- Modify: `wiki/log.md` (append entry)
- Read: `wiki/DRIFT-PROTOCOL.md`

**Interfaces:**
- Consumes: inventory Keep-v2 list (scaffold present)
- Produces: status language agents must trust

- [ ] **Step 1: Replace the false “not started” banner**

In `wiki/GameplayDesign.md`, change the status block from:

```markdown
> **Status: REDESIGN LOCKED (2026-06-02, supersedes v1)**
> This page replaces the v1 "shared-arena cooking" design rejected as "too boring."
> Design intent here is the source of truth for all gameplay implementation.
> Implementation has not started. All values below are design targets pending playtesting.
```

to:

```markdown
> **Status: REDESIGN LOCKED (2026-06-02, supersedes v1) — IMPLEMENTATION IN PROGRESS**
> This page replaces the v1 "shared-arena cooking" design rejected as "too boring."
> Design intent here is the source of truth for all gameplay implementation.
> Scaffold is present in tree (`PlayerCombatController`, `LootPickup`, `AutonomousCookingStation`,
> `MatchWinConditionCoordinator`, mode assets/maps, archetype abilities). **Not playable/complete.**
> Values below remain design targets pending playtesting. Do not claim v2 is done.
```

- [ ] **Step 2: Append drift-resolved log entry**

Append to `wiki/log.md`:

```markdown
## [2026-07-12] drift-resolved | Architecture cleanup truth pass (GameplayDesign)

- Option B: wiki updated to match code reality
- Was: "Implementation has not started"
- Now: scaffold present / implementation in progress / not playable-complete
- Track: docs/superpowers/specs/2026-07-12-architecture-cleanup-design.md
- Keep-v2 anchors listed in docs/superpowers/plans/2026-07-12-architecture-cleanup-inventory.md
```

- [ ] **Step 3: Grep for remaining “has not started” claims**

```bash
rg -n "has not started|Implementation has not started" wiki/
```

Expected: no matches (or only historical log quotes). Fix any remaining wiki page claims.

- [ ] **Step 4: Commit**

```bash
git add wiki/GameplayDesign.md wiki/log.md
git commit -m "$(cat <<'EOF'
docs(wiki): mark Kitchen Brawler scaffold as in progress

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

### Task 3: Align wiki Technical / LLM-Rules / index with code DI + UI + auth

**Files:**
- Modify: `wiki/Technical.md`
- Modify: `wiki/LLM-Rules.md`
- Modify: `wiki/index.md` and/or `wiki/README.md` if they claim v2 not started or wrong scopes
- Modify: `wiki/Gameplay.md`, `wiki/Characters.md` only where they present legacy cooking as the sole live path without marking it legacy
- Modify: `wiki/log.md`

**Interfaces:**
- Consumes: actual scopes in `Assets/_KitchenClash/Composition/*LifetimeScope.cs`
- Produces: wiki that says Unity 6, Root/Menu/Match, EOS auth, UIService navigation

- [ ] **Step 1: Inventory false claims**

```bash
rg -n "RouterService|IRouterService|GameLifetimeScope|SessionLifetimeScope|Unity 2022|Firebase Auth|implementation has not started" \
  wiki/ -n
```

- [ ] **Step 2: Update Technical.md DI / presentation rows**

Replace any production claim that RouterService is current navigation with:

```markdown
| Presentation | UI Toolkit screens (UXML+USS), ViewModels, `UIService` + `UIScreenStackManager` | Application only |
```

Document scopes as:

```markdown
RootLifetimeScope (app)
  → MenuLifetimeScope (session/menu child: lobby, matchmaking, team, economy, networking container)
    → MatchLifetimeScope (match: score, orders, abilities, hazards, bots, match context)
```

Note explicitly:

```markdown
Historical docs may say `GameLifetimeScope` / `SessionLifetimeScope`. **Code names are**
`RootLifetimeScope` / `MenuLifetimeScope` / `MatchLifetimeScope` under
`Assets/_KitchenClash/Composition/`.
```

Auth row must say EOS / project auth path is current; Firebase Auth is not the production auth story. Firebase may still appear as analytics/config provider code — do not claim it is removed unless Phase 2 deletes it.

- [ ] **Step 3: Update LLM-Rules.md UI row**

Change UI rule from RouterService-as-current to:

```markdown
| UI | UI Toolkit + MVVM + UIService / UIScreenStackManager (RouterService is aspirational, not production) |
```

- [ ] **Step 4: Mark legacy vs v2 in Gameplay.md / Characters.md**

Where those pages describe stand-and-cook as the only loop, add a short callout at the top of the conflicting section:

```markdown
> **Legacy vs v2:** Kitchen Brawler v2 (`wiki/GameplayDesign.md`) is the product direction.
> Sections below that describe cook-in-place / hand-off / combo-only loops are **legacy reference**
> unless explicitly marked as still live on the current match path.
```

Do not rewrite entire pages in this task — truth markers + obvious contradictions only.

- [ ] **Step 5: Log + commit**

Append to `wiki/log.md`:

```markdown
## [2026-07-12] drift-resolved | Technical + LLM-Rules scope/nav truth

- Option B: wiki matches Root/Menu/Match + UIService production nav
- RouterService demoted to aspirational
- Unity/engine and auth wording aligned to current code
```

```bash
git add wiki/Technical.md wiki/LLM-Rules.md wiki/index.md wiki/README.md wiki/Gameplay.md wiki/Characters.md wiki/log.md
git commit -m "$(cat <<'EOF'
docs(wiki): align Technical/LLM-Rules to Root/Menu/Match and UIService

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

### Task 4: Reframe architecture roadmap + PROJECT_MEMORY + GDD matrix

**Files:**
- Modify: `Documentation/Architecture/PHASE_ROADMAP.md`
- Modify: `Documentation/Architecture/PROJECT_MEMORY.md`
- Modify: `Documentation/Architecture/GDD_ALIGNMENT_MATRIX.md`

**Interfaces:**
- Consumes: cleanup track phases; Keep-v2 scaffold status
- Produces: roadmap that prioritizes cleanup → v2 slices, not legacy cooking verification as product path

- [ ] **Step 1: Rewrite PHASE_ROADMAP current phase header**

Replace the “Phase 2: Runtime Verification and Stabilization” product-path framing (legacy cooking verification as primary) with:

```markdown
## Current Phase

### Phase: Architecture Cleanup → Kitchen Brawler Vertical Slices

Status: `Current` (cleanup track active)

Primary targets (in order):

1. **Architecture cleanup track** (this work)
   - Docs truth (wiki + PROJECT_MEMORY + roadmap)
   - Dead-code purge of legacy-only cooking/mode surface
   - DI ownership: Root / Menu / Match
   - Green `dotnet build` + EditMode tests
2. **Follow-on (separate track):** Kitchen Brawler vertical slice — recommended first: Rush Service 2v2 to playable

Exit criteria for cleanup:

- Wiki/roadmap trustable; no “v2 not started” falsehoods
- Legacy-delete inventory empty
- DI ownership matches composition scopes
- Build + EditMode green

Legacy cooking-loop verification is **not** the product path. Dual-path stations may remain Shared until a v2 scene replaces them; do not prioritize extending stand-and-cook.
```

Keep historical “Completed Foundation” sections; do not delete history — reframe **current** work only.

- [ ] **Step 2: Align PROJECT_MEMORY paths and scope names**

Search/replace documentation-only names:

| Stale | Correct |
|-------|---------|
| `Assets/Scripts/Gameplay/Bootstrap/GameLifetimeScope.cs` | `Assets/_KitchenClash/Composition/RootLifetimeScope.cs` |
| `Assets/Scripts/Gameplay/Bootstrap/SessionLifetimeScope.cs` | `Assets/_KitchenClash/Composition/MenuLifetimeScope.cs` |
| `GameLifetimeScope` (as current code name) | `RootLifetimeScope` |
| `SessionLifetimeScope` (as current code name) | `MenuLifetimeScope` (session/menu child) |
| `GameplayLifetimeScope` (if present as planned current) | `MatchLifetimeScope` |

Preserve rules that remain true:
- Menu/session must **not** own `INetworkObjectPool` / `INetworkGameManager`
- Root owns those network primitives
- Match owns score/order/ability/hazard/bots/match context

Add a short Kitchen Brawler note:

```markdown
## Kitchen Brawler v2 (scaffold)

Scaffold present; not complete. See `wiki/GameplayDesign.md` and
`docs/superpowers/plans/2026-07-12-architecture-cleanup-inventory.md` Keep-v2 list.
Do not extend legacy stand-and-cook when v2 owns the responsibility.
```

- [ ] **Step 3: Update GDD_ALIGNMENT_MATRIX**

1. Fix rows that claim Root/Menu/Match is only “Planned” while code already has the three scopes — mark DI scope split as **Partial/Implemented** with note: names are Root/Menu/Match (not aspirational rename).
2. Keep RouterService as **Planned** (aspirational).
3. Add Kitchen Brawler rows:

| Feature | Status | Evidence | Notes |
|---------|--------|----------|-------|
| Player combat controller | Partial | `PlayerCombatController.cs` | Scaffold; not vertical-slice complete |
| Autonomous cooking stations | Partial | `AutonomousCookingStation.cs` + T1–T3 prefabs | Dual-path with legacy stations in `Game.unity` |
| KO loot pickups | Partial | `LootPickup.cs` | Scaffold |
| Mode win conditions (Triplet A) | Partial | `ModeWinConditions.cs`, mode assets | Rush / Hell’s Kitchen / Last Plate |
| Match win coordinator | Partial | `MatchWinConditionCoordinator.cs` | Scaffold |
| Archetype abilities | Partial | `ArchetypeAbilities.cs` | Scaffold |

- [ ] **Step 4: Commit**

```bash
git add Documentation/Architecture/PHASE_ROADMAP.md \
  Documentation/Architecture/PROJECT_MEMORY.md \
  Documentation/Architecture/GDD_ALIGNMENT_MATRIX.md
git commit -m "$(cat <<'EOF'
docs(arch): reframe roadmap and memory for cleanup → Kitchen Brawler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

### Task 5: Demote or fix root analysis docs (README / QUICK_REFERENCE / CODEBASE_ANALYSIS)

**Files:**
- Modify: `README.md`
- Modify: `QUICK_REFERENCE.md`
- Modify: `CODEBASE_ANALYSIS.md`

**Interfaces:**
- Consumes: Phase 1 wiki + PROJECT_MEMORY as higher SoT
- Produces: no root doc claiming Unity 2022 + GameLifetimeScope as current truth without demotion banner

- [ ] **Step 1: Prefer demotion banner + critical path fixes**

At the top of `CODEBASE_ANALYSIS.md` and `QUICK_REFERENCE.md` (and README architecture bullets if wrong), ensure readers see:

```markdown
> **Secondary doc:** For architecture truth prefer `wiki/` and
> `Documentation/Architecture/PROJECT_MEMORY.md`. This file may lag.
> Current DI scopes: `RootLifetimeScope` → `MenuLifetimeScope` → `MatchLifetimeScope`
> under `Assets/_KitchenClash/Composition/`. Engine: Unity 6.0. Navigation: `UIService`.
```

- [ ] **Step 2: Fix the worst false paths in README**

Replace:

```markdown
RecipeRage is a Unity 2022.3 multiplayer cooking game...
- Root DI scope: `Assets/Scripts/Gameplay/Bootstrap/GameLifetimeScope.cs`
- Session DI scope: `Assets/Scripts/Gameplay/Bootstrap/SessionLifetimeScope.cs`
```

with:

```markdown
RecipeRage is a Unity 6.0 multiplayer cooking-competition game (Kitchen Brawler v2 direction)...
- Root DI scope: `Assets/_KitchenClash/Composition/RootLifetimeScope.cs`
- Menu/session DI scope: `Assets/_KitchenClash/Composition/MenuLifetimeScope.cs`
- Match DI scope: `Assets/_KitchenClash/Composition/MatchLifetimeScope.cs`
```

- [ ] **Step 3: Commit**

```bash
git add README.md QUICK_REFERENCE.md CODEBASE_ANALYSIS.md
git commit -m "$(cat <<'EOF'
docs: demote stale analysis docs and fix Unity 6 / DI paths

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

### Phase 1 exit checklist

- [ ] **Step 4: Verify Phase 1 exit criteria**

```bash
rg -n "has not started" wiki/ || true
rg -n "GameLifetimeScope|SessionLifetimeScope" wiki/Documentation/Architecture/PROJECT_MEMORY.md README.md || true
rg -n "RouterService" wiki/Technical.md wiki/LLM-Rules.md
```

Pass bar:
- No wiki claim that v2 “has not started”
- Roadmap prioritizes cleanup → v2 slices
- DI/auth/UI descriptions match code (RouterService aspirational only)
- `wiki/log.md` has truth-pass entries

---

## Phase 2 — Dead-Code Purge

Delete **only** inventory rows with `Legacy-delete` + `refs-checked: yes`. Prefer small commits.

### Task 6: Purge batch — confirmed legacy modes / editor dead paths

**Files:**
- Modify or delete per inventory (likely): `Assets/Scripts/Editor/GameModeGenerator.cs`
- Delete only if inventory says so: any remaining FreeForAll/Ranked/TeamBattle/classicMode assets under `Assets/Resources` or elsewhere
- Modify: inventory file (strike deleted rows)

**Interfaces:**
- Consumes: inventory Legacy-delete mode rows
- Produces: no dead mode generators/assets that reintroduce classic modes

- [ ] **Step 1: Re-run reference gate for each mode candidate**

```bash
rg -n "FreeForAll|RankedMode|TeamBattle|classicMode" Assets Documentation -g '!Library/**'
```

- [ ] **Step 2: If GameModeGenerator only supports dead modes, strip dead branches**

If Triplet A modes must remain generatable, **edit** rather than delete the file: remove `SetFreeForAllDefaults` / `SetTeamBattleDefaults` and any enum/menu entries that create classic modes; keep Rush Service / Hell’s Kitchen / Last Plate Standing paths.

Example shape (adapt to actual file content after reading):

```csharp
// Remove classic mode cases from the generator switch.
// Keep only:
// - RushService
// - HellsKitchen
// - LastPlateStanding
```

If the entire generator only exists for classic modes and nothing invokes it for v2 → delete `GameModeGenerator.cs` + `.meta` after confirming zero refs:

```bash
rg -n "GameModeGenerator" Assets -g '!Library/**'
```

- [ ] **Step 3: Delete orphan classic mode assets only with zero refs**

```bash
# Example — only if files exist and refs are zero
git rm -f path/to/FreeForAll.asset path/to/FreeForAll.asset.meta
```

- [ ] **Step 4: Update inventory (mark rows deleted)**

- [ ] **Step 5: Commit**

```bash
git add -A Assets/Scripts/Editor Assets/Resources docs/superpowers/plans/2026-07-12-architecture-cleanup-inventory.md
git commit -m "$(cat <<'EOF'
chore(purge): remove legacy game mode paths from editor/assets

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

### Task 7: Purge batch — v1 mechanic remnants (combo / heat / desperation / hand-off / Router)

**Files:**
- Delete only paths listed Legacy-delete in inventory after gate
- At design time, RouterService had **no C# types** — skip code delete if still true

**Interfaces:**
- Consumes: inventory mechanic rows
- Produces: no orphan v1 mechanic types

- [ ] **Step 1: Search for remnant types**

```bash
rg -n "class .*Combo|class .*Desperation|class .*HeatChallenge|class .*HandOff|class RouterService|interface IRouter" \
  Assets -g '*.cs' -g '!Library/**'
```

- [ ] **Step 2: For each type found, classify and gate**

If Keep-v2/Shared references exist → reclassify, do not delete.  
If zero live refs → `git rm` code + `.meta` + orphaned prefabs/SOs.

- [ ] **Step 3: Commit only if something was deleted**

```bash
git commit -m "$(cat <<'EOF'
chore(purge): remove unused v1 mechanic remnants

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

If search finds nothing to delete, record in inventory “no v1 mechanic code found” and skip commit.

### Task 8: Purge batch — stand-and-cook surface (aggressive but gated)

**Files (candidates only — delete solely if inventory Legacy-delete + zero Keep-v2/Shared refs):**
- Station scripts under `Assets/_KitchenClash/Infrastructure/Network/Stations/`
- Prefabs under `Assets/Prefabs/Gameplay/Stations/`
- Scene references only if a dedicated legacy-only scene exists (do **not** gut `Game.unity` dual-path without replacement)

**Interfaces:**
- Consumes: inventory station rows
- Produces: single gameplay path **where v2 fully owns the responsibility**; dual-path remains if `Game.unity` still live

- [ ] **Step 1: Prove dual-path status**

```bash
rg -n "CookingPot|CuttingStation|AssemblyStation|CookingStation|AutonomousCookingStation" \
  Assets/Scenes Assets/Prefabs Assets/_KitchenClash -g '!Library/**' | head -100
```

- [ ] **Step 2: Apply the dual-path rule**

If `Game.unity` or live match code still depends on legacy stations:
- Leave those types **Shared/Unknown**
- Do **not** delete `CookingStation.cs` / `ProcessingStation.cs` / related prefabs in this batch
- Document in inventory: “Blocked on v2 scene replacement — follow-on track”

If a specific type is truly unreferenced (example: a helper only used by deleted classic mode):
- Delete code + `.meta` + orphan prefab together

- [ ] **Step 3: Never delete Keep-v2 autonomous station files**

Refuse any delete touching:
- `AutonomousCookingStation.cs`
- `AutonomousCookingStation_T*.prefab`

- [ ] **Step 4: Commit only real deletes**

```bash
git commit -m "$(cat <<'EOF'
chore(purge): remove unreferenced legacy station surface

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

### Task 9: Purge batch — stale Firebase project code (only if fully superseded)

**Files (candidates):**
- `Assets/_KitchenClash/Infrastructure/Analytics/FirebaseAnalyticsService.cs`
- `Assets/_KitchenClash/Infrastructure/Firebase/*`
- Registration in `RootLifetimeScope.cs`

**Interfaces:**
- Consumes: whether EOS/other implementations fully replace these registrations
- Produces: either kept Shared, or deleted with registration removed

- [ ] **Step 1: Trace registrations and interfaces**

```bash
rg -n "FirebaseAnalyticsService|FirebaseConfigProvider|IAnalyticsService|IConfigProvider" \
  Assets/_KitchenClash -g '*.cs'
```

- [ ] **Step 2: Decision rule**

- If `RootLifetimeScope` still registers Firebase as the live `IAnalyticsService` / config provider → **Shared** (do not delete; package removal out of scope)
- If a non-Firebase implementation is already registered and Firebase types are unreferenced → Legacy-delete

- [ ] **Step 3: If deleting, remove registration first, then types**

In `RootLifetimeScope.cs`, swap only when a replacement already exists (do not invent a new analytics stack):

```csharp
// Only if a non-Firebase IAnalyticsService implementation already exists and is intended:
// builder.Register<ExistingAnalyticsService>(Lifetime.Singleton).As<IAnalyticsService>();
// Remove: builder.Register<FirebaseAnalyticsService>(...)
```

Then `git rm` unreferenced Firebase files + `.meta`.

- [ ] **Step 4: Commit or skip**

```bash
git commit -m "$(cat <<'EOF'
chore(purge): remove unreferenced Firebase project wrappers

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

### Task 10: Fix compile breaks from purge (callers only)

**Files:**
- Any remaining references to deleted types (tests, usings, prefab scripts will show at build)

**Interfaces:**
- Consumes: deleted type names
- Produces: compiling tree without reintroducing legacy APIs

- [ ] **Step 1: Build Core**

```bash
dotnet build RecipeRage.Core.csproj -nologo
```

Expected: 0 errors. If errors name deleted types, fix by removing callers (not restoring legacy).

- [ ] **Step 2: Build gameplay / kitchen clash projects that exist**

```bash
ls *.csproj
dotnet build RecipeRage.Gameplay.csproj -nologo 2>/dev/null || true
# Build any KitchenClash.* or related csproj that failed discovery:
dotnet build <failed-project>.csproj -nologo
```

- [ ] **Step 3: Update or delete tests that targeted removed APIs**

```bash
rg -n "DeletedTypeName" Assets/Scripts/Tests -g '*.cs'
```

Delete obsolete tests or rewrite only enough to compile — no new coverage goals.

- [ ] **Step 4: Commit**

```bash
git commit -m "$(cat <<'EOF'
test: drop callers of purged legacy APIs

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

### Phase 2 exit checklist

- [ ] Inventory has no remaining undeleted Legacy-delete rows
- [ ] Keep-v2 files still present
- [ ] Build does not fail due to missing legacy types

---

## Phase 3 — DI Ownership Pass

Still no new gameplay features. Use **code** scope names.

### Task 11: Document and audit current registrations

**Files:**
- Read: `Assets/_KitchenClash/Composition/RootLifetimeScope.cs`
- Read: `Assets/_KitchenClash/Composition/MenuLifetimeScope.cs`
- Read: `Assets/_KitchenClash/Composition/MatchLifetimeScope.cs`
- Create or update section in inventory: `## DI audit`

**Interfaces:**
- Produces: checklist of violations to fix in Tasks 12–14

- [ ] **Step 1: Capture registration lists**

```bash
rg -n "builder\.Register" Assets/_KitchenClash/Composition/RootLifetimeScope.cs
rg -n "builder\.Register" Assets/_KitchenClash/Composition/MenuLifetimeScope.cs
rg -n "builder\.Register" Assets/_KitchenClash/Composition/MatchLifetimeScope.cs
```

- [ ] **Step 2: Write DI audit table into inventory**

```markdown
## DI audit

| Service | Registered in | Should be | Violation? |
|---------|---------------|-----------|------------|
| INetworkObjectPool | Root | Root | no |
| INetworkGameManager | Root | Root | no |
| IPlayerNetworkManager | Root | Root | no |
| ILobbyManager / matchmaking / team / player | Menu | Menu | no |
| IScoreService / IOrderService / IAbilityService / IHazardService | Match | Match | no |
| IMatchContext / bots / RecipeCatalog | Match | Match | no |
```

Flag any match-only service registered only at Root “for convenience,” or root network primitives registered in Menu.

- [ ] **Step 3: Scan anti-patterns**

```bash
rg -n "SessionContainer|FindObjectOfType|FindObjectsByType|NetworkManager\.Singleton" \
  Assets/_KitchenClash Assets/Scripts -g '*.cs' -g '!**/Samples/**'
```

Known design-time hits to classify:

| Location | Likely action |
|----------|----------------|
| `MatchRuntimeSceneBinder` `FindObjectsByType` for scene discovery | Shared bootstrap — only replace if binder already has a non-find path; do not invent architecture |
| `SpawnManager` `FindObjectsByType<SpawnPoint>` | Unknown/Shared — fix only if `IMatchRuntimeRegistry` already exposes spawn points |
| `ServingStation` `FindObjectsByType<SinkStation>` | Prefer registry/context if available; else leave Unknown |
| `HomeScreen` / `SkinsTabComponent` `SessionContainer` | Prefer injected services already on Menu scope |
| `PlayerController` `SessionContainer` | Prefer constructor/field injection of needed interfaces |
| Comments forbidding Singleton | Keep comments; no code change |

- [ ] **Step 4: Commit audit notes**

```bash
git add docs/superpowers/plans/2026-07-12-architecture-cleanup-inventory.md
git commit -m "$(cat <<'EOF'
docs(plan): record DI ownership audit for cleanup track

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

### Task 12: Fix wrong-scope registrations (Root / Menu / Match)

**Files:**
- Modify: `Assets/_KitchenClash/Composition/RootLifetimeScope.cs`
- Modify: `Assets/_KitchenClash/Composition/MenuLifetimeScope.cs`
- Modify: `Assets/_KitchenClash/Composition/MatchLifetimeScope.cs`
- Modify: consumers only if constructor injection sites must change

**Interfaces:**
- Target model:

```
RootLifetimeScope
  EventBus, Logging, Auth, UI, StateManager, Config/RC
  IPlayerNetworkManager, INetworkObjectPool, INetworkGameManager
  player data / save / root-safe services

MenuLifetimeScope
  lobby, matchmaking, team, player manager, game starter, NetworkingServiceContainer
  economy/character/skins/game mode as currently session-bound
  does NOT own INetworkObjectPool / INetworkGameManager

MatchLifetimeScope
  Score, Order, Ability, Hazard, MatchContext, bots, RecipeCatalog, bridges
```

- [ ] **Step 1: Move any mis-registered service**

For each audit violation, cut registration from wrong scope and paste into correct scope with the same lifetime and `As<TInterface>()` surface. Example pattern:

```csharp
// MatchLifetimeScope — match-only services stay here
builder.Register<ScoreService>(Lifetime.Scoped).As<IScoreService>();
builder.Register<OrderService>(Lifetime.Scoped).As<IOrderService>();
builder.Register<AbilityService>(Lifetime.Scoped).As<IAbilityService>();
builder.Register<HazardService>(Lifetime.Scoped).As<IHazardService>();
```

```csharp
// MenuLifetimeScope — must NOT re-register root network primitives
// FORBIDDEN here:
// builder.Register<NetworkObjectPool>(...).As<INetworkObjectPool>();
// builder.Register<NetworkGameManager>(...).As<INetworkGameManager>();
```

- [ ] **Step 2: Register Keep-v2 types only if compose-time resolve requires it**

If a Keep-v2 service is constructed via DI and missing registration, add **compile-only** registration in the correct scope. Do **not** implement missing gameplay.

Example (only if such a service type + interface already exist):

```csharp
// MatchLifetimeScope — example shape; use real types from code
// builder.Register<ExistingModeWinConditionService>(Lifetime.Scoped).As<IModeWinCondition>();
```

If types are scene `NetworkBehaviour`s without interfaces, do not force DI registration.

- [ ] **Step 3: Build after moves**

```bash
dotnet build RecipeRage.Core.csproj -nologo
dotnet build RecipeRage.Gameplay.csproj -nologo 2>/dev/null || true
```

Expected: 0 errors.

- [ ] **Step 4: Commit**

```bash
git add Assets/_KitchenClash/Composition/
git commit -m "$(cat <<'EOF'
refactor(di): correct Root/Menu/Match service ownership

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

### Task 13: Replace avoidable SessionContainer / scene-find resolution

**Files (only where a correct injected path already exists):**
- Modify: `Assets/_KitchenClash/Presentation/Screens/HomeScreen.cs`
- Modify: `Assets/_KitchenClash/Presentation/Components/SkinsTabComponent.cs`
- Modify: `Assets/_KitchenClash/Infrastructure/Network/PlayerController.cs`
- Modify: `Assets/_KitchenClash/Infrastructure/Network/Stations/ServingStation.cs` only if registry path exists
- Do **not** rewrite `MatchRuntimeSceneBinder` discovery unless replacing with an already-built API

**Interfaces:**
- Consumes: services already registered on Menu/Match/Root
- Produces: fewer `SessionContainer.Resolve` calls without new architecture

- [ ] **Step 1: HomeScreen — inject instead of SessionContainer when possible**

Read the current resolve block (~line 185). If it only resolves types already constructable via `[Inject]` / constructor on a ViewModel, move resolution to the ViewModel or inject the interface on the screen.

Pattern:

```csharp
// Before (avoid when interface is already registered on Menu scope):
// var sessionContainer = _sessionManager.SessionContainer;
// var svc = sessionContainer.Resolve<ISomeService>();

// After:
[Inject] private ISomeService _someService;
// use _someService directly
```

If the screen cannot take injection for a valid VContainer reason, leave a short comment and inventory note — do not invent a service locator.

- [ ] **Step 2: SkinsTabComponent — same rule**

- [ ] **Step 3: PlayerController — prefer injected match/session services**

Where `SessionContainer` is used to grab a service that is already an interface on Menu/Match, inject that interface. Keep NetworkBehaviour constraints in mind (`[Inject]` methods already used in this codebase).

- [ ] **Step 4: ServingStation sinks**

If `IMatchRuntimeRegistry` / `IMatchContext` already exposes sinks or interactables, use it. If not, **leave** `FindObjectsByType` and mark Unknown — do not build a new registry API in this track.

- [ ] **Step 5: Build + commit**

```bash
dotnet build RecipeRage.Core.csproj -nologo
git add Assets/_KitchenClash/Presentation Assets/_KitchenClash/Infrastructure
git commit -m "$(cat <<'EOF'
refactor(di): prefer injection over SessionContainer where path exists

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

### Task 14: Reconcile conductor system_registration_refactor plan names

**Files:**
- Modify: `conductor/tracks/system_registration_refactor_20260109/plan.md`
- Modify: `conductor/tracks/system_registration_refactor_20260109/spec.md` only if it invents wrong scope names
- Optionally: `metadata.json` status if the track is completed/superseded by this cleanup

**Interfaces:**
- Produces: conductor text that says `MatchLifetimeScope` / `MenuLifetimeScope` / `RootLifetimeScope`

- [ ] **Step 1: Search conductor for wrong names**

```bash
rg -n "GameplayLifetimeScope|GameLifetimeScope|SessionLifetimeScope" \
  conductor/tracks/system_registration_refactor_20260109/
```

- [ ] **Step 2: Replace plan language with code names**

| Plan text | Code text |
|-----------|-----------|
| `GameplayLifetimeScope` | `MatchLifetimeScope` |
| `GameLifetimeScope` | `RootLifetimeScope` |
| `SessionLifetimeScope` | `MenuLifetimeScope` (session/menu child) |

Close or mark obsolete any task that invents scopes the code does not use. Finish only registration work that matches Root/Menu/Match (already handled in Tasks 12–13).

- [ ] **Step 3: Commit**

```bash
git add conductor/tracks/system_registration_refactor_20260109/
git commit -m "$(cat <<'EOF'
docs(conductor): reconcile registration track to MatchLifetimeScope names

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

### Phase 3 exit checklist

- [ ] No parent-scope service depends on child-only types
- [ ] Root network primitives not owned by Menu
- [ ] Match services not registered only at Root for convenience
- [ ] Keep-v2 DI-constructed types registered if required
- [ ] Build green

---

## Phase 4 — Verification Gate

### Task 15: Full build + EditMode tests + doc spot-check

**Files:**
- No feature changes; fix only breakages from this track

**Interfaces:**
- Pass bar from spec §9

- [ ] **Step 1: Build Core**

```bash
dotnet build RecipeRage.Core.csproj -nologo
```

Expected: `Build succeeded` with 0 errors.

- [ ] **Step 2: Build remaining project assemblies used by the game**

```bash
ls *.csproj
dotnet build RecipeRage.Gameplay.csproj -nologo
# Also build test project to ensure it compiles:
dotnet build RecipeRage.Tests.EditMode.csproj -nologo
```

Expected: 0 errors on each.

- [ ] **Step 3: Run EditMode tests**

```bash
dotnet test RecipeRage.Tests.EditMode.csproj --no-build -nologo
```

If `--no-build` fails because artifacts missing, run without it:

```bash
dotnet test RecipeRage.Tests.EditMode.csproj -nologo
```

Expected: all tests pass. If failures are from deleted APIs, fix tests (Task 10 style) and re-run — do not skip.

- [ ] **Step 4: DI violation scan**

```bash
rg -n "INetworkObjectPool|INetworkGameManager" Assets/_KitchenClash/Composition/MenuLifetimeScope.cs
rg -n "builder\.Register" Assets/_KitchenClash/Composition/*.cs
```

Expected: Menu scope does not register root network primitives.

- [ ] **Step 5: Doc spot-check**

```bash
rg -n "has not started" wiki/GameplayDesign.md && exit 1 || true
rg -n "Architecture Cleanup|Kitchen Brawler" Documentation/Architecture/PHASE_ROADMAP.md
rg -n "RootLifetimeScope|MenuLifetimeScope|MatchLifetimeScope" Documentation/Architecture/PROJECT_MEMORY.md
```

Expected: GameplayDesign has no “has not started”; roadmap mentions cleanup → Brawler; memory uses code scope names.

- [ ] **Step 6: Final inventory status**

Update `docs/superpowers/plans/2026-07-12-architecture-cleanup-inventory.md` with:

```markdown
## Phase 4 verification

- Build Core: PASS/FAIL
- Build Gameplay: PASS/FAIL
- EditMode tests: PASS/FAIL (N tests)
- DI scan: PASS/FAIL
- Doc spot-check: PASS/FAIL
- Date: YYYY-MM-DD
```

- [ ] **Step 7: Commit verification notes (if inventory changed) or empty-skip**

```bash
git add docs/superpowers/plans/2026-07-12-architecture-cleanup-inventory.md
git commit -m "$(cat <<'EOF'
docs(plan): record architecture cleanup verification results

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

## Follow-on (out of scope — do not implement in this plan)

After Phase 4 passes, open a **separate** plan for Kitchen Brawler vertical slice (recommended: Rush Service 2v2 to playable) using `wiki/GameplayDesign.md`. Do not reopen legacy cooking path.

---

## Self-review (plan author)

### Spec coverage

| Spec section | Tasks |
|--------------|-------|
| §5 Phase 0 inventory | Task 1 |
| §6 Phase 1 wiki | Tasks 2–3 |
| §6 architecture docs | Task 4 |
| §6 root analysis docs | Task 5 |
| §7 purge procedure + categories | Tasks 6–10 |
| §8 DI ownership + conductor | Tasks 11–14 |
| §9 verification | Task 15 |
| §3 non-goals | Global Constraints + every phase reminder |
| §11 commit strategy | Commit steps per task |
| §13 follow-on | Closing section only |

### Placeholder scan

No TBD/TODO implementation steps; dual-path station deletes explicitly gated; Firebase delete explicitly conditional; SessionContainer fixes only where injection path exists.

### Type/name consistency

- Code scopes: `RootLifetimeScope`, `MenuLifetimeScope`, `MatchLifetimeScope` throughout
- Spec’s “SessionLifetimeScope” mapped to `MenuLifetimeScope` in Global Constraints
- Keep-v2 anchors match design doc paths under `Assets/_KitchenClash/`
