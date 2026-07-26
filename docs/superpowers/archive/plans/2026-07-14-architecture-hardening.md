# Architecture Hardening — Phase 1 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Enforce Clean Architecture dependency laws for the product shell: Presentation compiles without Infrastructure; Application ports have no EOS/PlayEveryWare types; `ISessionContext` lives in Application.

**Architecture:** Approach A from `docs/superpowers/specs/2026-07-14-architecture-hardening-design.md` — move contracts up, strip wrong-direction usings, de-vendor Application interfaces, then drop asmdef references.

**Tech Stack:** Unity 6, VContainer, NUnit EditMode, existing KitchenClash assemblies, Playcenter.GameFlow (unchanged).

**Out of scope this plan:** Infrastructure asmdef split (Phase 3), PlayerController split (Phase 4), new Playcenter modules, gameplay features.

---

## File map (Phase 1)

| Action | Path |
|--------|------|
| Move interface | `Infrastructure/DI/ISessionContext.cs` → `Application/Interfaces/ISessionContext.cs` (namespace `KitchenClash.Application`) |
| Edit impl | `Infrastructure/DI/SessionContext.cs` — implement Application interface; expose interfaces only |
| Edit | `Application/Interfaces/ILobbyManager.cs` — remove `Epic.OnlineServices.Result` |
| Add | `Application/Models/LobbyOpResult.cs` (or Domain) — success/failure for lobby ops |
| Edit | `Application/Interfaces/ITeamManager.cs` — Domain lobby DTO, not PlayEveryWare Lobby |
| Edit | `Application/Interfaces/IPlayerManager.cs` — same |
| Edit | All Presentation files with `using KitchenClash.Infrastructure.*` |
| Edit | `Presentation/KitchenClash.Presentation.asmdef` — remove Infrastructure (+ Netcode if unused) |
| Edit | `Application/KitchenClash.Application.asmdef` — remove EOS package refs when clean |
| Edit | EOS/Infra lobby adapters mapping `Result` → `LobbyOpResult` |
| Docs | wiki already updated; CLAUDE.md dependency note if needed |

---

### Task 1: Application-owned `ISessionContext` + interface-only facade

**Files:**
- Create: `Assets/_KitchenClash/Application/Interfaces/ISessionContext.cs`
- Modify: `Assets/_KitchenClash/Infrastructure/DI/SessionContext.cs`
- Delete: `Assets/_KitchenClash/Infrastructure/DI/ISessionContext.cs` (+ .meta if present)
- Test: `Assets/Scripts/Tests/EditMode/SessionContextContractTests.cs` (optional compile-shape test)

- [x] **Step 1: Write failing test** — assert `ISessionContext` is in `KitchenClash.Application` and exposes only interfaces (reflection on property types).

```csharp
using System.Linq;
using System.Reflection;
using KitchenClash.Application;
using NUnit.Framework;

namespace KitchenClash.Tests.EditMode
{
    public class SessionContextContractTests
    {
        [Test]
        public void ISessionContext_LivesInApplication_AndExposesOnlyInterfaces()
        {
            var t = typeof(ISessionContext);
            Assert.AreEqual("KitchenClash.Application", t.Namespace);

            foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (p.Name == nameof(ISessionContext.IsSessionActive)) continue;
                Assert.True(p.PropertyType.IsInterface,
                    $"{p.Name} must be an interface, was {p.PropertyType.Name}");
            }
        }
    }
}
```

- [x] **Step 2: Run test — expect fail** (type missing or wrong namespace / concrete properties).

```bash
dotnet build RecipeRage.Tests.EditMode.csproj -nologo
dotnet test RecipeRage.Tests.EditMode.csproj --filter="SessionContextContractTests" --no-build -nologo
```

- [x] **Step 3: Implement Application `ISessionContext`**

```csharp
using KitchenClash.Application.Services;
using KitchenClash.Domain;

namespace KitchenClash.Application
{
    public interface ISessionContext
    {
        bool IsSessionActive { get; }
        IGameModeService GameModeService { get; }
        ICharacterService CharacterService { get; }
        ISkinsService SkinsService { get; }
        IGameStarter GameStarter { get; }
        IEconomyService EconomyService { get; }
        IPlayerDataService PlayerDataService { get; }
        IFriendsService FriendsService { get; }
        ILobbyManager LobbyManager { get; }
        IMatchmakingService MatchmakingService { get; }
        T Resolve<T>() where T : class;
    }
}
```

- [x] **Step 4: Update `SessionContext` impl** to use `IEconomyService` / `IPlayerDataService`; delete old interface file; fix all `using KitchenClash.Infrastructure.DI` → `KitchenClash.Application` for this type.

- [x] **Step 5: Run test — expect pass.**

- [x] **Step 6: Commit**

```bash
git add Assets/_KitchenClash/Application/Interfaces/ISessionContext.cs \
  Assets/_KitchenClash/Infrastructure/DI/SessionContext.cs \
  Assets/_KitchenClash/Infrastructure/DI/ISessionContext.cs \
  Assets/Scripts/Tests/EditMode/SessionContextContractTests.cs
git commit -m "$(cat <<'EOF'
refactor(session): move ISessionContext to Application with interface-only facade

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 2: De-EOS Application lobby/team/player ports

**Files:**
- Create: `Assets/_KitchenClash/Application/Models/LobbyOpResult.cs` (or Domain)
- Modify: `ILobbyManager.cs`, `ITeamManager.cs`, `IPlayerManager.cs`
- Modify: Infrastructure implementations + call sites
- Test: mapping unit test if pure mapper extracted

- [x] **Step 1: Add `LobbyOpResult`** (Success bool + optional error code/message; no Epic types).

- [x] **Step 2: Change `ILobbyManager` events** from `Action<Result, LobbyInfo>` to `Action<LobbyOpResult, LobbyInfo>`.

- [x] **Step 3: Replace PlayEveryWare `Lobby` parameters** on `ITeamManager` / `IPlayerManager` with Domain `LobbyInfo` or a dedicated snapshot DTO already used by the game.

- [x] **Step 4: Update EOS adapters** to map `Epic.OnlineServices.Result` → `LobbyOpResult` at the boundary only.

- [x] **Step 5: Remove EOS package references** from `KitchenClash.Application.asmdef` when `rg` shows zero Epic/PlayEveryWare usings under Application.

- [x] **Step 6: Build Application + Infrastructure + Tests; commit.**

```bash
dotnet build RecipeRage.Core.csproj -nologo
# or relevant csproj set
git commit -m "$(cat <<'EOF'
refactor(session): remove EOS types from Application lobby/team ports

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 3: Strip Presentation → Infrastructure usings (dead + real)

**Files:** 19 Presentation files listed in design inventory.

Priority order:

1. **Dead usings only** (LobbyViewModel, MatchmakingViewModel, ShopViewModel, etc. that only needed `ISessionContext`) — change namespace usings; no logic change.
2. **Localization** — `using KitchenClash.Application.Services` for `ILocalizationManager`, not Infrastructure.Localization.
3. **Persistence** — inject `IPlayerDataService` from Application; drop Infrastructure.Persistence usings.
4. **Animation** — introduce `IUITransitionService` in Application if needed, or move transition helpers into Presentation if pure UI Toolkit.
5. **GameplayHudViewModel / ResultsScreen** — introduce thin Application match read ports **or** temporary keep Network ref only for these files and document exception (prefer ports; if too large, split Task 3b).

- [x] **Step 1: Fix all dead EOS/DI usings** to Application; build Presentation.

- [x] **Step 2: Fix Localization + Persistence usings.**

- [x] **Step 3: Animation + Firebase** — Presentation-local `TweenExtensions` (DOTween→UniTask); dead Firebase usings stripped.

- [x] **Step 4: Match HUD** — `IMatchHudPort` + Domain `MatchResultSnapshot`; Infra `MatchHudPort` adapter; Root `NullMatchHudPort`; Match-scoped real port. `ICharacterPreviewService` + null default + Menu register-if-present.

- [x] **Step 5: `rg -l "using KitchenClash\\.Infrastructure" Assets/_KitchenClash/Presentation` → empty.**

- [x] **Step 6: Commit.**

---

### Task 4: Asmdef delete gates

- [x] **Step 1:** Edit `KitchenClash.Presentation.asmdef` — remove `KitchenClash.Infrastructure` and `Unity.Netcode.Runtime` if unused.

- [x] **Step 2:** Edit `KitchenClash.Application.asmdef` — remove EOS refs.

- [x] **Step 3:** Full `dotnet build` of affected projects + EditMode tests.

- [x] **Step 4:** Commit + update plan checkboxes / session plan.md.

```bash
git commit -m "$(cat <<'EOF'
refactor(arch): enforce Presentation and Application dependency laws (Phase 1 gate)

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 5: Docs closeout Phase 1

- [x] Confirm `wiki/Technical.md` dependency laws match code.
- [x] Note Phase 1 complete in `wiki/log.md`.
- [x] Point extract-candidates plan at hardening design as superseding shell guidance.
- [x] Commit docs if not already.

---

## Phase 1 done when

- [x] Design committed  
- [x] `ISessionContext` in Application, interface-only properties  
- [x] No Epic/PlayEveryWare in Application sources or asmdef  
- [x] No `using KitchenClash.Infrastructure` in Presentation  
- [x] Presentation.asmdef does not reference Infrastructure  
- [x] EditMode tests green (CLI compile of EditMode project; Unity Test Runner for execution)

## Later phases (not this plan)

- Phase 2: UIService / transitions — **complete** (see below)
- Phase 3: Infrastructure assembly split — **3a + 3b + 3c complete** (see below); Network/EOS still mega  
- Phase 4: Match ports + god files — **complete**

---

## Phase 2 — UI navigation purity (complete)

**Goal:** Document UIService responsibilities; shrink god-file; confirm animation/localization/maintenance already port-clean.

### Audit results (no new ports required)

| Area | Finding |
|------|---------|
| Presentation → Infrastructure | Already zero after Phase 1 |
| Localization | Application `ILocalizationManager`; impl in Infra |
| Maintenance | Application `IMaintenanceService` (Domain); no Firebase usings in Presentation |
| Animation | Presentation-local `TweenExtensions` + `UITransitionHandler` — no Infra.Animation |
| Screen open paths | All via Application `IUIService` |

### Task 2.1: Document UIService responsibilities

- [x] Class-level summary on `UIService`: document/layer setup, screen resolve, category navigation, toast host; stack in `IUIScreenStackManager`; transitions in controllers + `UITransitionHandler`.

### Task 2.2: Split UIService into partials (god-file shrink)

- [x] `UIService.cs` — fields, ctor, Start/init, layers, dispose, tick (~206 lines)
- [x] `UIService.Navigation.cs` — public `IUIService` navigation/toast API (~208 lines)
- [x] `UIService.ScreenOps.cs` — resolve/show/hide internals (~134 lines)
- [x] `dotnet build KitchenClash.Presentation.csproj` green

### Phase 2 done when

- [x] UIService responsibilities documented
- [x] No Presentation → Infrastructure imports (Phase 1 gate held)
- [x] UIService primary file reduced via partials; total logic unchanged
- [x] Animation/localization/maintenance confirmed port-clean



---

## Phase 3 — Infrastructure assembly walls (3a + 3b + 3c complete)

**Goal:** Split leaf Infrastructure folders into compile-time assemblies; break Network↔EOS / Persistence→EOS / Flow→Network cycles with Application ports; extract Persistence / Audio / Flow as leaves once port-clean.

### Leaf assemblies (3a) — complete

| Assembly | Folder | Key types | Refs |
|----------|--------|-----------|------|
| `KitchenClash.Infrastructure.Logging` | Logging/ | `UnityLoggingService`, `LoggingBootstrap` | Domain, VContainer |
| `KitchenClash.Infrastructure.Localization` | Localization/ | `LocalizationManager` | Domain, Application, VContainer |
| `KitchenClash.Infrastructure.Animation` | Animation/ | `AnimationService`, DOTween animators | Domain, Application, UniTask, DOTween |
| `KitchenClash.Infrastructure.Configuration` | Configuration/ | `GameConstants`, `GameSettingsConfig`, `UGSConfig` | Domain |
| `KitchenClash.Infrastructure.Platform` | Platform/ | `PlatformUtils`, `CoroutineRunner` | (Unity only) |
| `KitchenClash.Infrastructure.Async` | Async/ | `TaskExtensions` | Domain |

- [x] Create leaf `.asmdef` files under each folder (folder-level asmdef auto-excludes from mega Infrastructure)
- [x] Mega `KitchenClash.Infrastructure.asmdef` references Configuration, Platform, Async (consumers of GameConstants / PlatformUtils)
- [x] `KitchenClash.Composition.asmdef` references all six leaves (DI registration types)
- [x] `RecipeRage.Editor.asmdef` references Configuration (`GameConstants` for MapSceneGenerator)
- [x] CLI csproj ProjectReferences for leaves; mega Compile excludes leaf sources
- [x] Register `AnimationService` + DOTween animators in `RootLifetimeScope` (was missing)
- [x] `dotnet build` Domain → leaves → Infrastructure → Presentation → Composition → EditMode green

### Phase 3b — Application ports + Persistence leaf (complete)

**Cycle-breaking ports (Application):**

| Port | Purpose | EOS / Infra adapter |
|------|---------|---------------------|
| `ICloudStorageProvider` | Cloud save lifecycle + `IStorageProvider` | `EOSCloudStorageProvider` |
| `IFriendsServiceFactory` | Create friends service without Network→EOS | `EOSFriendsServiceFactory` |
| `ILocalNetworkIdentity` | Local user id string for host checks | `EOSLocalNetworkIdentity` |
| `IClientTransportConfigurator` | Configure client host connection | `EOSClientTransportConfigurator` |
| `IMatchHudPort` (Phase 1) | Results/HUD without `IMatchContext` | `MatchHudPort` |

**Source-level edges fixed:**

- [x] `UGSConfig` moved Network → Configuration (namespace `KitchenClash.Infrastructure.Configuration`)
- [x] `StorageProviderFactory` injects `ICloudStorageProvider` (no `new EOS…`)
- [x] `SaveService` uses `ICloudStorageProvider` lifecycle (no EOS cast)
- [x] `NetworkingServiceContainer` uses `IFriendsServiceFactory` + identity/transport ports; no EOS usings
- [x] `GameStarter` uses `ILocalNetworkIdentity` + `IClientTransportConfigurator`; no Epic/PlayEveryWare usings
- [x] `ResultsPhase` depends on `IMatchHudPort` + `IEconomyService` only (no Network / `IMatchContext`)
- [x] `RootLifetimeScope` registers cloud provider, friends factory, identity, transport configurator

**Persistence leaf assembly:**

| Assembly | Folder | Refs |
|----------|--------|------|
| `KitchenClash.Infrastructure.Persistence` | Persistence/ | Domain, Application, UniTask |

- [x] Persistence `.asmdef` + mega Infrastructure / Composition references
- [x] CLI csproj ProjectReferences; mega Compile excludes Persistence sources
- [x] EditMode tests drop unused `Infrastructure.Persistence` usings
- [x] `dotnet build` Persistence → Infrastructure → Composition → EditMode green

### Phase 3c — Audio + Flow leaf assemblies (complete)

**Edges fixed before walls:**

- [x] `CoroutineRunner` moved Network → Platform (namespace `KitchenClash.Infrastructure.Platform`)
- [x] Application port `ISessionLifecycle` (`CreateSession` / `DestroySession` / `IsSessionActive`)
- [x] `SessionManager` implements `ISessionLifecycle`; `SessionLoader` depends on port only
- [x] Dead `using Infrastructure.DI` stripped from MatchmakingPhase / MatchRuntimePhase
- [x] `ForceUpdateChecker` moved Services → Flow.Handlers; BootSequence no longer uses Infrastructure.Services
- [x] `ForceUpdateChecker` uses Configuration `GameSettingsConfig` fallback

**Leaf assemblies:**

| Assembly | Folder | Refs |
|----------|--------|------|
| `KitchenClash.Infrastructure.Audio` | Audio/ | Domain, Application, Platform, VContainer |
| `KitchenClash.Infrastructure.Flow` | Flow/ | Domain, Application, Configuration, GameFlow, UniTask, VContainer |

- [x] Audio + Flow `.asmdef` + mega Infrastructure / Composition references
- [x] CLI csproj ProjectReferences; mega Compile excludes Audio/Flow sources
- [x] EditMode tests reference Flow leaf (`MatchmakingPhase`)
- [x] `RootLifetimeScope` registers `ISessionLifecycle` + SessionLoader wiring
- [x] `dotnet build` Application → Platform → Audio → Flow → Infrastructure → Composition → EditMode green

### Phase 3d — EOS leaf assembly (complete)

| Assembly | Folder | Refs |
|----------|--------|------|
| `KitchenClash.Infrastructure.EOS` | EOS/ | Domain, Application, Configuration, UniTask, Netcode, EOS/UGS packages |

- [x] EOS `.asmdef` + Composition reference (mega does **not** reference EOS)
- [x] CLI csproj; mega Compile excludes EOS sources; drop PlayEveryWare/Epic/Friends from mega
- [x] `SlideDirection` Domain → Animation (Phase 5 hygiene)
- [x] Dead `using Infrastructure.DI` stripped from `MatchEndController`
- [x] `dotnet build` Domain → Animation → EOS → Infrastructure → Composition green

### Still deferred (further walls)

| Candidate | Blocker |
|-----------|---------|
| Network separate asmdef | Network↔Gameplay cycle (`PlayerController` / stations ↔ abilities / validators) |
| Broader Phase 5 Domain kernel | Optional; only `SlideDirection` fixed |

**Next:** Network ports if Network leaf is required; Unity smoke; PR after gh auth.

---

## Phase 4 — Match gameplay ports + god-file shrink (complete for scoped criteria)

**Goal:** Presentation stays free of Network concretes; shrink `PlayerController`; confirm `BotTaskPlanner` has no Infrastructure deps.

### Inventory

| Item | Finding |
|------|---------|
| Presentation → Network / Infrastructure | Already zero (Phase 1 gate held) |
| Match HUD | Application `IMatchHudPort` + Domain `MatchResultSnapshot` (Phase 1) |
| `BotTaskPlanner` | Application service; **Domain-only** usings — no move required |
| `PlayerController` | Was ~994 lines; already had SOLID collaborators; skins/carry/class bulk remained |

### Task 4.1: Split PlayerController into partials

- [x] `PlayerController.cs` — fields, lifecycle, init, network spawn/despawn, public API, IInteractable (~393 lines, under ~400 target)
- [x] `PlayerController.InputMovement.cs` — input setup, movement processing, network RPCs
- [x] `PlayerController.Character.cs` — character class / ability registration
- [x] `PlayerController.Skins.cs` — skin NetworkVariable + apply/cleanup
- [x] `PlayerController.Carrying.cs` — hold point + dish carry list
- [x] Class-level summary documents partials + collaborators
- [x] `dotnet build KitchenClash.Infrastructure.csproj` green
- [x] `dotnet build RecipeRage.Tests.EditMode.csproj` green

### Task 4.2: BotTaskPlanner placement

- [x] Confirmed Domain-only (`KitchenClash.Domain`); stays in Application as pure planner over `BotPlanningSnapshot`
- [x] No Infrastructure / Network usings — design success criterion met without relocation

### Phase 4 done when

- [x] Presentation has zero Network usings
- [x] PlayerController primary file &lt; ~400 lines (partials)
- [x] BotTaskPlanner not in Application with Infra deps
- [x] Match HUD remains on Application ports (no expansion needed for Presentation)

**Deferred (optional follow-ups):** further collaborator extraction of skins/carry into non-partial classes; Application ports for non-HUD match consumers; Network leaf after Gameplay ports.

---

## Phase 5 — Domain kernel hygiene (partial / closed)

**Design:** optional follow-on only if Domain remains noisy after Phases 1–4. Not required to claim hardened shell.

### Done
- [x] `SlideDirection` moved Domain → Animation leaf (wrong assembly; UI tween enum)
- [x] Domain remains `noEngineReferences: true` with zero Unity usings
- [x] Dead Domain type `UpdateUrgency` left in place (unused; delete only if product confirms)

### Explicitly deferred (not required)
- Split Domain into shell ports vs cooking models
- Relocate Presentation-only `UIScreenCategory` (consumers are Presentation; Domain enum is fine as shared contract)
- Relocate Audio enums (`MusicTrack` / `SFXType`) — used by Domain events + Application services

### Program status
Required Approach A phases **1–4 + 3a–3d** are complete. Remaining optional work:
1. Network leaf after Network↔Gameplay ports
2. Broader Domain kernel split
3. Unity Editor smoke (boot → login → home → play → match HUD → results)
4. PR after `gh` auth


