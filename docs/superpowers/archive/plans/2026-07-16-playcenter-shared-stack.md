# Playcenter Shared-Stack Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Promote shared multi-title UI Toolkit host, DOTween animation, EOS auth/storage, session/social ports, optional Persistence, and NetworkObjectPool into Playcenter modules with hard cutover and a restricted dependency DAG.

**Architecture:** Approach C from the design spec — Tier 0 pure modules (Shell, GameFlow, Services ports, UI ports + pure stack manager) plus Tier 1 Unity-thin modules (Animation, UI.Toolkit, EOS, Persistence gated, Networking pool). Only Shell is a shared hub; GameFlow ⟂ Services ⟂ UI; no Playcenter → KitchenClash; hard cutover every phase (move not copy; no aliases/obsolete duals/Console fallback).

**Tech Stack:** Unity 6, UI Toolkit, DOTween, UniTask (T1 only), VContainer (game Composition only), PlayEveryWare EOS, NGO (Networking only), NUnit EditMode, existing `Playcenter.*.csproj` CLI pattern for pure modules.

## Global Constraints

- Spec: `docs/superpowers/specs/2026-07-16-playcenter-shared-stack-design.md` (Approach C, Wave 1 + Wave 2 High-only).
- Hard cutover always: move, don’t copy; delete KitchenClash originals in the same phase; no type aliases, obsolete stubs, dual namespaces, `#if PLAYCENTER` shims, Console logging fallbacks.
- Restricted DAG only (allowed edges):
  - Shell ← (optional consumers); Shell references nothing Playcenter.
  - GameFlow ⟂ Services ⟂ UI (ports) — no peer refs.
  - Animation → Shell only (+ DOTween/UniTask/Unity).
  - UI.Toolkit → Shell, UI (ports), Animation.
  - EOS → Shell, Services.
  - Persistence → Shell, Services (storage ports).
  - Networking → Shell (+ NGO).
  - No Playcenter → KitchenClash; no cycles.
- Pure modules keep `noEngineReferences: true`. Unity-thin modules set `noEngineReferences: false`.
- VContainer types must not appear in any Playcenter public API (use `IScreenInstanceFactory` / hooks).
- Game IP stays KitchenClash: cooking, chefs, maps, title UXML/USS, lobby/MM/friends **implementations**, player data DTOs, NetworkGameManager/spawn rules.
- Obsolete `EOSAuthService` is deleted during EOS phase.
- Do not commit unrelated WIP (maps, fonts, combat, packages-lock, VirtualProjectsConfig, etc.).
- Commit trailer on every commit:
  `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>`
- Phase order locked: pure stack → Animation → UI.Toolkit → EOS → session ports → Persistence (gated) → Networking → docs.
- Med/Low backlog (Firebase, Input, Audio plumbing, etc.) is **out of scope** (spec §15).

---

## File map

| Path | Role |
|------|------|
| `Assets/Playcenter/UI/Runtime/IUIScreenStackManager.cs` | NEW pure stack contract (from Presentation) |
| `Assets/Playcenter/UI/Runtime/UIScreenStackManager.cs` | NEW pure stack impl |
| `Assets/Playcenter/UI/README.md` | Update layout |
| `Playcenter.UI.csproj` | Compile includes for stack types |
| `Assets/Scripts/Tests/EditMode/Playcenter/UIScreenStackManagerTests.cs` | NEW pure stack tests |
| `Assets/Playcenter/Animation/Runtime/*` | NEW Unity-thin DOTween module |
| `Assets/Playcenter/UI.Toolkit/Runtime/*` | NEW Unity-thin UI host |
| `Assets/Playcenter/EOS/Runtime/*` | NEW Unity-thin auth/storage/mapper |
| `Assets/Playcenter/Services/Runtime/Session/*` + `Social/*` + models | Wave 2 pure ports + DTOs |
| `Assets/Playcenter/Persistence/Runtime/*` | Wave 2 gated save providers |
| `Assets/Playcenter/Networking/Runtime/*` | Wave 2 NetworkObjectPool |
| Delete KitchenClash originals listed per task | Hard purge |
| Asmdefs + `RootLifetimeScope` + consumers | Usings / references per phase |
| `wiki/Technical.md` + module READMEs | Phase 8 docs |

---

### Task 1: Pure UI stack manager → Playcenter.UI

**Files:**
- Create: `Assets/Playcenter/UI/Runtime/IUIScreenStackManager.cs`
- Create: `Assets/Playcenter/UI/Runtime/UIScreenStackManager.cs`
- Create: `Assets/Scripts/Tests/EditMode/Playcenter/UIScreenStackManagerTests.cs`
- Modify: `Playcenter.UI.csproj` — add Compile includes for the two new Runtime files (mirror existing IUIService includes)
- Modify: `Assets/Playcenter/UI/README.md` — document stack types
- Modify: `Assets/_KitchenClash/Composition/RootLifetimeScope.cs` — `using Playcenter.UI` already present; ensure stack registration resolves Playcenter types
- Modify: all Presentation files that reference `KitchenClash.Presentation.Common.IUIScreenStackManager` / `UIScreenStackManager` (at minimum `UIService.cs`, partials)
- Delete: `Assets/_KitchenClash/Presentation/Common/IUIScreenStackManager.cs`
- Delete: `Assets/_KitchenClash/Presentation/Common/UIScreenStackManager.cs`

**Interfaces:**
- Consumes: `Playcenter.UI.UIScreenCategory` (already shipped)
- Produces: `Playcenter.UI.IUIScreenStackManager`, `Playcenter.UI.UIScreenStackManager` with the signatures below

- [ ] **Step 1: Write the failing EditMode tests**

```csharp
// Assets/Scripts/Tests/EditMode/Playcenter/UIScreenStackManagerTests.cs
using System;
using NUnit.Framework;
using Playcenter.UI;

namespace KitchenClash.Tests.EditMode.Playcenter
{
    public class UIScreenStackManagerTests
    {
        private UIScreenStackManager _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new UIScreenStackManager();
        }

        [Test]
        public void Push_ThenPeek_ReturnsPushedType()
        {
            Type screen = typeof(string);
            _sut.Push(screen, UIScreenCategory.Screen);
            Assert.AreEqual(screen, _sut.Peek(UIScreenCategory.Screen));
            Assert.AreEqual(1, _sut.GetStackDepth(UIScreenCategory.Screen));
        }

        [Test]
        public void Pop_EmptyCategory_ReturnsNull()
        {
            Assert.IsNull(_sut.Pop(UIScreenCategory.Modal));
        }

        [Test]
        public void Pop_AfterPush_ReturnsTypeAndEmpties()
        {
            Type screen = typeof(int);
            _sut.Push(screen, UIScreenCategory.Popup);
            Assert.AreEqual(screen, _sut.Pop(UIScreenCategory.Popup));
            Assert.AreEqual(0, _sut.GetStackDepth(UIScreenCategory.Popup));
        }

        [Test]
        public void PopSpecific_RemovesMiddleEntry()
        {
            _sut.Push(typeof(int), UIScreenCategory.Screen);
            _sut.Push(typeof(string), UIScreenCategory.Screen);
            _sut.Push(typeof(float), UIScreenCategory.Screen);
            _sut.PopSpecific(typeof(string), UIScreenCategory.Screen);
            Assert.IsFalse(_sut.IsInHistory(typeof(string)));
            Assert.IsTrue(_sut.IsInHistory(typeof(int)));
            Assert.IsTrue(_sut.IsInHistory(typeof(float)));
            Assert.AreEqual(2, _sut.GetStackDepth(UIScreenCategory.Screen));
        }

        [Test]
        public void ClearCategory_And_ClearAll_EmptyStacks()
        {
            _sut.Push(typeof(int), UIScreenCategory.Screen);
            _sut.Push(typeof(string), UIScreenCategory.Modal);
            _sut.ClearCategory(UIScreenCategory.Screen);
            Assert.AreEqual(0, _sut.GetStackDepth(UIScreenCategory.Screen));
            Assert.AreEqual(1, _sut.GetStackDepth(UIScreenCategory.Modal));
            _sut.ClearAll();
            Assert.AreEqual(0, _sut.GetStackDepth(UIScreenCategory.Modal));
            Assert.IsFalse(_sut.IsInHistory(typeof(string)));
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test RecipeRage.Tests.EditMode.csproj --filter="FullyQualifiedName~UIScreenStackManagerTests" -nologo
```

Expected: FAIL (types not found in `Playcenter.UI` or compile error).

- [ ] **Step 3: Add pure stack types to Playcenter.UI**

```csharp
// Assets/Playcenter/UI/Runtime/IUIScreenStackManager.cs
using System;

namespace Playcenter.UI
{
    /// <summary>
    /// Engine-free per-category screen stack history.
    /// </summary>
    public interface IUIScreenStackManager
    {
        void Push(Type screenType, UIScreenCategory category);
        Type Pop(UIScreenCategory category);
        Type Peek(UIScreenCategory category);
        void PopSpecific(Type screenType, UIScreenCategory category);
        void ClearCategory(UIScreenCategory category);
        void ClearAll();
        bool IsInHistory(Type screenType);
        int GetStackDepth(UIScreenCategory category);
    }
}
```

```csharp
// Assets/Playcenter/UI/Runtime/UIScreenStackManager.cs
using System;
using System.Collections.Generic;

namespace Playcenter.UI
{
    public class UIScreenStackManager : IUIScreenStackManager
    {
        private readonly Dictionary<UIScreenCategory, Stack<Type>> _stacks = new();

        public void Push(Type screenType, UIScreenCategory category)
        {
            if (!_stacks.TryGetValue(category, out Stack<Type> stack))
            {
                stack = new Stack<Type>();
                _stacks[category] = stack;
            }
            stack.Push(screenType);
        }

        public Type Pop(UIScreenCategory category)
        {
            if (_stacks.TryGetValue(category, out Stack<Type> stack) && stack.Count > 0)
                return stack.Pop();
            return null;
        }

        public Type Peek(UIScreenCategory category)
        {
            if (_stacks.TryGetValue(category, out Stack<Type> stack) && stack.Count > 0)
                return stack.Peek();
            return null;
        }

        public void PopSpecific(Type screenType, UIScreenCategory category)
        {
            if (!_stacks.TryGetValue(category, out Stack<Type> stack)) return;

            Stack<Type> temp = new Stack<Type>();
            while (stack.Count > 0)
            {
                Type current = stack.Pop();
                if (current != screenType)
                    temp.Push(current);
            }
            while (temp.Count > 0)
                stack.Push(temp.Pop());
        }

        public void ClearCategory(UIScreenCategory category)
        {
            if (_stacks.TryGetValue(category, out Stack<Type> stack))
                stack.Clear();
        }

        public void ClearAll()
        {
            _stacks.Clear();
        }

        public bool IsInHistory(Type screenType)
        {
            foreach (Stack<Type> stack in _stacks.Values)
            {
                if (stack.Contains(screenType))
                    return true;
            }
            return false;
        }

        public int GetStackDepth(UIScreenCategory category)
        {
            return _stacks.TryGetValue(category, out Stack<Type> stack) ? stack.Count : 0;
        }
    }
}
```

- [ ] **Step 4: Wire csproj + README**

Add to `Playcenter.UI.csproj` the same style of `<Compile Include="Assets/Playcenter/UI/Runtime/IUIScreenStackManager.cs" />` and `UIScreenStackManager.cs` entries used for `IUIService.cs`.

Update `Assets/Playcenter/UI/README.md` layout to:

```
Runtime/
  Playcenter.UI.asmdef
  IUIService.cs
  NotificationType.cs
  UIScreenCategory.cs
  IUIScreenStackManager.cs
  UIScreenStackManager.cs
```

- [ ] **Step 5: Hard cutover Presentation**

1. Delete `Assets/_KitchenClash/Presentation/Common/IUIScreenStackManager.cs` and `UIScreenStackManager.cs` (and `.meta` if present).
2. Ensure `UIService` and any other consumer use `using Playcenter.UI;` only (remove any `KitchenClash.Presentation.Common` stack type references).
3. `RootLifetimeScope.RegisterUI` already has:
   `builder.Register<UIScreenStackManager>(Lifetime.Singleton).As<IUIScreenStackManager>();`
   — both types now resolve from `Playcenter.UI`.

- [ ] **Step 6: Run tests and build**

```bash
dotnet build Playcenter.UI.csproj -nologo
dotnet test RecipeRage.Tests.EditMode.csproj --filter="FullyQualifiedName~UIScreenStackManagerTests" -nologo
dotnet build RecipeRage.Presentation.csproj -nologo
dotnet build RecipeRage.Composition.csproj -nologo
```

Expected: all PASS / 0 errors.

- [ ] **Step 7: Commit**

```bash
git add Assets/Playcenter/UI/ Assets/Scripts/Tests/EditMode/Playcenter/ \
  Assets/_KitchenClash/Presentation/Common/ Assets/_KitchenClash/Composition/RootLifetimeScope.cs \
  Playcenter.UI.csproj
git commit -m "$(cat <<'EOF'
feat(playcenter.ui): promote pure UIScreenStackManager

Move stack contract + impl into Playcenter.UI with EditMode tests.
Hard cutover Presentation; no dual types.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 2: Playcenter.Animation module + hard cutover

**Files:**
- Create: `Assets/Playcenter/Animation/Runtime/Playcenter.Animation.asmdef`
- Create (move): all of `Assets/_KitchenClash/Infrastructure/Animation/*` except the old asmdef into `Assets/Playcenter/Animation/Runtime/`
- Create: `Assets/Playcenter/Animation/Runtime/IAnimationService.cs` (from Application)
- Create: `Assets/Playcenter/Animation/README.md`
- Modify: every consumer of `KitchenClash.Application.IAnimationService` or `KitchenClash.Infrastructure.Animation.*` → `Playcenter.Animation`
- Modify: `Assets/_KitchenClash/Composition/RootLifetimeScope.cs` — drop `using KitchenClash.Infrastructure.Animation`; add `using Playcenter.Animation`
- Modify: Presentation asmdef / Infrastructure asmdefs that referenced `KitchenClash.Infrastructure.Animation` → `Playcenter.Animation`
- Delete: `Assets/_KitchenClash/Application/Interfaces/IAnimationService.cs`
- Delete: entire `Assets/_KitchenClash/Infrastructure/Animation/` folder (including old asmdef)
- Note: `Assets/_KitchenClash/Presentation/Common/TweenExtensions.cs` is a **different** file from Animation’s `TweenExtensions.cs` — do not delete Presentation’s unless proven duplicate; Animation’s moves with the module

**Interfaces:**
- Consumes: `Playcenter.Shell.GameLogger` (if used), DOTween, UniTask, UnityEngine / UIElements
- Produces: namespace `Playcenter.Animation` with:
  - `IAnimationService` (same method surface as current Application interface — UniTask kept)
  - `AnimationService`, `IUIAnimator`, `DOTweenUIAnimator`, `ITransformAnimator`, `DOTweenTransformAnimator`, `SlideDirection`, `TweenExtensions`

- [ ] **Step 1: Create asmdef**

```json
{
    "name": "Playcenter.Animation",
    "rootNamespace": "Playcenter.Animation",
    "references": [
        "Playcenter.Shell",
        "DOTween.Modules",
        "UniTask"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

If DOTween asmdef name differs in this project, match the reference string used by the old `KitchenClash.Infrastructure.Animation.asmdef` exactly.

- [ ] **Step 2: Move sources and rewrite namespaces**

For each file moved from Infrastructure/Animation and for `IAnimationService`:

1. Set `namespace Playcenter.Animation`
2. Remove `using KitchenClash.Application` / `KitchenClash.Infrastructure.Animation`
3. Keep UniTask + UnityEngine.UIElements / Transform signatures unchanged on `IAnimationService`

`IAnimationService` body (unchanged surface):

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Playcenter.Animation
{
    public interface IAnimationService
    {
        UniTask FadeIn(VisualElement element, float duration, CancellationToken token = default);
        UniTask FadeOut(VisualElement element, float duration, CancellationToken token = default);
        UniTask ScaleInUI(VisualElement element, float duration, CancellationToken token = default);
        UniTask ScaleOutUI(VisualElement element, float duration, CancellationToken token = default);

        UniTask MoveTo(Transform transform, Vector3 target, float duration, CancellationToken token = default);
        UniTask ScaleTo(Transform transform, Vector3 target, float duration, CancellationToken token = default);
        UniTask RotateTo(Transform transform, Vector3 target, float duration, CancellationToken token = default);
        UniTask Punch(Transform transform, Vector3 direction, float duration, CancellationToken token = default);
        UniTask Shake(Transform transform, float duration, float strength, CancellationToken token = default);

        void FloatYoyo(VisualElement element, float distance, float duration);
        void CrossfadeLabel(VisualElement label, string newText, float fontSize, float duration);
        void BlurIn(VisualElement element, float blurAmount, float duration);
        void TrackingIn(VisualElement element, float startTracking, float endTracking, float duration);
        void SlideInfinite(VisualElement element, float startPercent, float endPercent, float duration);

        void KillAnimations(VisualElement element);
        void KillAnimations(Transform transform);
        void KillAllAnimations();
    }
}
```

Move implementation files with namespace rewrite only (logic unchanged):
- `AnimationService.cs`
- `IUIAnimator.cs` / `DOTweenUIAnimator.cs`
- `ITransformAnimator.cs` / `DOTweenTransformAnimator.cs`
- `SlideDirection.cs`
- `TweenExtensions.cs` (Animation copy)

- [ ] **Step 3: README**

```markdown
# Playcenter.Animation

Unity-thin DOTween animation service for multi-title shells.

## Rules
1. May reference Playcenter.Shell, DOTween, UniTask, Unity.
2. Must NOT reference Playcenter.UI, Services, GameFlow, EOS, or KitchenClash.
3. Public async API uses UniTask (Unity-thin lock).
4. Composition registration stays in the game.
```

- [ ] **Step 4: Consumer cutover**

```bash
rg -n "KitchenClash\.Infrastructure\.Animation|KitchenClash\.Application\.IAnimationService|using KitchenClash\.Application" \
  --glob '*.cs' -g '!Library/**' -g '!docs/**'
```

For every hit that uses `IAnimationService` or Animation types:
- `using Playcenter.Animation;`
- Remove Application interface import if only used for animation

Update asmdefs that referenced `KitchenClash.Infrastructure.Animation` to `Playcenter.Animation` (Presentation, Composition, any Infrastructure parent).

`RootLifetimeScope` registration stays shape-identical:

```csharp
builder.Register<DOTweenUIAnimator>(Lifetime.Singleton).As<IUIAnimator>();
builder.Register<DOTweenTransformAnimator>(Lifetime.Singleton).As<ITransformAnimator>();
builder.Register<AnimationService>(Lifetime.Singleton).As<IAnimationService>();
```

- [ ] **Step 5: Delete old Animation assembly folder and Application interface**

Delete `Assets/_KitchenClash/Infrastructure/Animation/` entirely and `Assets/_KitchenClash/Application/Interfaces/IAnimationService.cs`.

- [ ] **Step 6: Build**

```bash
dotnet build RecipeRage.Presentation.csproj -nologo
dotnet build RecipeRage.Composition.csproj -nologo
dotnet test RecipeRage.Tests.EditMode.csproj --no-build -nologo || \
  dotnet test RecipeRage.Tests.EditMode.csproj -nologo
```

Expected: 0 errors. (Unity may need to regenerate csproj for `Playcenter.Animation` — if CLI cannot see the new asmdef yet, open Unity once or build via existing generated project name after refresh.)

- [ ] **Step 7: Commit**

```bash
git add Assets/Playcenter/Animation/ Assets/_KitchenClash/Infrastructure/Animation/ \
  Assets/_KitchenClash/Application/Interfaces/IAnimationService.cs \
  Assets/_KitchenClash/Composition/ Assets/_KitchenClash/Presentation/ \
  Assets/_KitchenClash/**/*.asmdef
git commit -m "$(cat <<'EOF'
feat(playcenter.animation): extract DOTween animation module

Move IAnimationService + DOTween adapters to Playcenter.Animation.
Hard cutover; delete KitchenClash.Infrastructure.Animation.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 3: Playcenter.UI.Toolkit + VContainer decoupling + hard cutover

**Files:**
- Create: `Assets/Playcenter/UI.Toolkit/Runtime/Playcenter.UI.Toolkit.asmdef`
- Create: `Assets/Playcenter/UI.Toolkit/Runtime/IScreenInstanceFactory.cs`
- Create (move): from `Assets/_KitchenClash/Presentation/Common/`:
  - `UIService.cs`, `UIService.Navigation.cs`, `UIService.ScreenOps.cs`
  - `BaseUIScreen.cs`, `UIScreenController.cs`
  - `UIScreenRegistry.cs`, `UIScreenAttribute.cs`, `UIScreenPriority.cs`
  - `UITransitionHandler.cs`, `UITransitionType.cs`
  - `INotificationScreen.cs` (if only used by host)
- Create: `Assets/Playcenter/UI.Toolkit/README.md`
- Create: `Assets/_KitchenClash/Presentation/Common/VContainerScreenInstanceFactory.cs` (game adapter)
- Modify: all screens inheriting `BaseUIScreen` → `using Playcenter.UI.Toolkit`
- Modify: `RootLifetimeScope` registration for UIService factory
- Delete: moved files from Presentation.Common (keep game-only: `BaseViewModel`, `LocKeys`, Presentation `TweenExtensions` if still used by screens)

**Interfaces:**
- Consumes: `Playcenter.UI.IUIService`, `IUIScreenStackManager`, `UIScreenCategory`, `NotificationType`; `Playcenter.Shell`; optionally `Playcenter.Animation` for transitions
- Produces: `Playcenter.UI.Toolkit.*` host types + `IScreenInstanceFactory`

- [ ] **Step 1: Add factory port (no VContainer)**

```csharp
// Assets/Playcenter/UI.Toolkit/Runtime/IScreenInstanceFactory.cs
using System;

namespace Playcenter.UI.Toolkit
{
    /// <summary>
    /// Creates screen instances without exposing a DI container type.
    /// Game supplies a VContainer-backed implementation.
    /// </summary>
    public interface IScreenInstanceFactory
    {
        object Create(Type screenType);
    }
}
```

- [ ] **Step 2: Create Toolkit asmdef**

```json
{
    "name": "Playcenter.UI.Toolkit",
    "rootNamespace": "Playcenter.UI.Toolkit",
    "references": [
        "Playcenter.UI",
        "Playcenter.Shell",
        "Playcenter.Animation",
        "UniTask"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

Do **not** reference VContainer, Playcenter.Services, Playcenter.GameFlow, Playcenter.EOS, or KitchenClash.

- [ ] **Step 3: Move host types; strip VContainer from UIService/BaseUIScreen**

Namespace for all moved host types: `Playcenter.UI.Toolkit`.

**UIService constructor change** (replace `VContainer.IObjectResolver`):

```csharp
public partial class UIService : IUIService, IDisposable
{
    private readonly IScreenInstanceFactory _screenFactory;
    private readonly IUIScreenStackManager _stackManager;
    private UIDocument _uiDocument;
    // ... existing fields except VContainer resolvers ...

    public UIService(
        IScreenInstanceFactory screenFactory,
        UIDocument uiDocument,
        IUIScreenStackManager stackManager)
    {
        _screenFactory = screenFactory ?? throw new ArgumentNullException(nameof(screenFactory));
        _uiDocument = uiDocument;
        _stackManager = stackManager ?? throw new ArgumentNullException(nameof(stackManager));
    }

    public void SetCurrentScope(object scope)
    {
        // Optional: if factory is scope-aware, cast scope in game factory.
        // Toolkit must not reference VContainer.
        (_screenFactory as IScopeAwareScreenFactory)?.SetScope(scope);
    }
}
```

If `IStartable` / `ITickable` were used only for VContainer entry points, **drop those interfaces from Toolkit UIService**. Game Composition registers a thin entry-point adapter:

```csharp
// Assets/_KitchenClash/Presentation/Common/UIServiceEntryPoint.cs
using VContainer.Unity;
using Playcenter.UI.Toolkit;

namespace KitchenClash.Presentation.Common
{
    public sealed class UIServiceEntryPoint : IStartable, ITickable
    {
        private readonly UIService _ui;

        public UIServiceEntryPoint(UIService ui) => _ui = ui;

        public void Start() => _ui.Start();
        public void Tick() => _ui.Tick();
    }
}
```

Keep `Start()`, `Tick()`, `Update(float)` methods on `UIService` as ordinary methods.

**BaseUIScreen:** remove `[Inject]` / `using VContainer`. Replace with:

```csharp
protected IUIService UIService { get; private set; }

internal void SetUIService(IUIService uiService)
{
    UIService = uiService;
}
```

When `UIService` creates/resolves a screen via factory, call `SetUIService(this)` after create (or factory does it). Search current resolve path in `UIService.ScreenOps.cs` / Navigation and replace `_container.Resolve` / `_currentScope.Resolve` with `_screenFactory.Create(screenType)`.

**UIScreenAttribute / Registry / Controller / Priority / Transitions:** move with namespace rewrite only; attribute stays reflection-based.

- [ ] **Step 4: Game VContainer factory**

```csharp
// Assets/_KitchenClash/Presentation/Common/VContainerScreenInstanceFactory.cs
using System;
using Playcenter.UI.Toolkit;
using VContainer;

namespace KitchenClash.Presentation.Common
{
    public sealed class VContainerScreenInstanceFactory : IScreenInstanceFactory, IScopeAwareScreenFactory
    {
        private IObjectResolver _resolver;

        public VContainerScreenInstanceFactory(IObjectResolver resolver)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        public void SetScope(object scope)
        {
            if (scope is IObjectResolver resolver)
                _resolver = resolver;
        }

        public object Create(Type screenType)
        {
            return _resolver.Resolve(screenType);
        }
    }
}
```

```csharp
// Assets/Playcenter/UI.Toolkit/Runtime/IScopeAwareScreenFactory.cs
namespace Playcenter.UI.Toolkit
{
    public interface IScopeAwareScreenFactory
    {
        void SetScope(object scope);
    }
}
```

- [ ] **Step 5: RootLifetimeScope registration**

```csharp
builder.Register<UIScreenStackManager>(Lifetime.Singleton).As<IUIScreenStackManager>();
builder.Register<VContainerScreenInstanceFactory>(Lifetime.Singleton)
    .As<IScreenInstanceFactory>()
    .As<IScopeAwareScreenFactory>();
builder.Register<UIService>(Lifetime.Singleton).As<IUIService>().AsSelf();
builder.RegisterEntryPoint<UIServiceEntryPoint>();
// Animation registrations unchanged from Task 2
```

Usings: `Playcenter.UI`, `Playcenter.UI.Toolkit`, `KitchenClash.Presentation.Common` (factory + entry point only).

- [ ] **Step 6: Retarget all screens**

```bash
rg -n "KitchenClash\.Presentation\.Common|BaseUIScreen|UIScreenAttribute|UIScreenRegistry" \
  --glob '*.cs' Assets/_KitchenClash/Presentation -g '!Library/**'
```

Every screen file:
- `using Playcenter.UI.Toolkit;`
- inherit `BaseUIScreen` from Toolkit
- keep screen namespace `KitchenClash.Presentation.Screens` (or current)

Presentation asmdef must reference `Playcenter.UI.Toolkit` and `Playcenter.Animation`.

- [ ] **Step 7: Delete moved Presentation.Common host files**

Delete the moved originals. Leave game-only Common files.

- [ ] **Step 8: Build + EditMode**

```bash
dotnet build RecipeRage.Presentation.csproj -nologo
dotnet build RecipeRage.Composition.csproj -nologo
dotnet test RecipeRage.Tests.EditMode.csproj -nologo
```

Expected: 0 errors. Grep must find **zero** `VContainer` usings under `Assets/Playcenter/UI.Toolkit/`.

```bash
rg -n "VContainer" Assets/Playcenter/UI.Toolkit --glob '*.cs'
```

Expected: no matches.

- [ ] **Step 9: Commit**

```bash
git add Assets/Playcenter/UI.Toolkit/ Assets/_KitchenClash/Presentation/ \
  Assets/_KitchenClash/Composition/RootLifetimeScope.cs
git commit -m "$(cat <<'EOF'
feat(playcenter.ui.toolkit): extract UI Toolkit host

Move UIService/BaseUIScreen stack host to Playcenter.UI.Toolkit.
Decouple VContainer via IScreenInstanceFactory; hard cutover screens.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 4: Playcenter.EOS shared slice + hard cutover

**Files:**
- Create: `Assets/Playcenter/EOS/Runtime/Playcenter.EOS.asmdef`
- Create: `Assets/Playcenter/EOS/Runtime/IEOSConfig.cs`
- Create: `Assets/Playcenter/EOS/Runtime/IAuthLifecycleHooks.cs`
- Create: `Assets/Playcenter/EOS/Runtime/EosResultMapper.cs` (generic string/bool mapper — no KitchenClash types)
- Create (move): `AuthenticationService.cs`, `EOSCloudStorageProvider.cs`
- Create: `Assets/Playcenter/EOS/README.md`
- Create: `Assets/_KitchenClash/Infrastructure/EOS/KitchenClashAuthLifecycleHooks.cs` (publishes `LoginSuccessEvent`, updates settings)
- Create: `Assets/_KitchenClash/Infrastructure/Configuration/UGSConfigEOSAdapter.cs` or implement `IEOSConfig` on existing config
- Modify: `RootLifetimeScope` registrations
- Delete: `Assets/_KitchenClash/Infrastructure/EOS/EOSAuthService.cs` (obsolete)
- Delete: moved auth/storage originals from KitchenClash.EOS
- **Stay game:** `EOSLobbyService`, `EOSMatchmakingService`, `EOSTeamManager`, `EOSPlayerManager`, `EOSFriendsService`(+factory), transport, `EOSPlayerDataService`, lobby providers

**Interfaces:**
- Consumes: `Playcenter.Shell`, `Playcenter.Services` (`IAuthService`, `AuthResult`, `ICloudStorageProvider`), PlayEveryWare EOS, UniTask as needed
- Produces: `Playcenter.EOS.AuthenticationService`, `EOSCloudStorageProvider`, `IEOSConfig`, `IAuthLifecycleHooks`, `EosResultMapper`

- [ ] **Step 1: Define config + lifecycle hooks (no KitchenClash types)**

```csharp
// Assets/Playcenter/EOS/Runtime/IEOSConfig.cs
namespace Playcenter.EOS
{
    /// <summary>
    /// Title-supplied EOS/UGS identifiers. Game maps ScriptableObject → this.
    /// </summary>
    public interface IEOSConfig
    {
        /// <summary>Unity Gaming Services project id (if UGS bridge is used).</summary>
        string UgsProjectId { get; }

        /// <summary>When false, skip UGS authentication bridge.</summary>
        bool EnableUgsBridge { get; }
    }
}
```

```csharp
// Assets/Playcenter/EOS/Runtime/IAuthLifecycleHooks.cs
namespace Playcenter.EOS
{
    /// <summary>
    /// Game-side side effects after auth (events, settings). Optional no-op allowed.
    /// </summary>
    public interface IAuthLifecycleHooks
    {
        void OnLoginSucceeded(string productUserId, string displayName, bool isGuest, string loginMethod);
        void OnLogout();
    }
}
```

```csharp
// Assets/Playcenter/EOS/Runtime/EosResultMapper.cs
using Epic.OnlineServices;

namespace Playcenter.EOS
{
    public static class EosResultMapper
    {
        public static bool IsSuccess(Result result) => result == Result.Success;

        public static string ToErrorCode(Result result) => result.ToString();
    }
}
```

Game lobby code that previously used `EosResultMapper.ToLobbyOpResult` becomes:

```csharp
// in KitchenClash lobby service
if (!Playcenter.EOS.EosResultMapper.IsSuccess(result))
    return LobbyOpResult.Fail(Playcenter.EOS.EosResultMapper.ToErrorCode(result), result.ToString());
return LobbyOpResult.Ok();
```

- [ ] **Step 2: Asmdef**

```json
{
    "name": "Playcenter.EOS",
    "rootNamespace": "Playcenter.EOS",
    "references": [
        "Playcenter.Shell",
        "Playcenter.Services",
        "UniTask"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

Add the same PlayEveryWare / Epic Online Services asmdef references that `KitchenClash.Infrastructure.EOS.asmdef` currently lists (copy those reference strings exactly). Do **not** reference KitchenClash.*, UI, GameFlow, Animation.

- [ ] **Step 3: Move and sanitize AuthenticationService**

Namespace: `Playcenter.EOS`.

Constructor becomes:

```csharp
public AuthenticationService(
    IEventBus eventBus,
    IEOSConfig eosConfig,
    IAuthLifecycleHooks lifecycleHooks = null)
{
    _eventBus = eventBus;
    _eosConfig = eosConfig ?? throw new ArgumentNullException(nameof(eosConfig));
    _lifecycleHooks = lifecycleHooks;
}
```

Replace:
- `_saveService.UpdateSettings(s => s.LastLoginMethod = "DeviceID")` → `_lifecycleHooks?.OnLoginSucceeded(..., loginMethod: "DeviceID")` (or split method update into hooks)
- `_eventBus?.Publish(new LoginSuccessEvent ...)` → remove KitchenClash event; hooks handle game event. Keep `_eventBus` only if publishing **Playcenter/Shell-safe** events; otherwise drop bus from auth and use hooks only.

**Lock:** AuthenticationService must compile with **zero** `using KitchenClash.*`.

UGS init must read project id from `_eosConfig`, not `UGSConfig` ScriptableObject.

- [ ] **Step 4: Move EOSCloudStorageProvider**

Namespace `Playcenter.EOS`; keep `ICloudStorageProvider` implementation. Only Shell + Services + EOS SDK usings.

- [ ] **Step 5: Game adapters**

```csharp
// KitchenClashAuthLifecycleHooks.cs
using KitchenClash.Domain;
using Playcenter.EOS;
using Playcenter.Shell;
using KitchenClash.Application;

namespace KitchenClash.Infrastructure.EOS
{
    public sealed class KitchenClashAuthLifecycleHooks : IAuthLifecycleHooks
    {
        private readonly IEventBus _eventBus;
        private readonly ISaveService _saveService;

        public KitchenClashAuthLifecycleHooks(IEventBus eventBus, ISaveService saveService)
        {
            _eventBus = eventBus;
            _saveService = saveService;
        }

        public void OnLoginSucceeded(string productUserId, string displayName, bool isGuest, string loginMethod)
        {
            _saveService?.UpdateSettings(s => s.LastLoginMethod = loginMethod);
            _eventBus?.Publish(new LoginSuccessEvent { UserId = productUserId, DisplayName = displayName });
        }

        public void OnLogout() { }
    }
}
```

Implement `IEOSConfig` from `UGSConfig` (adapter class or partial):

```csharp
public sealed class UgsEosConfigAdapter : IEOSConfig
{
    private readonly UGSConfig _config;
    public UgsEosConfigAdapter(UGSConfig config) => _config = config;
    public string UgsProjectId => _config != null ? _config.ProjectId : string.Empty; // use real property names from UGSConfig
    public bool EnableUgsBridge => _config != null;
}
```

Inspect `UGSConfig` fields and map real property names (do not invent — open `Assets/_KitchenClash/Infrastructure/Configuration/UGSConfig.cs` and bind exactly).

- [ ] **Step 6: RootLifetimeScope**

```csharp
builder.Register<UgsEosConfigAdapter>(Lifetime.Singleton).As<IEOSConfig>();
builder.Register<KitchenClashAuthLifecycleHooks>(Lifetime.Singleton).As<IAuthLifecycleHooks>();
builder.Register<Playcenter.EOS.AuthenticationService>(Lifetime.Singleton).As<IAuthService>();
builder.Register<Playcenter.EOS.EOSCloudStorageProvider>(Lifetime.Singleton).As<ICloudStorageProvider>();
```

KitchenClash.Infrastructure.EOS asmdef gains reference to `Playcenter.EOS` for remaining lobby/MM types and hooks.

- [ ] **Step 7: Delete obsolete EOSAuthService + moved originals**

Delete `EOSAuthService.cs` entirely. Delete old `AuthenticationService.cs` / `EOSCloudStorageProvider.cs` / old `EosResultMapper.cs` from KitchenClash after lobby call sites updated.

- [ ] **Step 8: Build**

```bash
dotnet build RecipeRage.Composition.csproj -nologo
# or Infrastructure.EOS + Composition after Unity regenerates
dotnet test RecipeRage.Tests.EditMode.csproj -nologo
```

```bash
rg -n "KitchenClash" Assets/Playcenter/EOS --glob '*.cs'
rg -n "EOSAuthService" --glob '*.cs' -g '!docs/**' -g '!Library/**'
```

Expected: no KitchenClash inside Playcenter.EOS; no EOSAuthService references.

- [ ] **Step 9: Commit**

```bash
git add Assets/Playcenter/EOS/ Assets/_KitchenClash/Infrastructure/EOS/ \
  Assets/_KitchenClash/Infrastructure/Configuration/ \
  Assets/_KitchenClash/Composition/RootLifetimeScope.cs
git commit -m "$(cat <<'EOF'
feat(playcenter.eos): extract auth and cloud storage

Move AuthenticationService + EOSCloudStorageProvider to Playcenter.EOS.
Delete obsolete EOSAuthService; lobby/MM remain game-side.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 5: Session/social ports → Playcenter.Services + hard cutover

**Decision locked (spec §5.5.1 coupling note):**

| Port | Action | Reason |
|------|--------|--------|
| `IFriendsService` + `IFriendsServiceFactory` | **Promote** | Generic social surface |
| `ILobbyManager` | **Promote** | Generic lobby lifecycle |
| `ITeamManager` | **Promote** | Generic team lists |
| `IMatchmakingService` | **Promote** | Models below are generic enough (no cooking IP) |
| `FriendInfo`, `FriendRequest` | **Promote** | Social DTOs |
| `LobbyInfo`, `LobbyConfig`, `LobbyType`, `LobbyState` | **Promote** | Session DTOs |
| `PlayerInfo` | **Promote** | Use `int Team` or keep a Playcenter `TeamId` enum (A/B only) — **not** KitchenClash character enums |
| `BotPlayer` | **Promote** as `Playcenter.Services.BotPlayer` | Only id/name/team/ready — multi-title filler |
| `LobbyOpResult` | **Promote** | Shared op result for lobby ports |
| CharacterClass / economy / chef types | **Stay Domain** | Game IP |

If `PlayerInfo.Team` currently uses `KitchenClash.Domain.TeamId`, define:

```csharp
namespace Playcenter.Services
{
    public enum TeamId
    {
        TeamA = 0,
        TeamB = 1
    }
}
```

Game Domain may keep a separate match `TeamId` **only if** values differ; prefer **one** Playcenter `TeamId` and delete Domain duplicate on cutover when they match.

**Files:**
- Create under `Assets/Playcenter/Services/Runtime/`:
  - `Social/IFriendsService.cs`, `Social/IFriendsServiceFactory.cs`
  - `Social/FriendInfo.cs`, `Social/FriendRequest.cs`
  - `Session/ILobbyManager.cs`, `Session/IMatchmakingService.cs`, `Session/ITeamManager.cs`
  - `Session/LobbyInfo.cs`, `Session/LobbyConfig.cs`, `Session/LobbyType.cs`, `Session/LobbyState.cs`
  - `Session/PlayerInfo.cs`, `Session/BotPlayer.cs`, `Session/LobbyOpResult.cs`, `Session/TeamId.cs`
- Modify: `Playcenter.Services.csproj` Compile includes
- Modify: `Assets/Playcenter/Services/README.md`
- Modify: all Application/Infrastructure/Presentation consumers → `Playcenter.Services`
- Delete: Application interface originals + Domain model originals that moved

**Interfaces:**
- Consumes: nothing beyond BCL (pure)
- Produces: ports + models in `Playcenter.Services`

- [ ] **Step 1: Add models (copy surface from Domain/Application, new namespace)**

Use existing property sets from:
- `Assets/_KitchenClash/Domain/Models/FriendInfo.cs`
- `Assets/_KitchenClash/Domain/Models/FriendRequest.cs`
- `Assets/_KitchenClash/Domain/Models/LobbyInfo.cs`
- `Assets/_KitchenClash/Domain/Models/LobbyConfig.cs`
- `Assets/_KitchenClash/Domain/Models/PlayerInfo.cs`
- `Assets/_KitchenClash/Domain/Enums/LobbyType.cs`, `LobbyState.cs`, `TeamId.cs`
- `Assets/_KitchenClash/Application/Services/BotPlayer.cs`
- `Assets/_KitchenClash/Application/Models/LobbyOpResult.cs`

All under `namespace Playcenter.Services`. Replace any `KitchenClash.Domain` nested types with Playcenter equivalents.

- [ ] **Step 2: Add ports**

Copy method/event surfaces from:
- `IFriendsService.cs`, `IFriendsServiceFactory.cs` (locate factory path under Application)
- `ILobbyManager.cs`
- `IMatchmakingService.cs` (change `List<BotPlayer>` to Playcenter `BotPlayer`)
- `ITeamManager.cs`

Use `System.Threading.Tasks.Task` (not UniTask) on ports.

Example friends port header:

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    public interface IFriendsService : IDisposable
    {
        bool IsInitialized { get; }
        string MyFriendCode { get; }
        IReadOnlyList<FriendInfo> Friends { get; }
        IReadOnlyList<FriendInfo> RecentPlayers { get; }
        IReadOnlyList<FriendRequest> PendingRequests { get; }

        event Action OnFriendsListUpdated;
        event Action<FriendRequest> OnFriendRequestReceived;

        void Initialize();
        Task<FriendRequest> SendFriendRequestAsync(string friendCode);
        Task AcceptFriendRequestAsync(string requestId);
        Task RejectFriendRequestAsync(string requestId);
        Task RefreshFriendRequestsAsync();
        Task RefreshFriendsAsync();
        Task RemoveFriendAsync(string friendCode);
        void AddRecentPlayer(string productUserId, string displayName);
        void InviteToParty(string friendCode);
        FriendInfo GetFriend(string friendCode);
    }
}
```

Mirror lobby/matchmaking/team from current Application files with namespace/model swaps only.

- [ ] **Step 3: csproj + README**

Add Compile includes for every new file. README layout gains `Social/` and `Session/`.

- [ ] **Step 4: Hard cutover consumers**

```bash
rg -n "IFriendsService|ILobbyManager|IMatchmakingService|ITeamManager|FriendInfo|LobbyInfo|BotPlayer|LobbyOpResult" \
  --glob '*.cs' Assets/_KitchenClash Assets/Scripts -g '!Library/**'
```

1. Change usings to `Playcenter.Services`.
2. Delete Application/Domain originals listed above.
3. Fix Domain asmdef if it no longer owns those models — Application may drop Domain refs where only session models were needed.
4. Implementations (`EOSFriendsService`, etc.) stay in KitchenClash but implement Playcenter ports.

- [ ] **Step 5: Build + tests**

```bash
dotnet build Playcenter.Services.csproj -nologo
dotnet build RecipeRage.Composition.csproj -nologo
dotnet test RecipeRage.Tests.EditMode.csproj -nologo
```

Retarget `LobbyOpResultTests` to `Playcenter.Services.LobbyOpResult`.

- [ ] **Step 6: Commit**

```bash
git add Assets/Playcenter/Services/ Playcenter.Services.csproj \
  Assets/_KitchenClash/Application/ Assets/_KitchenClash/Domain/ \
  Assets/_KitchenClash/Infrastructure/ Assets/Scripts/Tests/
git commit -m "$(cat <<'EOF'
feat(playcenter.services): promote session and social ports

Move friends/lobby/matchmaking/team contracts + generic models.
Hard cutover Application/Domain; EOS impls stay game-side.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 6: Playcenter.Persistence (DTO gate) + cutover or skip

**Gate review (must run before any move):**

Open `SaveService.cs` + `ISaveService.cs`. Check for hard dependencies on:
- `GameSettingsData`
- Economy / player progress types
- KitchenClash Domain events

| Outcome | Action |
|---------|--------|
| **A — Clean enough** | Extract `LocalStorageProvider`, `StorageProviderFactory`, and a **generic** save orchestrator; settings API stays game-side wrapper |
| **B — Dirty** | Extract **only** `LocalStorageProvider` + `StorageProviderFactory`; leave `SaveService`/`ISaveService` in KitchenClash; document skip in wiki (Task 8) |

**Locked default if gate is ambiguous:** Outcome **B** (providers only) — do not ship a half-coupled SaveService.

**Files (Outcome B — default):**
- Create: `Assets/Playcenter/Persistence/Runtime/Playcenter.Persistence.asmdef`
- Create (move): `LocalStorageProvider.cs`, `StorageProviderFactory.cs`
- Create: `Assets/Playcenter/Persistence/README.md`
- Modify: consumers + RootLifetimeScope usings
- Delete: originals under Infrastructure/Persistence for moved files only
- **Stay game:** `SaveService`, `ISaveService`, `GameSettingsData`, `PlayerDataService*`, economy DTOs

**Files (Outcome A — only if gate passes):**
- Additionally move generic parts of SaveService into Persistence **without** `GameSettingsData` methods
- Game keeps `KitchenClashSaveService : ISaveService` wrapping generic orchestrator + settings

**Asmdef:**

```json
{
    "name": "Playcenter.Persistence",
    "rootNamespace": "Playcenter.Persistence",
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
    "noEngineReferences": false
}
```

- [ ] **Step 1: Write gate note in working tree (not a new markdown doc in repo root)**

In the commit message body, state `Persistence gate: Outcome A` or `Outcome B`.

- [ ] **Step 2: Move LocalStorageProvider + StorageProviderFactory**

Namespace `Playcenter.Persistence`. `LocalStorageProvider` already uses `UnityEngine.Application.persistentDataPath` and `Playcenter.Services.IStorageProvider` — valid T1.

Inspect `StorageProviderFactory` for KitchenClash types; strip or inject via interfaces before move.

- [ ] **Step 3: Cutover registrations**

```csharp
builder.Register<Playcenter.Persistence.EOSCloudStorageProvider>(...) // NO — cloud stays EOS module
builder.Register<Playcenter.Persistence.LocalStorageProvider>(Lifetime.Singleton).As<IStorageProvider>(); // only if currently registered that way
builder.Register<Playcenter.Persistence.StorageProviderFactory>(Lifetime.Singleton);
```

Match **existing** RootLifetimeScope patterns (today factory is concrete; cloud is EOS). Do not change behavior.

- [ ] **Step 4: If Outcome A only — genericize SaveService**

Split interface:

```csharp
// Playcenter.Services or Persistence
public interface ISaveOrchestrator
{
    void RegisterStorageConfig(string key, StorageStrategy strategy, bool encrypt);
    void OnUserLoggedIn();
    void OnUserLoggedOut();
    Task SyncAllCloudDataAsync();
    T LoadData<T>(string key) where T : class, new();
    void SaveData<T>(string key, T data) where T : class, new();
    T Load<T>(string key, T defaultValue);
    void Save(string key, object data);
}
```

Game `ISaveService` extends or wraps orchestrator + `GameSettingsData` methods. **If this split exceeds ~1 hour or breaks many call sites, fall back to Outcome B.**

- [ ] **Step 5: Build**

```bash
dotnet build RecipeRage.Composition.csproj -nologo
dotnet test RecipeRage.Tests.EditMode.csproj -nologo
```

```bash
rg -n "KitchenClash" Assets/Playcenter/Persistence --glob '*.cs'
```

Expected: no matches.

- [ ] **Step 6: Commit**

```bash
git add Assets/Playcenter/Persistence/ Assets/_KitchenClash/Infrastructure/Persistence/ \
  Assets/_KitchenClash/Composition/RootLifetimeScope.cs
git commit -m "$(cat <<'EOF'
feat(playcenter.persistence): extract storage providers

Persistence gate Outcome B (or A): shared local storage + factory.
Title SaveService/DTOs remain game-side when gated.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 7: Playcenter.Networking — NetworkObjectPool only

**Files:**
- Create: `Assets/Playcenter/Networking/Runtime/Playcenter.Networking.asmdef`
- Create (move): `INetworkObjectPool.cs`, `NetworkObjectPool.cs`
- Create: `Assets/Playcenter/Networking/README.md`
- Modify: consumers (IngredientNetworkSpawner, Root/Match scopes, etc.)
- Delete: originals under `Assets/_KitchenClash/Infrastructure/Network/` for those two files only
- **Stay game:** `NetworkGameManager`, transport configurators, spawn rules, player network manager

**Interfaces:**
- Consumes: `Playcenter.Shell` (GameLogger), Unity.Netcode, UnityEngine
- Produces: `Playcenter.Networking.INetworkObjectPool`, `NetworkObjectPool`

- [ ] **Step 1: Asmdef**

```json
{
    "name": "Playcenter.Networking",
    "rootNamespace": "Playcenter.Networking",
    "references": [
        "Playcenter.Shell",
        "Unity.Netcode.Runtime"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

Use the exact NGO asmdef name already referenced by `KitchenClash.Infrastructure` network asmdef.

- [ ] **Step 2: Move pool types**

Namespace `Playcenter.Networking`.

Remove unused `using KitchenClash.Domain;` from `NetworkObjectPool.cs` (currently present but pool logic does not need Domain — verify after move; if a Domain symbol is used, replace with local logic or reject the dependency — **must not** reference KitchenClash).

Surface unchanged:

```csharp
public interface INetworkObjectPool
{
    NetworkObject Get(GameObject prefab, Vector3 position, Quaternion rotation);
    void Return(NetworkObject networkObject);
    void Prewarm(GameObject prefab, int count);
    void Clear();
}
```

- [ ] **Step 3: Consumer cutover**

```bash
rg -n "INetworkObjectPool|NetworkObjectPool" --glob '*.cs' Assets/_KitchenClash -g '!Library/**'
```

Update usings + Infrastructure Network asmdef reference to `Playcenter.Networking`.

- [ ] **Step 4: Build**

```bash
dotnet build RecipeRage.Composition.csproj -nologo
dotnet test RecipeRage.Tests.EditMode.csproj -nologo
```

```bash
rg -n "KitchenClash" Assets/Playcenter/Networking --glob '*.cs'
```

Expected: no matches.

- [ ] **Step 5: Commit**

```bash
git add Assets/Playcenter/Networking/ Assets/_KitchenClash/Infrastructure/Network/ \
  Assets/_KitchenClash/Composition/
git commit -m "$(cat <<'EOF'
feat(playcenter.networking): extract NetworkObjectPool

Shared NGO object pool only; match spawn and NetworkGameManager stay game.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 8: Docs, wiki, design status, supersession

**Files:**
- Modify: `docs/superpowers/specs/2026-07-16-playcenter-shared-stack-design.md` — Status → **Approved / Implemented** (or **Approved** if mid-flight; final **Implemented** when all tasks done)
- Modify: `wiki/Technical.md` — Playcenter module map + DAG
- Modify: `wiki/log.md` — append entry
- Modify: each new module README if any gap remains
- Modify: `docs/superpowers/specs/2026-07-15-playcenter-foundation-extract-design.md` — note superseded for implementation modules
- Modify: session `plan.md` only if using conductor tracking (optional)

- [ ] **Step 1: Update design status header**

```markdown
**Status:** Approved (implementation plan: `docs/superpowers/plans/2026-07-16-playcenter-shared-stack.md`)
```

When all tasks complete, set:

```markdown
**Status:** Implemented on branch `architecture-cleanup`
```

- [ ] **Step 2: wiki/Technical.md section**

Add a section:

```markdown
## Playcenter shared stack (Approach C)

### Pure (Tier 0)
- Shell, GameFlow, Services (ports + session/social), UI (ports + UIScreenStackManager)

### Unity-thin (Tier 1)
- UI.Toolkit — screen host (no VContainer in public API)
- Animation — DOTween + UniTask
- EOS — AuthenticationService, EOSCloudStorageProvider, EosResultMapper
- Persistence — LocalStorageProvider / StorageProviderFactory (SaveService game if gated)
- Networking — NetworkObjectPool only

### DAG
Only Shell is shared hub. GameFlow ⟂ Services ⟂ UI. Toolkit → Shell+UI+Animation.
EOS → Shell+Services. Persistence → Shell+Services. Networking → Shell+NGO.
No Playcenter → KitchenClash.
```

Document Persistence gate outcome explicitly.

- [ ] **Step 3: wiki/log.md**

Append dated note: shared-stack Wave 1+2 High implemented; link plan + design.

- [ ] **Step 4: Final verification**

```bash
# No dual legacy names
rg -n "KitchenClash\.Presentation\.Common\.(UIService|BaseUIScreen|UIScreenStackManager)" --glob '*.cs' -g '!docs/**' -g '!Library/**'
rg -n "KitchenClash\.Infrastructure\.Animation" --glob '*.cs' -g '!docs/**' -g '!Library/**'
rg -n "EOSAuthService" --glob '*.cs' -g '!docs/**' -g '!Library/**'

# DAG spot-check: forbidden refs
rg -n "Playcenter\.(Services|GameFlow|EOS)" Assets/Playcenter/UI/Runtime --glob '*.cs'
rg -n "Playcenter\.(UI|GameFlow|EOS)" Assets/Playcenter/Animation --glob '*.cs'
rg -n "KitchenClash" Assets/Playcenter --glob '*.cs'

dotnet build RecipeRage.Composition.csproj -nologo
dotnet test RecipeRage.Tests.EditMode.csproj -nologo
```

Expected: no forbidden hits; builds/tests green.

- [ ] **Step 5: Commit**

```bash
git add docs/superpowers/specs/ wiki/ Assets/Playcenter/**/README.md
git commit -m "$(cat <<'EOF'
docs(playcenter): shared-stack modules and DAG

Record Approach C module map, Persistence gate outcome, and supersession notes.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

## Self-review (plan vs spec)

### 1. Spec coverage

| Spec item | Task |
|-----------|------|
| §5.2 UI.Toolkit host + factory | Task 3 |
| §5.2 pure stack in Playcenter.UI | Task 1 |
| §5.3 Animation + UniTask lock | Task 2 |
| §5.4 EOS auth/storage/mapper; delete EOSAuthService; lobby/MM stay game | Task 4 |
| §5.5.1 session/social ports + model coupling resolution | Task 5 (promote all four ports; models genericized) |
| §5.5.2 Persistence DTO gate | Task 6 |
| §5.5.3 NetworkObjectPool | Task 7 |
| §7 hard cutover | Every task delete step |
| §8 phase order | Tasks 1→8 |
| §9 testing | Task 1 tests; builds/EditMode each phase |
| §10 GameLogger / fail-closed | Inherited; no Console fallback introduced |
| §11 success criteria | Task 8 final verification |
| §15 Med/Low deferred | Global Constraints — out of scope |

### 2. Placeholder scan

No TBD/TODO/“implement later”/“similar to Task N” steps remain. Gate Outcome A/B is an explicit binary with default B.

### 3. Type consistency

- Stack: `Playcenter.UI.IUIScreenStackManager` / `UIScreenStackManager` used in Tasks 1 and 3.
- Factory: `IScreenInstanceFactory.Create(Type)` + optional `IScopeAwareScreenFactory.SetScope(object)`.
- Animation: `Playcenter.Animation.IAnimationService` with UniTask surface.
- EOS: `IEOSConfig`, `IAuthLifecycleHooks`, generic `EosResultMapper`.
- Session models/ports all `Playcenter.Services`.
- Pool: `Playcenter.Networking.INetworkObjectPool`.

---

## Execution handoff

Plan complete and saved to `docs/superpowers/plans/2026-07-16-playcenter-shared-stack.md`.

**Two execution options:**

1. **Subagent-Driven (recommended)** — fresh subagent per task, review between tasks, fast iteration  
2. **Inline Execution** — execute tasks in this session with executing-plans and checkpoints  

**Which approach?**
