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

- [ ] **Step 1: Add `LobbyOpResult`** (Success bool + optional error code/message; no Epic types).

- [ ] **Step 2: Change `ILobbyManager` events** from `Action<Result, LobbyInfo>` to `Action<LobbyOpResult, LobbyInfo>`.

- [ ] **Step 3: Replace PlayEveryWare `Lobby` parameters** on `ITeamManager` / `IPlayerManager` with Domain `LobbyInfo` or a dedicated snapshot DTO already used by the game.

- [ ] **Step 4: Update EOS adapters** to map `Epic.OnlineServices.Result` → `LobbyOpResult` at the boundary only.

- [ ] **Step 5: Remove EOS package references** from `KitchenClash.Application.asmdef` when `rg` shows zero Epic/PlayEveryWare usings under Application.

- [ ] **Step 6: Build Application + Infrastructure + Tests; commit.**

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

- [ ] **Step 1: Fix all dead EOS/DI usings** to Application; build Presentation.

- [ ] **Step 2: Fix Localization + Persistence usings.**

- [ ] **Step 3: Animation + Firebase** — Application ports or Presentation-local.

- [ ] **Step 4: Match HUD** — if blocking asmdef drop, add `IMatchHudFacade` in Application with Infrastructure adapter (minimal surface: scores, orders list DTOs, local player ready flag). Full PlayerController split is Phase 4.

- [ ] **Step 5: `rg -l "using KitchenClash\\.Infrastructure" Assets/_KitchenClash/Presentation` → empty.**

- [ ] **Step 6: Commit.**

---

### Task 4: Asmdef delete gates

- [ ] **Step 1:** Edit `KitchenClash.Presentation.asmdef` — remove `KitchenClash.Infrastructure` and `Unity.Netcode.Runtime` if unused.

- [ ] **Step 2:** Edit `KitchenClash.Application.asmdef` — remove EOS refs.

- [ ] **Step 3:** Full `dotnet build` of affected projects + EditMode tests.

- [ ] **Step 4:** Commit + update plan checkboxes / session plan.md.

```bash
git commit -m "$(cat <<'EOF'
refactor(arch): enforce Presentation and Application dependency laws (Phase 1 gate)

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 5: Docs closeout Phase 1

- [ ] Confirm `wiki/Technical.md` dependency laws match code.
- [ ] Note Phase 1 complete in `wiki/log.md`.
- [ ] Point extract-candidates plan at hardening design as superseding shell guidance.
- [ ] Commit docs if not already.

---

## Phase 1 done when

- [x] Design committed  
- [ ] `ISessionContext` in Application, interface-only properties  
- [ ] No Epic/PlayEveryWare in Application sources or asmdef  
- [ ] No `using KitchenClash.Infrastructure` in Presentation  
- [ ] Presentation.asmdef does not reference Infrastructure  
- [ ] EditMode tests green  

## Later phases (not this plan)

- Phase 2: UIService / transitions  
- Phase 3: Infrastructure assembly split  
- Phase 4: Match ports + god files  
