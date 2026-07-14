# GameFlow Module Boundary + Logging Fix — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make product logging visible in Unity (and the debug console), keep GameFlow as a clean reusable module without extracting it from the game yet, and document the boundary so gameplay never owns product navigation.

**Architecture:** `Playcenter.GameFlow` already lives under `Assets/Playcenter/GameFlow` as a zero-game, `noEngineReferences` assembly. Game adapters stay in `Assets/_KitchenClash/Infrastructure/Flow/`. Logging is fixed by wiring the static `GameLogger` facade to the DI `ILoggingService` at Root init; phase transitions also log via `AnalyticsFlowPort`.

**Tech Stack:** Unity 6, VContainer, Playcenter.GameFlow, KitchenClash Domain/Infrastructure

## Global Constraints

- Do **not** extract GameFlow to a separate UPM/git repo until a second game needs it (README already defers this).
- Do **not** move KitchenClash handlers into Playcenter.GameFlow (handlers are game-specific).
- GameFlow must keep **zero** KitchenClash / EOS / NGO / UnityEngine references.
- Product navigation remains **IAppFlow only** (Phase 2 hard purge stands).
- Do not commit unrelated untracked WIP (maps, combat, fonts, ANALYSIS_*, etc.).

---

## Decision: Do we need to “remove GameFlow from gameplay and make it a module”?

### Short answer

**No further extraction is required for production.** GameFlow is already a module.

| Layer | Location | Owns |
|-------|----------|------|
| **Module (reusable)** | `Assets/Playcenter/GameFlow/` (`Playcenter.GameFlow.asmdef`) | `IAppFlow`, `AppFlowController`, ports, policies, DTOs |
| **Game adapters** | `Assets/_KitchenClash/Infrastructure/Flow/` | Ports + handlers (BootSequence, *Phase, SidePhaseFlowPort) |
| **DI wiring** | `Assets/_KitchenClash/Composition/RootLifetimeScope.cs` | Builds controller + ports |
| **Gameplay** | Match/cooking/bots/UI | Calls `IAppFlow` intents only — never owns phase machine |

### What “module” already means here

1. Separate assembly: `Playcenter.GameFlow` with `references: []`, `noEngineReferences: true`.
2. Game code depends on GameFlow; GameFlow never depends on game code.
3. README documents future extract path: move folder → UPM/submodule when a second title needs it.

### What we will **not** do now

- Do not create a new UPM package / external repo.
- Do not delete or relocate `Infrastructure/Flow/Handlers` into Playcenter (those are RecipeRage-specific).
- Do not reintroduce `IGameStateManager`.

### Optional later (out of scope unless requested)

- UPM package `com.playcenter.gameflow` when second game exists.
- Rename `StateMachineFlowPorts.cs` → `MainFlowPorts.cs` (cosmetic; file no longer uses SM).

---

## Root cause: logging “not working”

### Symptom

Handlers and UI call `GameLogger.Log(...)` extensively, but nothing useful appears in the Unity Console / in-game debug console.

### Cause

```
GameLogger (static facade)
  └─ if _service == null → Console.WriteLine  ← invisible in Unity Editor Console
  └─ if _service set     → ILoggingService → UnityEngine.Debug.Log + OnLogAdded
```

`RootLifetimeScope` registers `UnityLoggingService` as `ILoggingService`, but **`GameLogger.SetService` is never called** (only defined). So:

- `GameLogger.*` → `System.Console.WriteLine` (not Unity Console)
- `DebugConsoleUI` injects `ILoggingService` and only shows entries from `OnLogAdded` — which only fires when code calls **`ILoggingService` directly**, not via unwired `GameLogger`
- `AppFlowController` itself has **no** log lines; phase changes only go to analytics (`IAnalyticsService.LogEvent`) unless the analytics port also logs

### Fix strategy

1. **Wire facade at Root init** via `LoggingBootstrap : IInitializable` → `GameLogger.SetService(logging)`.
2. **Surface phase transitions** in Unity Console by logging in `AnalyticsFlowPort.TrackPhaseChanged` (game adapter; keeps GameFlow engine-free).
3. Document: prefer `GameLogger` for product code; inject `ILoggingService` only when you need `OnLogAdded` / export.

---

### Task 1: Wire GameLogger to ILoggingService

**Files:**
- Create: `Assets/_KitchenClash/Infrastructure/Logging/LoggingBootstrap.cs`
- Create: `Assets/_KitchenClash/Infrastructure/Logging/LoggingBootstrap.cs.meta`
- Modify: `Assets/_KitchenClash/Composition/RootLifetimeScope.cs` (RegisterCoreServices)

**Interfaces:**
- Consumes: `ILoggingService`, `GameLogger.SetService`
- Produces: All `GameLogger.*` calls after Root build hit `UnityEngine.Debug` + `OnLogAdded`

- [x] **Step 1: Add LoggingBootstrap**

```csharp
// Assets/_KitchenClash/Infrastructure/Logging/LoggingBootstrap.cs
using KitchenClash.Domain;
using VContainer.Unity;

namespace KitchenClash.Infrastructure.Logging
{
    public sealed class LoggingBootstrap : IInitializable
    {
        private readonly ILoggingService _logging;

        public LoggingBootstrap(ILoggingService logging)
        {
            _logging = logging;
        }

        public void Initialize()
        {
            GameLogger.SetService(_logging);
            _logging.LogInfo("LoggingBootstrap: GameLogger wired to ILoggingService", "Logging");
        }
    }
}
```

- [x] **Step 2: Register at Root before other entry points**

In `RegisterCoreServices`:

```csharp
builder.Register<UnityLoggingService>(Lifetime.Singleton).As<ILoggingService>();
builder.Register<LoggingBootstrap>(Lifetime.Singleton).As<IInitializable>();
```

VContainer runs `IInitializable` before `IStartable`, so `GameBootstrapper.Start` logs will already be wired.

- [x] **Step 3: Build**

```bash
dotnet build KitchenClash.Infrastructure.csproj -nologo
dotnet build KitchenClash.Composition.csproj -nologo
```

Expected: 0 errors.

- [ ] **Step 4: Manual verify in Unity**

Play mode → Console should show:
- `[Logging] LoggingBootstrap: GameLogger wired to ILoggingService`
- `[General] GameBootstrapper starting AppFlow cold boot...`
- `[General] [BootSequence] ...` / `[LoginPhase] ...` as flow runs
- Backtick (`) opens DebugConsole with the same stream

- [x] **Step 5: Commit** (with Task 2)

---

### Task 2: Phase transition visibility (game adapter)

**Files:**
- Modify: `Assets/_KitchenClash/Infrastructure/Flow/StateMachineFlowPorts.cs` (`AnalyticsFlowPort.TrackPhaseChanged`)

**Interfaces:**
- Consumes: `IFlowAnalyticsPort.TrackPhaseChanged` (already called by `AppFlowController.ForceTransitionTo`)
- Produces: Unity Console line `[AppFlow] {from} → {to}` on every phase change

- [x] **Step 1: Log in AnalyticsFlowPort**

```csharp
public void TrackPhaseChanged(FlowPhaseId from, FlowPhaseId to, FlowContext context)
{
    GameLogger.Log($"[AppFlow] {from} → {to}");
    _analytics?.LogEvent("flow_phase_changed", /* ... */);
}
```

Why not inside `AppFlowController`? GameFlow is `noEngineReferences` and must not take KitchenClash `GameLogger`. Optional later: inject `Action<string>` diagnostics into the controller — not needed if analytics port always registered.

- [x] **Step 2: Build + commit**

```bash
dotnet build KitchenClash.Infrastructure.csproj -nologo
```

---

### Task 3: Document module boundary (wiki + README)

**Files:**
- Modify: `Assets/Playcenter/GameFlow/README.md` (production target text still mentions “game states are phase workers”)
- Modify: `wiki/GameFlow-SDK.md` (module vs adapters section if thin)
- Modify: `wiki/log.md`

- [x] **Step 1: README production target** — updated (handlers + logging section)
- [x] **Step 2: wiki/log entry** + GameFlow-SDK Logging / extract sections
- [ ] **Step 3: Commit docs** (with Task 1–2 code)
---

### Task 4: Optional hygiene (same PR if cheap)

- [ ] Rename `StateMachineFlowPorts.cs` → `MainFlowPorts.cs` (update only if no large churn; else leave)
- [ ] Grep gate: `GameLogger.SetService` call sites ≥ 1 (LoggingBootstrap)
- [ ] Grep: no product `IGameStateManager` (still true after hard purge)

---

## Out of scope

| Item | Why |
|------|-----|
| UPM extract of GameFlow | Deferred until second game |
| Moving handlers into Playcenter | Handlers are game-specific |
| Replacing GameLogger with Microsoft.Extensions.Logging | YAGNI |
| Full structured log categories audit | Follow-up |

---

## Verification checklist

| Check | Expected |
|-------|----------|
| `dotnet build` Infrastructure + Composition | 0 errors |
| Play Mode Console | LoggingBootstrap + BootSequence + `[AppFlow] …` lines |
| DebugConsole (`) | Same entries via OnLogAdded |
| Gameplay code | Still only `IAppFlow` for navigation |
| GameFlow asmdef | Still `references: []`, `noEngineReferences: true` |

---

## Self-review

1. **Spec coverage:** Module decision documented; logging root cause + fix; phase visibility; docs.
2. **Placeholders:** None — concrete files and code.
3. **Type consistency:** `LoggingBootstrap(ILoggingService)`, `GameLogger.SetService`, `IInitializable.Initialize`.
