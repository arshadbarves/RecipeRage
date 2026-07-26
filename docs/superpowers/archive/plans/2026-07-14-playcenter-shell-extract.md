# Playcenter.Shell Extract Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for tracking.
>
> **Status:** Complete — module @ `d486fdf3`, hard cutover @ `a5bcaf43`, docs follow.

**Goal:** Extract engine-free `Playcenter.Shell` (logging, event bus, connectivity contracts) and fully migrate RecipeRage with zero legacy dual APIs.

**Architecture:** Mirror GameFlow: `Assets/Playcenter/Shell/Runtime/` with `noEngineReferences: true` and zero KitchenClash refs. Unity adapters stay in `KitchenClash.Infrastructure.Logging` / Network. Domain and Application reference Shell; originals deleted.

**Tech Stack:** Unity 6, VContainer, NUnit EditMode tests, existing CLI csproj pattern, `Playcenter.GameFlow` as layout reference.

## Global Constraints

- Full cutover: no type aliases, obsolete stubs, dual namespaces, or `GameLogger` Console fallback.
- `Playcenter.GameFlow` must not reference Shell.
- Do not move Unity adapters, Audio, Async, Platform, cooking Domain, EOS, NGO.
- Do not commit unrelated WIP (maps, fonts, combat, packages-lock, etc.).
- Commit trailer: `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>`
- Spec: `docs/superpowers/specs/2026-07-14-playcenter-shell-extract-design.md`

---

## File map

| Path | Role |
|------|------|
| `Assets/Playcenter/Shell/Runtime/Playcenter.Shell.asmdef` | Engine-free wall |
| `Assets/Playcenter/Shell/Runtime/Logging/*` | `ILoggingService`, `LogLevel`, `LogEntry`, `GameLogger` |
| `Assets/Playcenter/Shell/Runtime/Events/*` | `IEventBus`, `EventBus` |
| `Assets/Playcenter/Shell/Runtime/Connectivity/*` | `IConnectivityService`, `ConnectivityState` |
| `Playcenter.Shell.csproj` | CLI build (gitignored pattern; create for local gate) |
| Delete Domain/Application shell originals | Hard purge |
| Update asmdefs + all consumers | `using Playcenter.Shell` |

---

### Task 1: Create Playcenter.Shell module sources

**Files:**
- Create: `Assets/Playcenter/Shell/Runtime/Playcenter.Shell.asmdef`
- Create: `Assets/Playcenter/Shell/Runtime/Logging/LogLevel.cs`
- Create: `Assets/Playcenter/Shell/Runtime/Logging/LogEntry.cs`
- Create: `Assets/Playcenter/Shell/Runtime/Logging/ILoggingService.cs`
- Create: `Assets/Playcenter/Shell/Runtime/Logging/GameLogger.cs`
- Create: `Assets/Playcenter/Shell/Runtime/Events/IEventBus.cs`
- Create: `Assets/Playcenter/Shell/Runtime/Events/EventBus.cs`
- Create: `Assets/Playcenter/Shell/Runtime/Connectivity/ConnectivityState.cs`
- Create: `Assets/Playcenter/Shell/Runtime/Connectivity/IConnectivityService.cs`
- Create: `Playcenter.Shell.csproj` (CLI)
- Create: matching `.meta` files for Unity (asmdef + folders as needed)

**Interfaces:**
- Produces: all public types in namespace `Playcenter.Shell`

- [x] **Step 1: Create asmdef**

```json
{
    "name": "Playcenter.Shell",
    "rootNamespace": "Playcenter.Shell",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": true
}
```

- [x] **Step 2: Add Logging types** (namespace `Playcenter.Shell`; same members as Domain originals)

`GameLogger` must throw if service unset:

```csharp
public static void Log(string message)
{
    if (_service == null)
        throw new InvalidOperationException(
            "GameLogger has no ILoggingService. Register LoggingBootstrap at root DI before logging.");
    _service.Log(message);
}
// same guard for LogWarning, LogError, LogException
```

- [x] **Step 3: Add Events + Connectivity types** (copy from Domain/Application; namespace `Playcenter.Shell`)

- [x] **Step 4: Create CLI csproj** referencing only netstandard2.1 / language features used by Domain (match `Playcenter.GameFlow.csproj` if present)

- [x] **Step 5: Build Shell**

Run: `dotnet build Playcenter.Shell.csproj -nologo -v q`  
Expected: 0 errors

- [x] **Step 6: Commit**

```bash
git add Assets/Playcenter/Shell Playcenter.Shell.csproj
git commit -m "feat(shell): add Playcenter.Shell module (logging, events, connectivity)"
```

---

### Task 2: Wire Domain/Application to Shell and delete originals

**Files:**
- Modify: `Assets/_KitchenClash/Domain/KitchenClash.Domain.asmdef` — add `"Playcenter.Shell"`
- Modify: `Assets/_KitchenClash/Application/KitchenClash.Application.asmdef` — add `"Playcenter.Shell"`
- Delete: `Assets/_KitchenClash/Domain/Interfaces/ILoggingService.cs`
- Delete: `Assets/_KitchenClash/Domain/Interfaces/IEventBus.cs`
- Delete: `Assets/_KitchenClash/Domain/Interfaces/IConnectivityService.cs`
- Delete: `Assets/_KitchenClash/Domain/Enums/LogLevel.cs`
- Delete: `Assets/_KitchenClash/Domain/Enums/ConnectivityState.cs`
- Delete: `Assets/_KitchenClash/Domain/Models/LogEntry.cs`
- Delete: `Assets/_KitchenClash/Domain/GameLogger.cs`
- Delete: `Assets/_KitchenClash/Application/Services/EventBus.cs`
- Delete: corresponding `.meta` files
- Modify: CLI `KitchenClash.Domain.csproj` / `KitchenClash.Application.csproj` if present — ProjectReference to Shell; remove deleted Compile items

**Interfaces:**
- Consumes: Task 1 types
- Produces: Domain no longer defines shell types

- [x] **Step 1: Update asmdefs**
- [x] **Step 2: Delete originals + metas**
- [x] **Step 3: Update CLI csprojs**
- [x] **Step 4: Commit scaffold** (may not build until Task 3)

```bash
git commit -m "refactor(shell): Domain/Application reference Playcenter.Shell; delete originals"
```

---

### Task 3: Migrate all consumers (usings + DI)

**Files:**
- Modify: every `.cs` that referenced Domain shell types — add `using Playcenter.Shell;`
- Modify: `Assets/_KitchenClash/Infrastructure/Logging/*` — `using Playcenter.Shell`
- Modify: `Assets/_KitchenClash/Infrastructure/Network/NetworkConnectivityService.cs`
- Modify: `Assets/_KitchenClash/Composition/RootLifetimeScope.cs` — register `Playcenter.Shell.EventBus` as `IEventBus`
- Modify: EditMode tests/fakes (`SpyEventBus`, `FakeAppFlow` if needed, logging tests)
- Modify: leaf/mega Infrastructure + Presentation + Application files that use `IEventBus` / `GameLogger` / connectivity

**Mechanical approach:**

```bash
# Find files still needing Domain shell symbols after delete (compile errors)
dotnet build KitchenClash.Domain.csproj
dotnet build KitchenClash.Application.csproj
# Fix file-by-file or scripted: ensure using Playcenter.Shell where GameLogger/IEventBus/etc. used
```

Registration shape (RootLifetimeScope) stays:

```csharp
builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
builder.Register<UnityLoggingService>(Lifetime.Singleton).As<ILoggingService>();
builder.Register<LoggingBootstrap>(Lifetime.Singleton).AsImplementedInterfaces();
builder.Register<NetworkConnectivityService>(Lifetime.Singleton).As<IConnectivityService>().As<ITickable>();
```

Types resolve from `Playcenter.Shell` after usings.

- [x] **Step 1: Fix Logging leaf + Composition**
- [x] **Step 2: Fix Application services that use IEventBus**
- [x] **Step 3: Fix Infrastructure + Presentation + Tests**
- [x] **Step 4: Build chain**

Run:

```bash
dotnet build Playcenter.Shell.csproj -nologo -v q
dotnet build KitchenClash.Domain.csproj -nologo -v q
dotnet build KitchenClash.Application.csproj -nologo -v q
dotnet build KitchenClash.Infrastructure.Logging.csproj -nologo -v q
dotnet build KitchenClash.Infrastructure.csproj -nologo -v q
dotnet build KitchenClash.Composition.csproj -nologo -v q
```

Expected: 0 errors each

- [x] **Step 5: Grep gate — no leftover Domain shell definitions**

```bash
rg -n "interface ILoggingService|interface IEventBus|interface IConnectivityService|static class GameLogger|enum LogLevel|enum ConnectivityState|class LogEntry|class EventBus" Assets/_KitchenClash/Domain Assets/_KitchenClash/Application --glob '*.cs'
```

Expected: no matches (or only unrelated names)

- [x] **Step 6: Commit**

```bash
git commit -m "refactor(shell): migrate RecipeRage consumers to Playcenter.Shell"
```

---

### Task 4: Docs + extract-candidates supersede + final verify

**Files:**
- Modify: `wiki/Technical.md` — Playcenter modules = GameFlow + Shell
- Modify: `wiki/GameFlow-SDK.md` — note Shell sibling; logging types live in Shell
- Modify: `wiki/log.md`
- Modify: `docs/superpowers/plans/2026-07-14-playcenter-module-extract-candidates.md` — banner: Shell extract implemented; remaining candidates still deferred
- Modify: `Assets/Playcenter/Shell/README.md` (short, like GameFlow README)

- [x] **Step 1: Write README + wiki**
- [x] **Step 2: Rebuild full chain**
- [x] **Step 3: Commit**

```bash
git commit -m "docs(shell): document Playcenter.Shell module and migration"
```

---

## Verification checklist

| Check | Expected |
|-------|----------|
| `Playcenter.Shell` asmdef | `noEngineReferences: true`, refs `[]` |
| GameFlow | still zero Shell/KitchenClash refs |
| Domain shell types | deleted |
| GameLogger fallback | none (throws) |
| Builds | Shell → Domain → Application → Logging → Infra → Composition green |
| Unrelated WIP | unstaged |

---

## Spec coverage self-review

| Spec requirement | Task |
|------------------|------|
| Create Shell module | T1 |
| Delete Domain/Application originals | T2 |
| Migrate consumers | T3 |
| GameLogger fail-closed | T1 + T3 |
| Adapters stay game-side | T3 (no move) |
| Docs | T4 |
| No Audio/Async extract | Global constraints |

---

## Execution note (autopilot)

User directed implement fully without legacy. After plan commit, execute Tasks 1–4 in this session (inline), not wait for second-game trigger.
