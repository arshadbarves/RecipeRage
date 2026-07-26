# Playcenter.Shell Extract — Design

**Date:** 2026-07-14  
**Branch:** `architecture-cleanup`  
**Status:** Locked under autopilot (user directed full extract + implement; unavailable for mid-flight Q&A)  
**Supersedes (for extract timing):** `docs/superpowers/plans/2026-07-14-playcenter-module-extract-candidates.md` decision “extract later” — product now wants portable Shell **now**, GameFlow-quality, **no legacy dual APIs**.

**Related:** `Assets/Playcenter/GameFlow/`, `wiki/GameFlow-SDK.md`, architecture hardening (complete).

---

## 1. Problem

Only **GameFlow** lives under `Assets/Playcenter/`. Generic shell contracts still sit in `KitchenClash.Domain`:

| Type | Today | Problem |
|------|-------|---------|
| `ILoggingService`, `LogLevel`, `LogEntry`, `GameLogger` | Domain | Second game cannot reuse without KitchenClash Domain |
| `IEventBus` + `EventBus` | Domain + Application | Pure pub/sub buried in game assemblies |
| `IConnectivityService` + `ConnectivityState` | Domain | Shared Brawl offline/host-drop contract mixed with cooking Domain |
| `GameLogger` Console fallback | Domain | Dual path: DI service **or** `Console.WriteLine` — hides missing bootstrap |

Architecture hardening made **KitchenClash leaf assemblies** (Logging, Audio, …). Those are **not** portable Playcenter modules.

**Goal:** Extract a **Playcenter.Shell** module mirroring GameFlow rules so another Brawl-class title can depend on Shell without any KitchenClash reference. Full cutover in RecipeRage — **delete** Domain/Application originals; **no** type aliases, dual namespaces, or Console fallback.

---

## 2. Locked decisions (autopilot)

1. **Scope this program:** `Playcenter.Shell` only — Logging contracts + facade, EventBus, Connectivity contracts.
2. **Not in this program:** Audio, Async, Platform, Config, Analytics, UI stack, Session/Lobby, EOS, Network leaf, cooking Domain split.
3. **Pattern:** Same as GameFlow — engine-free Playcenter assembly; Unity/network adapters stay in KitchenClash Infrastructure.
4. **Hard cutover:** No `KitchenClash.Domain` shims, no `using GameLogger = …`, no obsolete wrappers, no Console fallback on `GameLogger`.
5. **In-repo module** under `Assets/Playcenter/Shell/` (not UPM yet). Future: same extract path as GameFlow README (submodule/UPM when game #2 is real).
6. **Domain may reference Shell** — Domain keeps cooking models; shell ports leave Domain.
7. **GameFlow stays independent** — `Playcenter.GameFlow` does **not** reference Shell (GameFlow remains zero-deps). Game adapters may use both.

---

## 3. Approaches considered

| Approach | Description | Pros | Cons |
|----------|-------------|------|------|
| **A. Playcenter.Shell (recommended)** | One engine-free assembly: logging + events + connectivity contracts + pure EventBus + GameLogger | Matches prior shortlist; one migration; clear second-game surface | Medium blast radius (`GameLogger` ~100 files, `IEventBus` ~55) |
| B. Multiple Playcenter modules now | Shell + Logging.Unity + Audio.Core + … | Maximum modularity | Over-scope; Audio is Unity-bound; multi-asmdef churn without second consumer |
| C. Domain.Shell folder only | Namespace hygiene, no Playcenter | Low risk | **Does not** meet “like GameFlow / separate for another game” |

**Chosen: A.**

---

## 4. Target architecture

```
Assets/Playcenter/
  GameFlow/     (unchanged — zero refs)
  Shell/        (NEW)
    Runtime/
      Playcenter.Shell.asmdef   # noEngineReferences: true, references: []
      Logging/
        ILoggingService.cs
        LogLevel.cs
        LogEntry.cs
        GameLogger.cs           # requires SetService; no Console fallback
      Events/
        IEventBus.cs
        EventBus.cs             # pure implementation (moved from Application)
      Connectivity/
        IConnectivityService.cs
        ConnectivityState.cs

Assets/_KitchenClash/
  Domain/                       # DELETE shell types; asmdef refs Playcenter.Shell
  Application/                  # DELETE EventBus.cs; asmdef refs Playcenter.Shell
  Infrastructure/Logging/       # UnityLoggingService implements Playcenter.Shell.ILoggingService
  Infrastructure/Network/       # NetworkConnectivityService implements Playcenter.Shell.IConnectivityService
  Composition/                  # usings → Playcenter.Shell; DI unchanged shape
```

### Dependency graph (compile-time)

```
Playcenter.Shell          ← no deps, no Unity
Playcenter.GameFlow       ← no deps (unchanged)
KitchenClash.Domain       ← Playcenter.Shell
KitchenClash.Application  ← Domain, Playcenter.Shell
KitchenClash.Infrastructure.Logging ← Domain (if needed), Playcenter.Shell, Unity
KitchenClash.Infrastructure (mega)  ← …, Playcenter.Shell
KitchenClash.Composition  ← …, Playcenter.Shell
```

### What stays game-side (adapters)

| Adapter | Assembly | Implements |
|---------|----------|------------|
| `UnityLoggingService` | `KitchenClash.Infrastructure.Logging` | `ILoggingService` |
| `LoggingBootstrap` | same | wires `GameLogger.SetService` |
| `NetworkConnectivityService` | mega Infrastructure / Network | `IConnectivityService` + Unity reachability |

---

## 5. Public API (Playcenter.Shell)

### Logging

```csharp
namespace Playcenter.Shell
{
    public enum LogLevel { Verbose, Info, Warning, Error, Critical }

    public sealed class LogEntry { /* message, level, category, timestamp, stackTrace */ }

    public interface ILoggingService : IDisposable
    {
        event Action<LogEntry> OnLogAdded;
        void Log(string message, LogLevel level = LogLevel.Info, string category = "General");
        void LogInfo(string message, string category = "General");
        void LogWarning(string message, string category = "General");
        void LogError(string message, string category = "General");
        void LogException(Exception exception, string category = "General");
        LogEntry[] GetLogs();
        void ClearLogs();
        void SaveLogsToFile(string filePath);
    }

    /// <summary>
    /// Static facade. Must call SetService from game LoggingBootstrap before use.
    /// Missing service → InvalidOperationException (no Console fallback).
    /// </summary>
    public static class GameLogger { /* SetService, Log, LogInfo, LogWarning, LogError, LogException */ }
}
```

### Events

```csharp
namespace Playcenter.Shell
{
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> handler) where T : class;
        void Unsubscribe<T>(Action<T> handler) where T : class;
        void Publish<T>(T eventData) where T : class;
        void ClearAllSubscriptions();
    }

    public sealed class EventBus : IEventBus { /* pure dict of handlers */ }
}
```

### Connectivity

```csharp
namespace Playcenter.Shell
{
    public enum ConnectivityState { Online, OfflineMenu, OfflineMatch, HostDropped }

    public interface IConnectivityService
    {
        bool IsOnline { get; }
        ConnectivityState CurrentState { get; }
        event Action<bool> OnConnectivityChanged;
        event Action<bool> OnConnectionStatusChanged;
        event Action<ConnectivityState> OnStateChanged;
        void NotifyMatchStarted();
        void NotifyMatchEnded();
        void NotifyHostDropped();
    }
}
```

API surface matches today’s Domain contracts so migration is **namespace + assembly ref** only (except GameLogger fail-closed).

---

## 6. Migration rules (no legacy)

1. **Move** types into `Playcenter.Shell` (same members; new namespace).
2. **Delete** originals under Domain/Application — do not leave obsolete stubs.
3. **Update** every consumer: `using Playcenter.Shell;` where shell types are used. Files that still need cooking Domain keep `using KitchenClash.Domain;`.
4. **Domain.asmdef** gains reference to `Playcenter.Shell`.
5. **Application.asmdef** gains reference to `Playcenter.Shell`; remove `EventBus.cs` from Application.
6. **CLI csproj** (if present): add `Playcenter.Shell.csproj`; Domain/Application project refs updated.
7. **Tests:** EditMode fakes/spies that implement `IEventBus` / logging use `Playcenter.Shell`.
8. **GameLogger:** remove `Console.WriteLine` branches; if `_service == null`, throw `InvalidOperationException` with message to register via `LoggingBootstrap`.
9. **Bootstrap order:** Root must register `LoggingBootstrap` as `IInitializable` before any `IStartable` that logs (already true with VContainer). Document as invariant.
10. **Wiki:** `wiki/GameFlow-SDK.md` / `wiki/Technical.md` — Playcenter modules = GameFlow + Shell; update extract-candidates plan status to superseded for Shell.

---

## 7. Explicit non-goals

- Do **not** put `UnityLoggingService` or `NetworkConnectivityService` into Playcenter.
- Do **not** move cooking events, `IAudioService`, economy, EOS, NGO types.
- Do **not** make GameFlow depend on Shell.
- Do **not** keep `KitchenClash.Domain.ILoggingService` as alias.
- Do **not** extract Audio/Async/Platform in this program.

---

## 8. Error handling & testing

| Case | Behavior |
|------|----------|
| `GameLogger` before `SetService` | `InvalidOperationException` |
| `EventBus.Publish` with no subscribers | no-op (unchanged) |
| `EventBus` handler throws | propagate (unchanged — no swallow) |
| Connectivity adapter offline | game `NetworkConnectivityService` owns timers/forfeit events (extra events stay on concrete type if not on interface) |

**Tests:**

- Unit: `EventBus` subscribe/publish/unsubscribe/clear (move or retarget existing if any).
- Unit: `GameLogger` throws without service; delegates after `SetService`.
- Build gate: `Playcenter.Shell`, Domain, Application, Infrastructure.Logging, Composition — 0 errors.
- No dual-type compile: grepping `namespace KitchenClash.Domain` must not find shell type definitions.

---

## 9. Success criteria

1. `Assets/Playcenter/Shell/` exists with `Playcenter.Shell.asmdef`, `noEngineReferences: true`, **zero** KitchenClash refs.
2. Domain/Application no longer define shell types; originals deleted.
3. RecipeRage DI still registers `UnityLoggingService` as `ILoggingService`, `EventBus` as `IEventBus`, `NetworkConnectivityService` as `IConnectivityService`.
4. `GameLogger` has no Console fallback.
5. CLI builds green for Shell → Domain → Application → Logging → Composition.
6. Wiki documents Shell as second Playcenter module.
7. Unrelated WIP remains uncommitted.

---

## 10. Implementation phases (for plan)

| Phase | Work |
|-------|------|
| P0 | Create `Playcenter.Shell` sources + asmdef + CLI csproj |
| P1 | Point Domain/Application asmdefs at Shell; delete moved files; fix Application EventBus registration namespace |
| P2 | Bulk update usings across codebase + tests |
| P3 | GameLogger fail-closed; LoggingBootstrap/UnityLoggingService namespaces |
| P4 | Docs (wiki, supersede extract-candidates note); verify builds; commit |

---

## 11. Risk

| Risk | Mitigation |
|------|------------|
| Large using churn | Mechanical; compile-driven |
| Early log before bootstrap | Fail-closed + keep LoggingBootstrap as first IInitializable |
| Domain events still in Domain | Fine — EventBus is generic; event **types** stay game Domain |
| Second program needed for Audio etc. | Explicit non-goal; separate design later |

---

## 12. Self-review

- No TBD placeholders for in-scope API.
- Scope is one module; not multi-module mega-extract.
- Hard cutover rules are explicit (no dual API).
- Matches GameFlow layout and dependency laws.
