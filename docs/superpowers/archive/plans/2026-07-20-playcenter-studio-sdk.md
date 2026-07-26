# Playcenter Studio SDK Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship a complete-replacement Playcenter Studio SDK (`Playcenter.SDK`) with ServiceRegistry DI (no VContainer in Playcenter), ordered module boot + loading shell, themable SDK screens, vendor firewall, RecipeRage `IGameEntry` cutover, and an AI skill — deleting `BootSequence` and cold-boot-via-AppFlow.

**Architecture:** Public facade assembly `Playcenter.SDK` owns `PlaycenterClient`, `IServiceRegistry`, `ModuleHost`, `IBootProgress`, `IShellUi`, and `IGameEntry` handoff. Default modules replace KitchenClash `BootSequence`. Game keeps VContainer only for game IP and bridges ports from `client.Services`. Session ownership (CreateSession-only) stays. Spec: `docs/superpowers/specs/2026-07-20-playcenter-studio-sdk-design.md` (commit `d9b486fd`).

**Tech Stack:** Unity 6, UniTask (game/Infrastructure only), UI Toolkit, NUnit EditMode, existing Playcenter.Shell/Services/GameFlow/UI*/EOS, NGO+EOS behind adapters, no VContainer inside `Assets/Playcenter/**`.

## Global Constraints

- Spec is law: `docs/superpowers/specs/2026-07-20-playcenter-studio-sdk-design.md`
- **S2:** Zero `using VContainer` / `VContainer.` types under `Assets/Playcenter/**` (comments mentioning VContainer as “game supplies factory” must not become real refs)
- **S5:** No interactive login mid loading bar — auth UI only after `OnPlaycenterReady`
- **S6:** SDK never calls `IAppFlow` internally
- **S8/S13:** Hard cutover — no dual boot, no feature flag keeping `BootSequence` after Task 5 delete gate
- **S9:** SESSION only via `CreateSession` + installer; do not reopen orphan session DI
- Pure modules/registry use `System.Threading.Tasks.Task` (not UniTask) so `Playcenter.SDK` core can stay testable
- Test naming: `MethodName_Condition_ExpectedResult`
- Build/test: `dotnet build <csproj> -nologo` then `dotnet test RecipeRage.Tests.EditMode.csproj --filter="..." -nologo` (Unity may need to regenerate csproj after new asmdefs — open Editor or use existing test asmdef reference update)
- Do **not** commit UserSettings, Library, Android build/, analysis dumps, `.claude/`, gitignored csproj churn unless required
- Commit trailer: `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>`
- Prefer small focused files; public API only in `Playcenter.SDK`

---

## File Map

### Create

| Path | Responsibility |
|------|----------------|
| `Assets/Playcenter/SDK/Runtime/Playcenter.SDK.asmdef` | Public facade asmdef |
| `Assets/Playcenter/SDK/Runtime/PlaycenterClient.cs` | Host entry `Create` / `RunAsync` |
| `Assets/Playcenter/SDK/Runtime/ClientOptions.cs` | Builder options |
| `Assets/Playcenter/SDK/Runtime/DI/IServiceRegistry.cs` | Register API |
| `Assets/Playcenter/SDK/Runtime/DI/IServiceProvider.cs` | Resolve API (name: `IPlaycenterServices` if clash) |
| `Assets/Playcenter/SDK/Runtime/DI/ServiceRegistry.cs` | Implementation |
| `Assets/Playcenter/SDK/Runtime/Modules/IPlaycenterModule.cs` | Module contract |
| `Assets/Playcenter/SDK/Runtime/Modules/ModuleContext.cs` | Per-module context |
| `Assets/Playcenter/SDK/Runtime/Modules/ModuleHost.cs` | Ordered run / retry |
| `Assets/Playcenter/SDK/Runtime/Modules/ModuleResult.cs` | Success / failure result |
| `Assets/Playcenter/SDK/Runtime/Boot/IBootProgress.cs` | Progress port |
| `Assets/Playcenter/SDK/Runtime/Boot/BootProgress.cs` | Weighted progress |
| `Assets/Playcenter/SDK/Runtime/Boot/BootFailure.cs` | Failure DTO |
| `Assets/Playcenter/SDK/Runtime/Boot/BootFailureCode.cs` | Enum |
| `Assets/Playcenter/SDK/Runtime/Boot/IGameEntry.cs` | Game handoff |
| `Assets/Playcenter/SDK/Runtime/Shell/IShellUi.cs` | Shell show/hide |
| `Assets/Playcenter/SDK/Runtime/Shell/ShellScreenId.cs` | Screen ids |
| `Assets/Playcenter/SDK/Runtime/Shell/IShellTheme.cs` | Theme port |
| `Assets/Playcenter/SDK/Runtime/Shell/ShellTheme.cs` | Default theme + USS paths |
| `Assets/Playcenter/SDK/Runtime/Shell/NullShellUi.cs` | No-op shell for unit tests |
| `Assets/Playcenter/SDK/Runtime/Modules/Default/LoggingModule.cs` | Default module |
| `Assets/Playcenter/SDK/Runtime/Modules/Default/ConnectivityModule.cs` | Default module |
| `Assets/Playcenter/SDK/Runtime/Modules/Default/NtpModule.cs` | Default module |
| `Assets/Playcenter/SDK/Runtime/Modules/Default/RemoteConfigModule.cs` | Default module |
| `Assets/Playcenter/SDK/Runtime/Modules/Default/ForceUpdateModule.cs` | Default module |
| `Assets/Playcenter/SDK/Runtime/Modules/Default/MaintenanceModule.cs` | Default module |
| `Assets/Playcenter/SDK/Runtime/Modules/Default/AuthWarmupModule.cs` | Warm only |
| `Assets/Playcenter/SDK/Runtime/Modules/Default/AnalyticsModule.cs` | Default module |
| `Assets/Playcenter/SDK/Runtime/Modules/Default/ShellReadyModule.cs` | Theme apply |
| `Assets/Playcenter/SDK/Runtime/Modules/DefaultModulePack.cs` | `UseDefaultModules` order+weights |
| `Assets/Playcenter/SDK/Runtime/Shell/ToolkitShellUi.cs` | UITK shell host (Unity) |
| `Assets/Playcenter/SDK/UI/Screens/*.uxml` | Splash/Loading/gates/Settings |
| `Assets/Playcenter/SDK/UI/Styles/DefaultShell.uss` | Base theme + cursor fix |
| `Assets/Playcenter/SDK/UI/Styles/shell_*.uss` | Per-screen styles |
| `Assets/_KitchenClash/Composition/RecipeRageGameEntry.cs` | Game handoff impl |
| `Assets/_KitchenClash/Composition/PlaycenterSdkBootstrap.cs` | Replaces GameBootstrapper start path |
| `Assets/Scripts/Tests/EditMode/Playcenter/SDK/*` | Registry/ModuleHost/Client tests |
| `.github/skills/playcenter-sdk/SKILL.md` | AI skill |
| `.claude/skills/playcenter-sdk/SKILL.md` | Mirror for Claude |

### Modify

| Path | Change |
|------|--------|
| `Assets/Scripts/Tests/RecipeRage.Tests.EditMode.asmdef` | Reference `Playcenter.SDK` |
| `Assets/_KitchenClash/Composition/GameBootstrapper.cs` | Delete or reduce to no-op; SDK bootstrap owns start |
| `Assets/_KitchenClash/Composition/RootLifetimeScope.cs` | Remove BootSequence/BootFlowPort wiring; register SDK bridge |
| `Assets/_KitchenClash/Infrastructure/Flow/BootFlowPort.cs` | Delete with BootSequence |
| `Assets/_KitchenClash/Infrastructure/Flow/Handlers/BootSequence.cs` | **Delete** |
| `Assets/_KitchenClash/Infrastructure/Flow/Handlers/NoConnectionPhase.cs` | Retry via SDK ModuleHost / shell, not BootSequence |
| `Assets/Playcenter/GameFlow/Runtime/Core/IAppFlow.cs` | Deprecate or remove `StartColdBoot` / `NotifyBootComplete` **only if** no remaining callers after cutover; prefer remove after grep clean |
| `Assets/Playcenter/GameFlow/Runtime/Core/AppFlowController.cs` | Stop requiring Boot port for cold start; Home entry from game after ready |
| `Assets/Scripts/Tests/EditMode/Gameplay/BootSequenceConnectivityTests.cs` | **Delete** or rewrite as module tests |
| `Assets/Scripts/Tests/EditMode/Gameplay/AppFlowControllerTests.cs` | Stop depending on StartColdBoot for Home |
| Game shell screens (Splash/Loading/NoInternet/Maintenance/Settings) | Delete after SDK pack owns them |
| `wiki/Technical.md`, `wiki/LLM-Rules.md`, `wiki/log.md` | SDK laws after cutover |

### Do not touch

- Cooking / match NetBehaviours / chef IP
- Wallet ledger ownership laws
- MenuSessionRegistrations CreateSession-only install pattern (keep)

---

### Task 1: ServiceRegistry (SDK DI core)

**Files:**
- Create: `Assets/Playcenter/SDK/Runtime/Playcenter.SDK.asmdef`
- Create: `Assets/Playcenter/SDK/Runtime/DI/IServiceRegistry.cs`
- Create: `Assets/Playcenter/SDK/Runtime/DI/IPlaycenterServices.cs`
- Create: `Assets/Playcenter/SDK/Runtime/DI/ServiceRegistry.cs`
- Create: `Assets/Scripts/Tests/EditMode/Playcenter/SDK/ServiceRegistryTests.cs`
- Modify: `Assets/Scripts/Tests/RecipeRage.Tests.EditMode.asmdef` — add `"Playcenter.SDK"`

**Interfaces:**
- Consumes: nothing
- Produces:
  - `IServiceRegistry`: `void AddSingleton<TService>(TService instance)`, `void AddSingleton<TService, TImpl>() where TImpl : TService, new()`, `void AddSingleton<TService>(Func<IPlaycenterServices, TService> factory)`
  - `IPlaycenterServices`: `T Get<T>()`, `bool TryGet<T>(out T service)`, `bool IsRegistered<T>()`
  - `ServiceRegistry : IServiceRegistry, IPlaycenterServices` with `IPlaycenterServices Build()` that freezes registrations (second `Add*` throws)

**asmdef:**

```json
{
    "name": "Playcenter.SDK",
    "rootNamespace": "Playcenter.SDK",
    "references": [
        "Playcenter.Shell",
        "Playcenter.Services"
    ],
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

Note: Shell UI Toolkit host is added in Task 4 with a **second** Unity-thin assembly `Playcenter.SDK.Unity` **or** by flipping `noEngineReferences` only when shell lands. Prefer split:

- `Playcenter.SDK` — pure (`noEngineReferences: true`) — Tasks 1–3
- `Playcenter.SDK.Unity` — UITK shell — Task 4+

If split, create `Assets/Playcenter/SDK.Unity/Runtime/Playcenter.SDK.Unity.asmdef` in Task 4. Task 1 uses pure SDK only.

- [ ] **Step 1: Write the failing test**

Create `Assets/Scripts/Tests/EditMode/Playcenter/SDK/ServiceRegistryTests.cs`:

```csharp
using NUnit.Framework;
using Playcenter.SDK;

namespace RecipeRage.Tests.EditMode.Playcenter.SDK
{
    public sealed class ServiceRegistryTests
    {
        public interface IFoo { }
        public sealed class Foo : IFoo { }
        public sealed class Foo2 : IFoo { }

        [Test]
        public void Get_AfterAddSingletonInstance_ReturnsSameInstance()
        {
            var reg = new ServiceRegistry();
            var foo = new Foo();
            reg.AddSingleton<IFoo>(foo);
            IPlaycenterServices services = reg.Build();

            Assert.AreSame(foo, services.Get<IFoo>());
        }

        [Test]
        public void Get_AfterAddSingletonType_CreatesSingleInstance()
        {
            var reg = new ServiceRegistry();
            reg.AddSingleton<IFoo, Foo>();
            IPlaycenterServices services = reg.Build();

            Assert.AreSame(services.Get<IFoo>(), services.Get<IFoo>());
            Assert.IsInstanceOf<Foo>(services.Get<IFoo>());
        }

        [Test]
        public void Get_AfterFactory_ReceivesServices()
        {
            var reg = new ServiceRegistry();
            reg.AddSingleton<IFoo, Foo>();
            reg.AddSingleton<string>(sp => "id:" + sp.Get<IFoo>().GetType().Name);
            IPlaycenterServices services = reg.Build();

            Assert.AreEqual("id:Foo", services.Get<string>());
        }

        [Test]
        public void Get_WhenMissing_ThrowsInvalidOperationException()
        {
            IPlaycenterServices services = new ServiceRegistry().Build();
            Assert.Throws<System.InvalidOperationException>(() => services.Get<IFoo>());
        }

        [Test]
        public void TryGet_WhenMissing_ReturnsFalse()
        {
            IPlaycenterServices services = new ServiceRegistry().Build();
            bool ok = services.TryGet<IFoo>(out IFoo _);
            Assert.IsFalse(ok);
        }

        [Test]
        public void AddSingleton_AfterBuild_ThrowsInvalidOperationException()
        {
            var reg = new ServiceRegistry();
            reg.Build();
            Assert.Throws<System.InvalidOperationException>(() => reg.AddSingleton<IFoo, Foo>());
        }

        [Test]
        public void AddSingleton_DuplicateService_ThrowsInvalidOperationException()
        {
            var reg = new ServiceRegistry();
            reg.AddSingleton<IFoo, Foo>();
            Assert.Throws<System.InvalidOperationException>(() => reg.AddSingleton<IFoo, Foo2>());
        }
    }
}
```

- [ ] **Step 2: Update test asmdef + run test (expect fail)**

Add `"Playcenter.SDK"` to `RecipeRage.Tests.EditMode.asmdef` references.

Run:

```bash
dotnet test RecipeRage.Tests.EditMode.csproj --filter="FullyQualifiedName~ServiceRegistryTests" -nologo
```

Expected: FAIL (type/asm missing or compile error). If Unity has not generated `Playcenter.SDK.csproj` yet, create a minimal hand-aligned compile by opening Unity once, or `dotnet build` after Editor refresh.

- [ ] **Step 3: Implement ServiceRegistry**

`IServiceRegistry.cs`:

```csharp
using System;

namespace Playcenter.SDK
{
    public interface IServiceRegistry
    {
        void AddSingleton<TService>(TService instance) where TService : class;
        void AddSingleton<TService, TImpl>() where TService : class where TImpl : class, TService, new();
        void AddSingleton<TService>(Func<IPlaycenterServices, TService> factory) where TService : class;
        IPlaycenterServices Build();
    }
}
```

`IPlaycenterServices.cs`:

```csharp
namespace Playcenter.SDK
{
    public interface IPlaycenterServices
    {
        T Get<T>() where T : class;
        bool TryGet<T>(out T service) where T : class;
        bool IsRegistered<T>() where T : class;
    }
}
```

`ServiceRegistry.cs` — dictionary of `Type → registration` (instance | lazy factory). `Build()` sets `_built = true` and returns `this`. Factories invoke with `this` as `IPlaycenterServices`. Lazy singletons lock on first resolve. Missing `Get` throws `InvalidOperationException` with type name. Duplicate registration throws.

- [ ] **Step 4: Run tests (expect pass)**

```bash
dotnet test RecipeRage.Tests.EditMode.csproj --filter="FullyQualifiedName~ServiceRegistryTests" -nologo
```

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add Assets/Playcenter/SDK Assets/Scripts/Tests/EditMode/Playcenter/SDK/ServiceRegistryTests.cs Assets/Scripts/Tests/RecipeRage.Tests.EditMode.asmdef
git commit -m "$(cat <<'EOF'
feat(sdk): add Playcenter.SDK ServiceRegistry DI

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 2: ModuleHost + BootProgress + BootFailure

**Files:**
- Create: `Assets/Playcenter/SDK/Runtime/Boot/BootFailureCode.cs`
- Create: `Assets/Playcenter/SDK/Runtime/Boot/BootFailure.cs`
- Create: `Assets/Playcenter/SDK/Runtime/Boot/IBootProgress.cs`
- Create: `Assets/Playcenter/SDK/Runtime/Boot/BootProgress.cs`
- Create: `Assets/Playcenter/SDK/Runtime/Modules/IPlaycenterModule.cs`
- Create: `Assets/Playcenter/SDK/Runtime/Modules/ModuleContext.cs`
- Create: `Assets/Playcenter/SDK/Runtime/Modules/ModuleResult.cs`
- Create: `Assets/Playcenter/SDK/Runtime/Modules/ModuleHost.cs`
- Create: `Assets/Scripts/Tests/EditMode/Playcenter/SDK/ModuleHostTests.cs`
- Create: `Assets/Scripts/Tests/EditMode/Playcenter/SDK/BootProgressTests.cs`

**Interfaces:**
- Consumes: `IPlaycenterServices`
- Produces:
  - `enum BootFailureCode { Offline, ForceUpdate, Maintenance, RemoteConfig, Cancelled, Unknown }`
  - `sealed class BootFailure { BootFailureCode Code; string Message; string FailedModuleId; }`
  - `interface IBootProgress { float Overall01 { get; } void Report(string moduleId, float local01); event Action<float, string> Changed; }`
  - `interface IPlaycenterModule { string Id { get; } float Weight { get; } Task<ModuleResult> InitializeAsync(ModuleContext context, CancellationToken ct); }`
  - `sealed class ModuleResult { bool Success; BootFailure Failure; static ModuleResult Ok(); static ModuleResult Fail(BootFailureCode code, string message); }`
  - `sealed class ModuleContext { IPlaycenterServices Services; IBootProgress Progress; }`
  - `sealed class ModuleHost` with:
    - `Task<BootFailure> RunAsync(IReadOnlyList<IPlaycenterModule> modules, ModuleContext context, CancellationToken ct)` — returns `null` on full success, else failure
    - `Task<BootFailure> RetryFromAsync(string moduleId, ...)` — re-runs from that module index through end (prior modules assumed OK)

**Weight rule:** `Overall01 = sum(completedWeights) / totalWeight + (currentWeight * local01) / totalWeight`. Zero total weight → Overall01 = 1 when done.

- [ ] **Step 1: Write failing BootProgress tests**

```csharp
using NUnit.Framework;
using Playcenter.SDK;

namespace RecipeRage.Tests.EditMode.Playcenter.SDK
{
    public sealed class BootProgressTests
    {
        [Test]
        public void Report_TwoEqualModules_HalfwayAfterFirstComplete()
        {
            var p = new BootProgress(new[] { ("a", 1f), ("b", 1f) });
            p.Report("a", 1f);
            Assert.AreEqual(0.5f, p.Overall01, 0.001f);
        }

        [Test]
        public void Report_PartialLocal_IncludesFraction()
        {
            var p = new BootProgress(new[] { ("a", 1f), ("b", 1f) });
            p.Report("a", 0.5f);
            Assert.AreEqual(0.25f, p.Overall01, 0.001f);
        }
    }
}
```

- [ ] **Step 2: Write failing ModuleHost tests**

```csharp
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Playcenter.SDK;

namespace RecipeRage.Tests.EditMode.Playcenter.SDK
{
    public sealed class ModuleHostTests
    {
        private sealed class FakeModule : IPlaycenterModule
        {
            public string Id { get; }
            public float Weight { get; }
            public int Runs;
            public ModuleResult ResultToReturn = ModuleResult.Ok();
            public FakeModule(string id, float weight = 1f) { Id = id; Weight = weight; }
            public Task<ModuleResult> InitializeAsync(ModuleContext context, CancellationToken ct)
            {
                Runs++;
                context.Progress.Report(Id, 1f);
                return Task.FromResult(ResultToReturn);
            }
        }

        [Test]
        public async Task RunAsync_AllOk_ReturnsNullAndRunsInOrder()
        {
            var order = new List<string>();
            var m1 = new RecordingModule("a", order);
            var m2 = new RecordingModule("b", order);
            var host = new ModuleHost();
            var reg = new ServiceRegistry().Build();
            var progress = new BootProgress(new[] { ("a", 1f), ("b", 1f) });
            var ctx = new ModuleContext(reg, progress);

            BootFailure fail = await host.RunAsync(new IPlaycenterModule[] { m1, m2 }, ctx, CancellationToken.None);

            Assert.IsNull(fail);
            CollectionAssert.AreEqual(new[] { "a", "b" }, order);
            Assert.AreEqual(1f, progress.Overall01, 0.001f);
        }

        [Test]
        public async Task RunAsync_WhenModuleFails_StopsAndReturnsFailure()
        {
            var m1 = new FakeModule("a");
            var m2 = new FakeModule("b")
            {
                ResultToReturn = ModuleResult.Fail(BootFailureCode.Offline, "down")
            };
            var m3 = new FakeModule("c");
            var host = new ModuleHost();
            var progress = new BootProgress(new[] { ("a", 1f), ("b", 1f), ("c", 1f) });
            var ctx = new ModuleContext(new ServiceRegistry().Build(), progress);

            BootFailure fail = await host.RunAsync(new IPlaycenterModule[] { m1, m2, m3 }, ctx, CancellationToken.None);

            Assert.IsNotNull(fail);
            Assert.AreEqual(BootFailureCode.Offline, fail.Code);
            Assert.AreEqual("b", fail.FailedModuleId);
            Assert.AreEqual(1, m1.Runs);
            Assert.AreEqual(1, m2.Runs);
            Assert.AreEqual(0, m3.Runs);
        }

        [Test]
        public async Task RunAsync_WhenCancelled_ReturnsCancelled()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var host = new ModuleHost();
            var m1 = new FakeModule("a");
            var progress = new BootProgress(new[] { ("a", 1f) });
            var ctx = new ModuleContext(new ServiceRegistry().Build(), progress);

            BootFailure fail = await host.RunAsync(new IPlaycenterModule[] { m1 }, ctx, cts.Token);

            Assert.IsNotNull(fail);
            Assert.AreEqual(BootFailureCode.Cancelled, fail.Code);
            Assert.AreEqual(0, m1.Runs);
        }

        [Test]
        public async Task RetryFromAsync_ReRunsFromFailedModule()
        {
            var m1 = new FakeModule("a");
            var m2 = new FakeModule("b")
            {
                ResultToReturn = ModuleResult.Fail(BootFailureCode.Offline, "down")
            };
            var host = new ModuleHost();
            var modules = new IPlaycenterModule[] { m1, m2 };
            var progress = new BootProgress(new[] { ("a", 1f), ("b", 1f) });
            var ctx = new ModuleContext(new ServiceRegistry().Build(), progress);
            await host.RunAsync(modules, ctx, CancellationToken.None);

            m2.ResultToReturn = ModuleResult.Ok();
            progress = new BootProgress(new[] { ("a", 1f), ("b", 1f) });
            ctx = new ModuleContext(new ServiceRegistry().Build(), progress);
            // After first success of a, retry from b should not re-run a if host tracks last run list:
            BootFailure fail = await host.RetryFromAsync("b", modules, ctx, CancellationToken.None);

            Assert.IsNull(fail);
            Assert.AreEqual(1, m1.Runs); // not re-run
            Assert.AreEqual(2, m2.Runs);
        }

        private sealed class RecordingModule : IPlaycenterModule
        {
            private readonly List<string> _order;
            public string Id { get; }
            public float Weight => 1f;
            public RecordingModule(string id, List<string> order) { Id = id; _order = order; }
            public Task<ModuleResult> InitializeAsync(ModuleContext context, CancellationToken ct)
            {
                _order.Add(Id);
                context.Progress.Report(Id, 1f);
                return Task.FromResult(ModuleResult.Ok());
            }
        }
    }
}
```

Fix the draft test’s leftover `FakeModule a = ...` noise when implementing — keep only clean tests above (RecordingModule + FakeModule).

- [ ] **Step 3: Run tests (expect fail)**

```bash
dotnet test RecipeRage.Tests.EditMode.csproj --filter="FullyQualifiedName~ModuleHostTests|FullyQualifiedName~BootProgressTests" -nologo
```

- [ ] **Step 4: Implement types**

Implement exactly to satisfy tests. `ModuleHost.RunAsync`:

1. If `ct.IsCancellationRequested` → return `new BootFailure(BootFailureCode.Cancelled, "cancelled", null)`
2. For each module: `ct.ThrowIfCancellationRequested` (catch → Cancelled), await `InitializeAsync`, on `!Success` return failure with `FailedModuleId = module.Id`
3. Uncaught exception → `Unknown` with message
4. Success → return `null`

`RetryFromAsync`: find index of `moduleId`; if missing throw; run slice `[index..]` only.

`ModuleResult.Fail` sets `Success = false` and populates `Failure` (module id filled by host).

- [ ] **Step 5: Run tests (expect pass)**

- [ ] **Step 6: Commit**

```bash
git add Assets/Playcenter/SDK Assets/Scripts/Tests/EditMode/Playcenter/SDK
git commit -m "$(cat <<'EOF'
feat(sdk): add ModuleHost, BootProgress, and BootFailure

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 3: PlaycenterClient + IGameEntry handoff

**Files:**
- Create: `Assets/Playcenter/SDK/Runtime/Boot/IGameEntry.cs`
- Create: `Assets/Playcenter/SDK/Runtime/ClientOptions.cs`
- Create: `Assets/Playcenter/SDK/Runtime/PlaycenterClient.cs`
- Create: `Assets/Playcenter/SDK/Runtime/Shell/IShellUi.cs`
- Create: `Assets/Playcenter/SDK/Runtime/Shell/ShellScreenId.cs`
- Create: `Assets/Playcenter/SDK/Runtime/Shell/IShellTheme.cs`
- Create: `Assets/Playcenter/SDK/Runtime/Shell/NullShellUi.cs`
- Create: `Assets/Playcenter/SDK/Runtime/Shell/ShellTheme.cs`
- Create: `Assets/Scripts/Tests/EditMode/Playcenter/SDK/PlaycenterClientTests.cs`

**Interfaces:**
- Consumes: Task 1–2 types
- Produces:
  - `interface IGameEntry { Task OnPlaycenterReadyAsync(PlaycenterClient client, CancellationToken ct); Task OnPlaycenterFailedAsync(BootFailure failure, CancellationToken ct); }`
  - `enum ShellScreenId { Splash, Loading, Settings, NoConnection, ForceUpdate, Maintenance }`
  - `interface IShellUi { void Show(ShellScreenId id); void Hide(ShellScreenId id); void HideAll(); void SetProgress(float overall01, string status); void SetTheme(IShellTheme theme); }`
  - `interface IShellTheme { string OverrideUssResourcesPath { get; } }`
  - `sealed class ClientOptions` built via `PlaycenterClient.Create(Action<ClientOptions> configure)`
  - Options API:
    - `IServiceRegistry Services` (mutable registry before build)
    - `void AddModule(IPlaycenterModule module)`
    - `void SetGameEntry(IGameEntry entry)`
    - `void UseShell(IShellUi shell)` — default `NullShellUi`
    - `void UseTheme(IShellTheme theme)`
    - `void UseDefaultModules()` — empty until Task 5; for Task 3 tests pass explicit modules
  - `sealed class PlaycenterClient`:
    - `IPlaycenterServices Services { get; }`
    - `IShellUi Shell { get; }`
    - `static PlaycenterClient Create(Action<ClientOptions> configure)`
    - `Task RunAsync(CancellationToken ct)`

**RunAsync algorithm:**

1. `options` build registry → `Services`
2. Build `BootProgress` from module weights
3. `Shell.SetTheme` if theme set
4. `Shell.Show(Splash)` then `Shell.Show(Loading)` (Splash may be no-op hide after 0ms in NullShell)
5. Subscribe progress → `Shell.SetProgress`
6. `failure = await ModuleHost.RunAsync(...)`
7. If failure == null: `Shell.HideAll()`; `await entry.OnPlaycenterReadyAsync(this, ct)`
8. Else: map code → show gate screen (`Offline`→NoConnection, `ForceUpdate`→ForceUpdate, `Maintenance`→Maintenance, else NoConnection); `await entry.OnPlaycenterFailedAsync(failure, ct)`

- [ ] **Step 1: Write failing client tests**

```csharp
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Playcenter.SDK;

namespace RecipeRage.Tests.EditMode.Playcenter.SDK
{
    public sealed class PlaycenterClientTests
    {
        private sealed class OkModule : IPlaycenterModule
        {
            public string Id => "ok";
            public float Weight => 1f;
            public Task<ModuleResult> InitializeAsync(ModuleContext context, CancellationToken ct)
            {
                context.Progress.Report(Id, 1f);
                return Task.FromResult(ModuleResult.Ok());
            }
        }

        private sealed class FailModule : IPlaycenterModule
        {
            public string Id => "bad";
            public float Weight => 1f;
            public Task<ModuleResult> InitializeAsync(ModuleContext context, CancellationToken ct)
                => Task.FromResult(ModuleResult.Fail(BootFailureCode.ForceUpdate, "update"));
        }

        private sealed class SpyEntry : IGameEntry
        {
            public int ReadyCount;
            public int FailCount;
            public BootFailure LastFailure;
            public PlaycenterClient ReadyClient;
            public Task OnPlaycenterReadyAsync(PlaycenterClient client, CancellationToken ct)
            {
                ReadyCount++;
                ReadyClient = client;
                return Task.CompletedTask;
            }
            public Task OnPlaycenterFailedAsync(BootFailure failure, CancellationToken ct)
            {
                FailCount++;
                LastFailure = failure;
                return Task.CompletedTask;
            }
        }

        [Test]
        public async Task RunAsync_WhenModulesSucceed_CallsOnPlaycenterReadyOnce()
        {
            var entry = new SpyEntry();
            var client = PlaycenterClient.Create(o =>
            {
                o.SetGameEntry(entry);
                o.AddModule(new OkModule());
                o.UseShell(new NullShellUi());
            });

            await client.RunAsync(CancellationToken.None);

            Assert.AreEqual(1, entry.ReadyCount);
            Assert.AreEqual(0, entry.FailCount);
            Assert.AreSame(client, entry.ReadyClient);
        }

        [Test]
        public async Task RunAsync_WhenModuleFails_CallsOnPlaycenterFailed_NotReady()
        {
            var entry = new SpyEntry();
            var client = PlaycenterClient.Create(o =>
            {
                o.SetGameEntry(entry);
                o.AddModule(new FailModule());
                o.UseShell(new NullShellUi());
            });

            await client.RunAsync(CancellationToken.None);

            Assert.AreEqual(0, entry.ReadyCount);
            Assert.AreEqual(1, entry.FailCount);
            Assert.AreEqual(BootFailureCode.ForceUpdate, entry.LastFailure.Code);
        }

        [Test]
        public void Create_WithoutGameEntry_ThrowsOnRun()
        {
            var client = PlaycenterClient.Create(o => o.AddModule(new OkModule()));
            Assert.ThrowsAsync<System.InvalidOperationException>(async () => await client.RunAsync(CancellationToken.None));
        }
    }
}
```

- [ ] **Step 2: Run (expect fail) → implement → run (expect pass)**

`NullShellUi`: all methods no-op.  
`PlaycenterClient.Create`: new `ClientOptions`, invoke configure, construct client holding options.  
On `RunAsync`, if entry null throw.

- [ ] **Step 3: Commit**

```bash
git commit -m "$(cat <<'EOF'
feat(sdk): add PlaycenterClient and IGameEntry handoff

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 4: Unity shell pack (Splash + Loading) + SDK.Unity asmdef

**Files:**
- Create: `Assets/Playcenter/SDK.Unity/Runtime/Playcenter.SDK.Unity.asmdef`
- Create: `Assets/Playcenter/SDK.Unity/Runtime/ToolkitShellUi.cs`
- Create: `Assets/Playcenter/SDK.Unity/Runtime/ShellScreenController.cs` (minimal)
- Create: `Assets/Playcenter/SDK/UI/Screens/LoadingShell.uxml`
- Create: `Assets/Playcenter/SDK/UI/Screens/SplashShell.uxml`
- Create: `Assets/Playcenter/SDK/UI/Styles/DefaultShell.uss`
- Create: `Assets/Playcenter/SDK/UI/Styles/shell_loading.uss`
- Create: `Assets/Playcenter/SDK/UI/Styles/shell_splash.uss`
- Test: manual / optional EditMode for status mapping only if pure helper extracted

**asmdef `Playcenter.SDK.Unity`:**

```json
{
    "name": "Playcenter.SDK.Unity",
    "rootNamespace": "Playcenter.SDK.Unity",
    "references": [
        "Playcenter.SDK",
        "Playcenter.Shell",
        "UniTask",
        "Unity.InputSystem"
    ],
    "noEngineReferences": false,
    "autoReferenced": true
}
```

**DefaultShell.uss** must include:

```css
* {
    cursor: arrow;
}
```

**LoadingShell.uxml** elements (name attrs stable):

- `status-label` (Label)
- `progress-bar` (ProgressBar or VisualElement fill)
- root class `pc-loading`

**ToolkitShellUi:**

- Constructs or binds a `UIDocument` (create DDOL GameObject if none provided)
- Loads UXML from `Resources` **or** serialized references injected by game bootstrap
- Prefer **Resources** paths under `Assets/Playcenter/SDK/Resources/UI/Shell/` so load is `Resources.Load<VisualTreeAsset>("UI/Shell/LoadingShell")` — move UXML there in this task:

Final resource paths:

- `Assets/Playcenter/SDK.Unity/Resources/UI/Shell/LoadingShell.uxml`
- `Assets/Playcenter/SDK.Unity/Resources/UI/Shell/SplashShell.uxml`
- `Assets/Playcenter/SDK.Unity/Resources/UI/Shell/DefaultShell.uss`

`SetProgress(overall01, status)` updates label + bar value `overall01 * 100`.

`Show(Loading)` clears and instances Loading UXML; applies DefaultShell.uss + optional theme override USS from `IShellTheme.OverrideUssResourcesPath`.

- [ ] **Step 1: Add UXML/USS + ToolkitShellUi**

Keep controllers thin — no game namespaces.

- [ ] **Step 2: Wire ClientOptions helper**

In pure SDK, keep `IShellUi` only. In Unity bootstrap (Task 6), game does:

```csharp
o.UseShell(new Playcenter.SDK.Unity.ToolkitShellUi());
o.UseTheme(new ShellTheme("UI/Themes/DesignSystem"));
```

Add `ShellTheme` ctor `(string overrideUssResourcesPath)`.

- [ ] **Step 3: Smoke compile**

```bash
# After Unity refreshes csproj:
dotnet build Playcenter.SDK.csproj -nologo
dotnet build Playcenter.SDK.Unity.csproj -nologo
```

Expected: build succeeded (0 errors)

- [ ] **Step 4: Commit**

```bash
git commit -m "$(cat <<'EOF'
feat(sdk): add ToolkitShellUi splash and loading pack

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 5: Default module pack (port BootSequence logic)

**Files:**
- Create: default modules under `Assets/Playcenter/SDK/Runtime/Modules/Default/`
- Create: `Assets/Playcenter/SDK/Runtime/Modules/DefaultModulePack.cs`
- Create: `Assets/Playcenter/SDK/Runtime/Modules/Default/ForceUpdateEvaluator.cs` (pure version compare)
- Create: `Assets/Scripts/Tests/EditMode/Playcenter/SDK/DefaultModulesTests.cs`
- Create: `Assets/Scripts/Tests/EditMode/Playcenter/SDK/ForceUpdateEvaluatorTests.cs`
- Reference ports: `IConnectivityService`, `INTPTimeService`, `IRemoteConfigService`, `IMaintenanceService`, `IAuthService`, `IAnalyticsService` from Playcenter.Services/Shell

**Module behaviors (must match spec weights/order):**

| Id | Weight | Behavior |
|----|--------|----------|
| `logging` | 0.05 | Ok if logger available; always Ok |
| `connectivity` | 0.15 | Fail Offline if `!IsOnline` |
| `ntp` | 0.10 | Best-effort `SyncTime` with 5s timeout; always Ok |
| `remote_config` | 0.15 | `Initialize` + `RefreshConfig`; fail RemoteConfig if both throw/false hard fail — treat false Initialize as Fail |
| `force_update` | 0.10 | Use `ForceUpdateEvaluator` + RC configs registered as needed; Fail ForceUpdate if required |
| `maintenance` | 0.10 | `CheckMaintenanceStatusAsync` true → Fail Maintenance |
| `auth_warmup` | 0.15 | **Do not** login; Ok always (optional future platform warm). Does **not** fail if `ProductUserId` empty |
| `analytics` | 0.10 | If `IAnalyticsService` registered, no-op init Ok; else Ok |
| `shell_ready` | 0.10 | Ok (theme already applied in client) |

`ClientOptions.UseDefaultModules()` adds these nine in order.

**ForceUpdateEvaluator** (pure):

```csharp
public static class ForceUpdateEvaluator
{
    // returns <0 if current < minimum
    public static int CompareVersions(string current, string minimum) { /* numeric semver-ish split on '.' */ }
    public static bool IsUpdateRequired(string current, string minimum)
        => !string.IsNullOrEmpty(minimum) && CompareVersions(current, minimum) < 0;
}
```

Port logic from `ForceUpdateChecker.CompareVersions` (copy algorithm, no UnityEngine in pure SDK). Current version string is passed in from Unity bootstrap via registry:

```csharp
// registration in game bootstrap before RunAsync:
services.AddSingleton<IAppVersion>(new AppVersion(Application.version));
```

Define minimal:

```csharp
namespace Playcenter.SDK
{
    public interface IAppVersion { string Current { get; } }
}
```

Force update module reads `IAppVersion` + `IRemoteConfigService`. For RC typed configs that today live in KitchenClash (`ForceUpdateConfig`), **v1 approach:** module calls optional `IForceUpdatePolicy` port:

```csharp
public interface IForceUpdatePolicy
{
    Task<ForceUpdateDecision> EvaluateAsync(CancellationToken ct);
}
public readonly struct ForceUpdateDecision
{
    public bool Required { get; }
    public string Message { get; }
    public string StoreUrl { get; }
}
```

Game registers `KitchenClashForceUpdatePolicy` wrapping existing checker logic in Task 6. Default module: if policy missing → Ok (skip). If policy says required → Fail ForceUpdate.

Same pattern optional for maintenance if needed — prefer direct `IMaintenanceService` already in Playcenter.Services.

- [ ] **Step 1: Tests for ForceUpdateEvaluator + ConnectivityModule**

```csharp
[Test]
public void IsUpdateRequired_WhenCurrentLower_ReturnsTrue()
{
    Assert.IsTrue(ForceUpdateEvaluator.IsUpdateRequired("1.0.0", "1.1.0"));
}

[Test]
public async Task ConnectivityModule_WhenOffline_FailsOffline()
{
    var reg = new ServiceRegistry();
    reg.AddSingleton<IConnectivityService>(new FakeConnectivity(false));
    var services = reg.Build();
    var mod = new ConnectivityModule();
    var ctx = new ModuleContext(services, new BootProgress(new[] { (mod.Id, mod.Weight) }));
    ModuleResult result = await mod.InitializeAsync(ctx, CancellationToken.None);
    Assert.IsFalse(result.Success);
    Assert.AreEqual(BootFailureCode.Offline, result.Failure.Code);
}
```

`FakeConnectivity` in test file implementing `IConnectivityService` with fixed `IsOnline`.

- [ ] **Step 2: Implement modules + UseDefaultModules**

- [ ] **Step 3: Tests pass + commit**

```bash
git commit -m "$(cat <<'EOF'
feat(sdk): add default boot module pack

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 6: RecipeRage cutover — delete BootSequence (HARD DELETE GATE)

**Files:**
- Create: `Assets/_KitchenClash/Composition/RecipeRageGameEntry.cs`
- Create: `Assets/_KitchenClash/Composition/PlaycenterSdkBootstrap.cs`
- Create: `Assets/_KitchenClash/Infrastructure/Boot/KitchenClashForceUpdatePolicy.cs`
- Modify: `Assets/_KitchenClash/Composition/RootLifetimeScope.cs` — remove `BootSequence` construction / `BootFlowPort`; register `PlaycenterSdkBootstrap` as `IStartable` instead of (or replacing) `GameBootstrapper` cold boot
- Modify: `Assets/_KitchenClash/Composition/GameBootstrapper.cs` — **delete file** OR replace body to throw if called
- Delete: `Assets/_KitchenClash/Infrastructure/Flow/Handlers/BootSequence.cs`
- Delete: `Assets/_KitchenClash/Infrastructure/Flow/BootFlowPort.cs`
- Modify: `Assets/_KitchenClash/Infrastructure/Flow/Handlers/NoConnectionPhase.cs` — remove `BootSequence` dependency; Retry calls `IPlaycenterBootRetry` or re-enter SDK shell retry API
- Modify: `Assets/Playcenter/GameFlow/Runtime/Core/AppFlowController.cs` — `StartColdBoot` becomes obsolete: either no-op documenting “use PlaycenterClient” **or** remove method and fix all call sites (prefer **remove** after grep)
- Modify: `Assets/Playcenter/GameFlow/Runtime/Core/IAppFlow.cs` — remove `StartColdBoot` and `NotifyBootComplete` if unused
- Delete: `Assets/Scripts/Tests/EditMode/Gameplay/BootSequenceConnectivityTests.cs`
- Modify: `Assets/Scripts/Tests/EditMode/Gameplay/AppFlowControllerTests.cs` — bootstrap Home without StartColdBoot
- Modify: `Assets/Scripts/Tests/EditMode/Gameplay/Fakes/FakeAppFlow.cs` — remove deleted members
- Modify: KitchenClash Infrastructure asmdef / Composition references to include `Playcenter.SDK` + `Playcenter.SDK.Unity`

**RecipeRageGameEntry responsibilities:**

```csharp
public sealed class RecipeRageGameEntry : IGameEntry
{
    // Resolve game services via static/composition root assigned before RunAsync,
    // or pass delegates in ctor from RootLifetimeScope.
    public async Task OnPlaycenterReadyAsync(PlaycenterClient client, CancellationToken ct)
    {
        // 1. Bridge: expose client.Services to game (SessionManager / facades)
        // 2. If auth ProductUserId empty → show Login via IUIService / IAppFlow.EnterSidePhase(Login)
        // 3. Else SessionLoader.LoadAsync + IAppFlow navigate Home
        //    Use existing LoginPhase success path patterns; do NOT call deleted BootSequence
    }

    public Task OnPlaycenterFailedAsync(BootFailure failure, CancellationToken ct)
    {
        // Analytics log; shell already showing gate
        return Task.CompletedTask;
    }
}
```

**PlaycenterSdkBootstrap (`IStartable`):**

```csharp
public sealed class PlaycenterSdkBootstrap : IStartable
{
    public void Start()
    {
        Run().Forget(); // UniTask extension in Infrastructure
    }

    private async UniTask Run()
    {
        var client = PlaycenterClient.Create(o =>
        {
            o.UseDefaultModules();
            o.UseShell(new ToolkitShellUi());
            o.UseTheme(new ShellTheme("UI/Themes/DesignSystem"));
            o.SetGameEntry(entry);
            // register game-provided port instances into o.Services:
            // connectivity, ntp, remoteConfig, maintenance, auth, analytics, forceUpdatePolicy, appVersion
        });
        await client.RunAsync(cts.Token).AsUniTask();
    }
}
```

**Critical:** Instances registered into SDK registry must be the **same** objects game VContainer uses post-ready (pass live refs from RootLifetimeScope ctor fields into bootstrap). Do not `new` a second EOS auth.

**NoConnection retry:** expose on client:

```csharp
// Add in Task 3/6 if missing:
public Task<BootFailure> RetryBootAsync(CancellationToken ct);
```

Stores last module list + options; `RetryFromAsync("connectivity")` or full `RunAsync` modules again. `NoConnectionPhase` calls this instead of `BootSequence.Start()`.

- [ ] **Step 1: Implement GameEntry + Bootstrap; wire RootLifetimeScope**

- [ ] **Step 2: Delete BootSequence + BootFlowPort + boot tests; fix compile breaks**

Grep gate:

```bash
rg -n "BootSequence|BootFlowPort|StartColdBoot" Assets --glob '*.cs'
```

Expected: **no** matches in production code (tests updated).

- [ ] **Step 3: Fix AppFlow tests — enter Home via test helper**

```csharp
// If StartColdBoot removed, tests call a test-only transition or public method
// Prefer: flow.ReturnHome() after constructing controller with null splash/boot ports
```

Read `AppFlowController` and adjust so post-ready navigation works with `boot: null` and game calling into Home after entry.

- [ ] **Step 4: Run EditMode tests**

```bash
dotnet test RecipeRage.Tests.EditMode.csproj -nologo
```

Expected: PASS (or only pre-existing failures unrelated — fix any caused by this task)

- [ ] **Step 5: Commit**

```bash
git commit -m "$(cat <<'EOF'
feat(sdk): cut over RecipeRage boot to PlaycenterClient

Delete BootSequence and StartColdBoot init path.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 7: Gate screens + Settings in SDK; delete game duplicates

**Files:**
- Create UXML/USS under `Assets/Playcenter/SDK.Unity/Resources/UI/Shell/`:
  - `NoConnectionShell.uxml`
  - `ForceUpdateShell.uxml`
  - `MaintenanceShell.uxml`
  - `SettingsShell.uxml`
  - matching `shell_*.uss`
- Extend `ToolkitShellUi` for these ids + button callbacks:
  - Retry → `PlaycenterClient.RetryBootAsync`
  - Quit → `Application.Quit` (Editor: stop play mode)
  - Update → `Application.OpenURL(storeUrl)` from last `BootFailure` or policy
- Settings: bind audio sliders to `ISettingsService` if registered; show version via `IAppVersion`
- Delete game duplicates after SDK works:
  - `Assets/_KitchenClash/Presentation/Screens/SplashScreen.cs`
  - `LoadingScreen.cs`
  - `SettingsScreen.cs` (only if fully replaced — if game settings has extra sections, keep game screen but prefer SDK shell for v1 per spec; **spec says SDK Settings** — delete game SettingsScreen and route Home gear to `client.Shell.Show(Settings)`)
  - `MaintenanceScreen.cs` + NoInternet screen/popup used only for boot
  - Matching UXML under `Assets/_KitchenClash/UI/Screens/` for those screens
- Update phases (`MaintenancePhase`, `NoConnectionPhase`, force update UI) to drive `IShellUi` or thin wrappers

- [ ] **Step 1: Implement shell screens + wire buttons**

- [ ] **Step 2: Point game chrome at SDK Settings**

- [ ] **Step 3: Delete duplicates; fix refs**

```bash
rg -n "SplashScreen|LoadingScreen|SettingsScreen|MaintenanceScreen|NoInternetScreen" Assets/_KitchenClash --glob '*.cs'
```

- [ ] **Step 4: Commit**

```bash
git commit -m "$(cat <<'EOF'
feat(sdk): move shell gates and settings into SDK UI pack

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 8: VContainer grep gate + game bridge cleanup

**Files:**
- Modify any Playcenter file that still implies DI host leakage
- Modify `Assets/Playcenter/UI/Runtime/IUIService.cs` comments if needed (keep factory pattern; no VContainer type refs)
- Modify `Assets/Playcenter/UI.Toolkit/Runtime/IScreenInstanceFactory.cs` — rename doc from “VContainer-backed” to “game DI-backed”
- Ensure `Assets/Playcenter/**` has **zero** `using VContainer`

- [ ] **Step 1: Grep gate**

```bash
rg -n "using VContainer|VContainer\." Assets/Playcenter --glob '*.cs'
```

Expected: no matches

- [ ] **Step 2: Document bridge in RecipeRageGameEntry** — single place resolves `client.Services.Get<T>()` for ports game systems need if not already in VContainer as same instance

- [ ] **Step 3: Commit**

```bash
git commit -m "$(cat <<'EOF'
chore(sdk): enforce no VContainer references under Playcenter

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 9: Vendor firewall audit + AI skill + wiki

**Files:**
- Create: `.github/skills/playcenter-sdk/SKILL.md`
- Create: `.claude/skills/playcenter-sdk/SKILL.md` (same content)
- Modify: `wiki/Technical.md` — add Playcenter Studio SDK section (laws S1–S14 summary, boot timeline, delete list)
- Modify: `wiki/LLM-Rules.md` — REQUIRED/FORBIDDEN for SDK
- Modify: `wiki/log.md` — entry for 2026-07-20 SDK cutover
- Audit: `rg -n "using Epic\.|Epic\.OnlineServices" Assets/_KitchenClash/Presentation Assets/_KitchenClash/Application --glob '*.cs'`
- Fix leaks by moving to Infrastructure adapters only (minimal moves; no gameplay rewrite)

**SKILL.md minimum sections:**

```markdown
---
name: playcenter-sdk
description: Integrate and extend Playcenter Studio SDK (modules, shell, vendor firewall).
---

# Playcenter Studio SDK

## When to use
...
## Glossary
...
## Integrate a game
```csharp
// builder snippet from spec
```
## Add a module
...
## FORBIDDEN
- VContainer inside Assets/Playcenter
- Epic/NGO in game Presentation/Application shell
- Orphan session DI
- Dual boot / BootSequence
- Copying SDK UXML into the game
## Delete gates
...
## See also
- docs/superpowers/specs/2026-07-20-playcenter-studio-sdk-design.md
- wiki/Technical.md
```

- [ ] **Step 1: Write skill files**

- [ ] **Step 2: Wiki updates**

- [ ] **Step 3: Vendor grep; fix high-confidence shell leaks only**

- [ ] **Step 4: Final test suite**

```bash
dotnet test RecipeRage.Tests.EditMode.csproj -nologo
rg -n "BootSequence" Assets --glob '*.cs'
rg -n "using VContainer" Assets/Playcenter --glob '*.cs'
```

Expected: tests pass; no BootSequence; no VContainer in Playcenter

- [ ] **Step 5: Commit**

```bash
git commit -m "$(cat <<'EOF'
docs(sdk): add playcenter-sdk skill and wiki laws

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 10: Spec status + design doc mark implemented

**Files:**
- Modify: `docs/superpowers/specs/2026-07-20-playcenter-studio-sdk-design.md` — Status line → Implemented (tasks 1–9); add commit table if desired

- [ ] **Step 1: Update status**

- [ ] **Step 2: Commit**

```bash
git commit -m "$(cat <<'EOF'
docs(sdk): mark studio SDK design implemented

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

## Spec coverage checklist (author self-review)

| Spec requirement | Task |
|------------------|------|
| ServiceRegistry Builder DI | 1 |
| ModuleHost + weights + BootFailure | 2 |
| PlaycenterClient + IGameEntry | 3 |
| Loading/Splash shell + theme cursor fix | 4 |
| Default module pack / no login mid-bar | 5 |
| Full BootSequence delete + bootstrap cutover | 6 |
| Gate screens + Settings in SDK; delete dupes | 7 |
| Zero VContainer in Playcenter | 8 |
| Vendor firewall + AI skill + wiki | 9 |
| Hard cutover / no legacy boot | 6 delete gate |
| Session ownership preserved | 6 (do not touch CreateSession laws) |
| IAppFlow post-ready only | 6 |

## Placeholder / consistency notes (resolved in plan)

- Public resolve API named **`IPlaycenterServices`** (not `IServiceProvider`) to avoid `System.IServiceProvider` clash.
- Pure SDK vs Unity shell split: **`Playcenter.SDK`** + **`Playcenter.SDK.Unity`**.
- Force update game-specific RC types accessed via **`IForceUpdatePolicy`** registered by RecipeRage.
- `StartColdBoot` / `NotifyBootComplete` removed when callers gone (Task 6), not left as shims.

---

## Execution notes

- After each task: commit before starting the next.
- Unity must refresh `.csproj` / `.meta` for new asmdefs — run Editor once if `dotnet` cannot find projects.
- Do not implement beyond a task’s delete gate in the same commit without tests green.
