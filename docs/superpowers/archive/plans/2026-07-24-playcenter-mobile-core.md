# Playcenter.MobileCore Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build `Playcenter.MobileCore` — a reusable mobile multiplayer core (input, session lifecycle, bot framework, net glue) with a unified MonoBehaviour bootstrap — and cut RecipeRage over to it.

**Architecture:** Single asmdef (`Playcenter.MobileCore`) with a folder firewall: `Core/` is engine-free (CI grep gate), `Adapters/` is the only vendor zone, `Bootstrap/` holds the one MonoBehaviour entry point. RecipeRage adopts each subsystem via hard cutover (old code deleted in the same commit), per spec `docs/superpowers/specs/2026-07-24-playcenter-mobile-core-design.md`.

**Tech Stack:** Unity 6000.3, C# 9 (`LangVersion` from generated csproj), NUnit (EditMode), VContainer (game-side only), Playcenter.SDK/Services/Shell, Unity InputSystem.

## Global Constraints

- `Core/` must NEVER contain usings matching `UnityEngine|VContainer|Unity.Netcode|Epic|Firebase|Cysharp` (CI gate, Task 1).
- Async in Core: `System.Threading.Tasks.Task` only — no UniTask in the module (UniTask conversion happens at game seams via `.AsUniTask()`).
- Namespace root: `Playcenter.MobileCore` (sub-namespaces mirror folders: `.Core.Input`, `.Adapters.Input`, …).
- Code style: 4-space indent, explicit accessibility, no `this.`, `readonly` fields, `var` only when obvious.
- Build verify: `dotnet build RecipeRage.Core.csproj -nologo` and `dotnet test RecipeRage.Tests.EditMode.csproj --no-build -nologo` must stay green after every task. NOTE: Unity regenerates csproj files when it imports new asmdefs — if `Playcenter.MobileCore.csproj` does not exist yet when a build command runs, open the project in Unity once (or run `dotnet build` on an existing project to confirm no regressions) and record this in the commit message.
- Commits: `type(scope): description` per repo convention; include the Co-authored-by trailer.
- Money-path tests only (spec §8): state machines and planners get tests; thin adapters, DTOs, bootstrap glue do not.
- No hardcoded tuning: option structs with defaults matching spec §9 RC-key defaults.

---

### Task 1: Module Skeleton + CI Firewall Gate

**Files:**
- Create: `Assets/Playcenter/MobileCore/Runtime/Playcenter.MobileCore.asmdef`
- Create: `Assets/Playcenter/MobileCore/Runtime/Bootstrap/PlaycenterBootstrap.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/MobileCoreContext.cs`
- Modify: `.github/workflows/*.yml` or `tools/ci/*.sh` (wherever the existing W5 vendor-firewall grep gate lives — search first: `grep -rn "vendor" .github/ tools/ 2>/dev/null | head`; if no CI gate infrastructure exists in-repo, create `tools/ci/grep-gates.sh` and document invocation in commit message)

**Interfaces:**
- Consumes: `Playcenter.SDK` (`PlaycenterClient`, `ClientOptions`, `IServiceRegistry`, `IPlaycenterServices`)
- Produces: `PlaycenterBootstrap` (singleton MonoBehaviour), `MobileCoreContext` (empty shell — populated by Tasks 2–6)

- [ ] **Step 1: Locate the existing grep gate**

```bash
grep -rn "Epic\|vendor" .github/workflows/ tools/ 2>/dev/null | head -20
```

Expected: find where the SDK W5 gate is implemented, or confirm none exists.

- [ ] **Step 2: Create the asmdef**

```json
{
    "name": "Playcenter.MobileCore",
    "rootNamespace": "Playcenter.MobileCore",
    "references": [
        "Playcenter.SDK",
        "Playcenter.Services",
        "Playcenter.Shell",
        "Unity.InputSystem"
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

- [ ] **Step 3: Create the bootstrap shell**

```csharp
using Playcenter.SDK;
using UnityEngine;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Sole scene entry point for the Playcenter stack. One prefab per title.
    /// Owns the SDK client and the MobileCore context; ticks all core systems.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class PlaycenterBootstrap : MonoBehaviour
    {
        public static PlaycenterBootstrap Instance { get; private set; }

        public IPlaycenterServices Services { get; private set; }
        public MobileCoreContext Core { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Core = new MobileCoreContext();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
```

- [ ] **Step 4: Create the context shell**

```csharp
namespace Playcenter.MobileCore
{
    /// <summary>
    /// Facade over all MobileCore subsystems. Constructed by PlaycenterBootstrap;
    /// populated during boot (clock first, then input/session/bots/net).
    /// </summary>
    public sealed class MobileCoreContext
    {
    }
}
```

- [ ] **Step 5: Add the CI grep gate**

Append to the existing gate script (or create `tools/ci/grep-gates.sh`):

```bash
#!/usr/bin/env bash
# Playcenter vendor-firewall gates. Exits non-zero on violation.
set -euo pipefail

CORE_DIR="Assets/Playcenter/MobileCore/Runtime/Core"
PATTERN='using (UnityEngine|VContainer|Unity\.Netcode|Epic|Firebase|Cysharp)'

if grep -rnE "$PATTERN" "$CORE_DIR" --include='*.cs'; then
    echo "GATE FAIL: vendor using found under $CORE_DIR" >&2
    exit 1
fi

echo "GATE PASS: $CORE_DIR is vendor-free"
```

```bash
chmod +x tools/ci/grep-gates.sh && tools/ci/grep-gates.sh
```

Expected: `GATE PASS` (Core/ has no files with usings yet).

- [ ] **Step 6: Verify build and commit**

```bash
dotnet build RecipeRage.Core.csproj -nologo
git add Assets/Playcenter/MobileCore tools/ci
git commit -m "feat(sdk): Playcenter.MobileCore skeleton — asmdef, bootstrap shell, vendor firewall gate

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 2: Game Clock (Core + Adapter)

**Files:**
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Clock/IGameClock.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Clock/ManualClock.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Adapters/Clock/UnityGameClock.cs`
- Create: `Assets/Scripts/Tests/EditMode/Playcenter/MobileCore/ManualClockTests.cs`
- Modify: `Assets/Playcenter/MobileCore/Runtime/Bootstrap/PlaycenterBootstrap.cs` (tick the clock in `Update`)

**Interfaces:**
- Consumes: nothing
- Produces: `IGameClock { float DeltaTime { get; } float Elapsed { get; } event Action<float> Ticked; void Tick(float deltaTime); }` — used by every later task. `ManualClock` for tests; `UnityGameClock` for runtime.

- [ ] **Step 1: Write the failing test**

```csharp
using NUnit.Framework;
using Playcenter.MobileCore;

namespace RecipeRage.Tests.Playcenter.MobileCore
{
    public sealed class ManualClockTests
    {
        [Test]
        public void Tick_AccumulatesElapsed_AndFiresEvent()
        {
            var clock = new ManualClock();
            float observed = 0f;
            clock.Ticked += dt => observed = dt;

            clock.Tick(0.5f);
            clock.Tick(0.25f);

            Assert.AreEqual(0.75f, clock.Elapsed, 0.0001f);
            Assert.AreEqual(0.25f, clock.DeltaTime, 0.0001f);
            Assert.AreEqual(0.25f, observed, 0.0001f);
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

```bash
dotnet test RecipeRage.Tests.EditMode.csproj --filter="ManualClockTests" --no-build -nologo
```

Expected: FAIL — `ManualClock` does not exist.

- [ ] **Step 3: Implement clock types**

`IGameClock.cs`:

```csharp
using System;

namespace Playcenter.MobileCore
{
    /// <summary>Single time source for all Core logic. No Time./DateTime. in Core.</summary>
    public interface IGameClock
    {
        float DeltaTime { get; }
        float Elapsed { get; }
        event Action<float> Ticked;
        void Tick(float deltaTime);
    }
}
```

`ManualClock.cs`:

```csharp
using System;

namespace Playcenter.MobileCore
{
    /// <summary>Test/headless clock — caller drives ticks explicitly.</summary>
    public sealed class ManualClock : IGameClock
    {
        public float DeltaTime { get; private set; }
        public float Elapsed { get; private set; }
        public event Action<float> Ticked;

        public void Tick(float deltaTime)
        {
            DeltaTime = deltaTime;
            Elapsed += deltaTime;
            Ticked?.Invoke(deltaTime);
        }
    }
}
```

`UnityGameClock.cs`:

```csharp
using System;
using UnityEngine;

namespace Playcenter.MobileCore
{
    /// <summary>Runtime clock — ticked by PlaycenterBootstrap.Update with Time.deltaTime.</summary>
    public sealed class UnityGameClock : IGameClock
    {
        public float DeltaTime { get; private set; }
        public float Elapsed { get; private set; }
        public event Action<float> Ticked;

        public void Tick(float deltaTime)
        {
            DeltaTime = deltaTime;
            Elapsed += deltaTime;
            Ticked?.Invoke(deltaTime);
        }
    }
}
```

- [ ] **Step 4: Wire clock into bootstrap and context**

In `MobileCoreContext`:

```csharp
namespace Playcenter.MobileCore
{
    public sealed class MobileCoreContext
    {
        public IGameClock Clock { get; set; }
    }
}
```

In `PlaycenterBootstrap.Awake`, after `Core = new MobileCoreContext();`:

```csharp
            _clock = new UnityGameClock();
            Core.Clock = _clock;
```

Add field + Update:

```csharp
        private UnityGameClock _clock;

        private void Update()
        {
            _clock?.Tick(Time.deltaTime);
        }
```

- [ ] **Step 5: Run tests, gate, commit**

```bash
dotnet test RecipeRage.Tests.EditMode.csproj --filter="ManualClockTests" --no-build -nologo
tools/ci/grep-gates.sh
git add Assets/Playcenter/MobileCore Assets/Scripts/Tests/EditMode/Playcenter/MobileCore
git commit -m "feat(sdk): MobileCore game clock — IGameClock, ManualClock, UnityGameClock, bootstrap tick

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

Expected: test PASS, `GATE PASS`.

---

### Task 3: Input Core — Frames, Gestures, Dual-Stick Model

**Files:**
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Input/InputAxis.cs` (alias usage — see below)
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Input/InputButtons.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Input/InputFrame.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Input/PointerEvent.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Input/DualStickConfig.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Input/TapGestureDetector.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Input/DualStickModel.cs`
- Create: `Assets/Scripts/Tests/EditMode/Playcenter/MobileCore/TapGestureDetectorTests.cs`
- Create: `Assets/Scripts/Tests/EditMode/Playcenter/MobileCore/DualStickModelTests.cs`

**Interfaces:**
- Consumes: `IGameClock` (Task 2), `Playcenter.Services.InputAxis2` (existing — reused, NOT redefined)
- Produces: `InputFrame`, `InputButtons`, `PointerEvent`, `DualStickConfig`, `TapGestureDetector`, `DualStickModel` — consumed by Task 4 (adapter) and RecipeRage cutover (Task 8)

- [ ] **Step 1: Write failing tests**

`TapGestureDetectorTests.cs`:

```csharp
using NUnit.Framework;
using Playcenter.MobileCore;

namespace RecipeRage.Tests.Playcenter.MobileCore
{
    public sealed class TapGestureDetectorTests
    {
        [Test]
        public void Taps_WithinWindow_AccumulateCount()
        {
            var clock = new ManualClock();
            var detector = new TapGestureDetector(windowSeconds: 0.3f, idleResetSeconds: 0.5f, clock);

            detector.OnTap();
            clock.Tick(0.1f);
            detector.OnTap();
            clock.Tick(0.1f);
            detector.OnTap();

            Assert.AreEqual(3, detector.TapCount);
        }

        [Test]
        public void Idle_BeyondReset_ClearsCount()
        {
            var clock = new ManualClock();
            var detector = new TapGestureDetector(0.3f, 0.5f, clock);

            detector.OnTap();
            detector.OnTap();
            clock.Tick(0.6f);

            Assert.AreEqual(0, detector.TapCount);
        }
    }
}
```

`DualStickModelTests.cs`:

```csharp
using NUnit.Framework;
using Playcenter.MobileCore;
using Playcenter.Services;

namespace RecipeRage.Tests.Playcenter.MobileCore
{
    public sealed class DualStickModelTests
    {
        private static DualStickModel CreateModel(ManualClock clock)
        {
            return new DualStickModel(new DualStickConfig(deadzone: 0.15f), clock);
        }

        [Test]
        public void Move_BelowDeadzone_ReturnsZero()
        {
            var clock = new ManualClock();
            var model = CreateModel(clock);
            float halfW = 400f, halfH = 400f;

            // small deflection on left half (move stick): ~5% of half-width
            model.OnPointer(new PointerEvent(1, halfW * 0.5f + 0.05f * halfW, halfH, PointerPhase.Moved, halfW, halfH));
            InputFrame frame = model.Tick();

            Assert.AreEqual(0f, frame.Move.X, 0.0001f);
            Assert.AreEqual(0f, frame.Move.Y, 0.0001f);
        }

        [Test]
        public void Tick_IncrementsSequenceNumber()
        {
            var clock = new ManualClock();
            var model = CreateModel(clock);

            InputFrame first = model.Tick();
            InputFrame second = model.Tick();

            Assert.AreEqual(first.SequenceNumber + 1u, second.SequenceNumber);
        }

        [Test]
        public void AimRelease_RaisesFlag_ForExactlyOneTick()
        {
            var clock = new ManualClock();
            var model = CreateModel(clock);
            float halfW = 400f, halfH = 400f;

            // press on right half (aim stick), then release
            model.OnPointer(new PointerEvent(2, halfW * 1.5f, halfH, PointerPhase.Began, halfW, halfH));
            model.Tick();
            model.OnPointer(new PointerEvent(2, halfW * 1.5f, halfH, PointerPhase.Ended, halfW, halfH));

            InputFrame releaseFrame = model.Tick();
            InputFrame nextFrame = model.Tick();

            Assert.IsTrue((releaseFrame.Buttons & InputButtons.AimReleased) != 0);
            Assert.IsTrue((nextFrame.Buttons & InputButtons.AimReleased) == 0);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test RecipeRage.Tests.EditMode.csproj --filter="TapGestureDetectorTests|DualStickModelTests" --no-build -nologo
```

Expected: FAIL — types do not exist.

- [ ] **Step 3: Implement input core**

`InputButtons.cs`:

```csharp
using System;

namespace Playcenter.MobileCore
{
    [Flags]
    public enum InputButtons : byte
    {
        None = 0,
        Interact = 1 << 0,
        Ability = 1 << 1,
        Super = 1 << 2,
        Gadget = 1 << 3,
        ChopTap = 1 << 4,
        AimReleased = 1 << 5,
    }
}
```

`InputFrame.cs`:

```csharp
using Playcenter.Services;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Versioned wire-ready input snapshot. Bump Version on any layout change.
    /// Uses InputAxis2 (Playcenter.Services) — no UnityEngine types.
    /// </summary>
    public readonly struct InputFrame
    {
        public const byte CurrentVersion = 1;

        public byte Version => CurrentVersion;
        public uint SequenceNumber { get; }
        public float DeltaTime { get; }
        public InputAxis2 Move { get; }
        public InputAxis2 Aim { get; }
        public InputButtons Buttons { get; }

        public InputFrame(uint sequenceNumber, float deltaTime, InputAxis2 move, InputAxis2 aim, InputButtons buttons)
        {
            SequenceNumber = sequenceNumber;
            DeltaTime = deltaTime;
            Move = move;
            Aim = aim;
            Buttons = buttons;
        }
    }
}
```

`PointerEvent.cs`:

```csharp
namespace Playcenter.MobileCore
{
    public enum PointerPhase : byte
    {
        Began,
        Moved,
        Ended,
        Cancelled,
    }

    /// <summary>
    /// Raw pointer sample in screen pixels, normalized into stick space by DualStickModel.
    /// HalfWidth/HalfHeight carry the screen half-extents so the model stays resolution-aware
    /// without touching UnityEngine.Screen.
    /// </summary>
    public readonly struct PointerEvent
    {
        public int PointerId { get; }
        public float X { get; }
        public float Y { get; }
        public PointerPhase Phase { get; }
        public float HalfWidth { get; }
        public float HalfHeight { get; }

        public PointerEvent(int pointerId, float x, float y, PointerPhase phase, float halfWidth, float halfHeight)
        {
            PointerId = pointerId;
            X = x;
            Y = y;
            Phase = phase;
            HalfWidth = halfWidth;
            HalfHeight = halfHeight;
        }
    }
}
```

`DualStickConfig.cs`:

```csharp
namespace Playcenter.MobileCore
{
    /// <summary>Tuning for dual-stick behavior. Fill from IConfigService (mc_input_* keys).</summary>
    public readonly struct DualStickConfig
    {
        public float Deadzone { get; }
        public float TapWindowSeconds { get; }
        public float TapIdleResetSeconds { get; }

        public DualStickConfig(
            float deadzone = 0.15f,
            float tapWindowSeconds = 0.3f,
            float tapIdleResetSeconds = 0.5f)
        {
            Deadzone = deadzone;
            TapWindowSeconds = tapWindowSeconds;
            TapIdleResetSeconds = tapIdleResetSeconds;
        }
    }
}
```

`TapGestureDetector.cs`:

```csharp
namespace Playcenter.MobileCore
{
    /// <summary>
    /// Multi-tap detector: taps within windowSeconds accumulate; idleResetSeconds
    /// of silence clears the count. Driven by IGameClock ticks (deterministic in tests).
    /// </summary>
    public sealed class TapGestureDetector
    {
        private readonly float _windowSeconds;
        private readonly float _idleResetSeconds;
        private float _sinceLastTap;

        public int TapCount { get; private set; }

        public TapGestureDetector(float windowSeconds, float idleResetSeconds, IGameClock clock)
        {
            _windowSeconds = windowSeconds;
            _idleResetSeconds = idleResetSeconds;
            clock.Ticked += OnTicked;
        }

        public void OnTap()
        {
            TapCount++;
            _sinceLastTap = 0f;
        }

        public void Reset()
        {
            TapCount = 0;
            _sinceLastTap = 0f;
        }

        private void OnTicked(float deltaTime)
        {
            if (TapCount == 0)
            {
                return;
            }

            _sinceLastTap += deltaTime;
            if (_sinceLastTap >= _idleResetSeconds)
            {
                Reset();
            }
        }
    }
}
```

`DualStickModel.cs`:

```csharp
using Playcenter.Services;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Engine-free dual-stick state machine. Left half of screen = move stick,
    /// right half = aim stick. Feed raw PointerEvents; read one InputFrame per Tick().
    /// Chop taps are right-side quick taps tracked by the embedded TapGestureDetector.
    /// </summary>
    public sealed class DualStickModel
    {
        private readonly DualStickConfig _config;
        private readonly IGameClock _clock;
        private readonly TapGestureDetector _chopTaps;

        private int _movePointerId = -1;
        private int _aimPointerId = -1;
        private InputAxis2 _move;
        private InputAxis2 _aim;
        private bool _aimReleasedPending;
        private uint _sequence;

        public DualStickModel(DualStickConfig config, IGameClock clock)
        {
            _config = config;
            _clock = clock;
            _chopTaps = new TapGestureDetector(config.TapWindowSeconds, config.TapIdleResetSeconds, clock);
        }

        public int ChopTapCount => _chopTaps.TapCount;
        public bool AimActive => _aimPointerId >= 0;

        public void OnPointer(in PointerEvent e)
        {
            bool isLeftSide = e.X < e.HalfWidth;

            switch (e.Phase)
            {
                case PointerPhase.Began:
                    if (isLeftSide && _movePointerId < 0)
                    {
                        _movePointerId = e.PointerId;
                    }
                    else if (!isLeftSide && _aimPointerId < 0)
                    {
                        _aimPointerId = e.PointerId;
                    }
                    break;

                case PointerPhase.Moved:
                case PointerPhase.Began | PointerPhase.Moved:
                    if (e.PointerId == _movePointerId)
                    {
                        _move = Normalize(e, isLeftSide);
                    }
                    else if (e.PointerId == _aimPointerId)
                    {
                        _aim = Normalize(e, isLeftSide);
                    }
                    break;

                case PointerPhase.Ended:
                case PointerPhase.Cancelled:
                    if (e.PointerId == _movePointerId)
                    {
                        _movePointerId = -1;
                        _move = InputAxis2.Zero;
                    }
                    else if (e.PointerId == _aimPointerId)
                    {
                        _aimPointerId = -1;
                        _aimReleasedPending = true;
                        _aim = InputAxis2.Zero;
                    }
                    break;
            }
        }

        /// <summary>Registers one chop tap (right-side quick tap, game decides what counts).</summary>
        public void RegisterChopTap()
        {
            _chopTaps.OnTap();
        }

        public InputFrame Tick()
        {
            InputButtons buttons = InputButtons.None;
            if (_aimReleasedPending)
            {
                buttons |= InputButtons.AimReleased;
                _aimReleasedPending = false;
            }

            return new InputFrame(
                sequenceNumber: _sequence++,
                deltaTime: _clock.DeltaTime,
                move: ApplyDeadzone(_move),
                aim: ApplyDeadzone(_aim),
                buttons: buttons);
        }

        private InputAxis2 Normalize(in PointerEvent e, bool isLeftSide)
        {
            float centerX = isLeftSide ? e.HalfWidth * 0.5f : e.HalfWidth * 1.5f;
            float nx = (e.X - centerX) / (e.HalfWidth * 0.5f);
            float ny = (e.Y - e.HalfHeight) / e.HalfHeight;
            return ClampToUnit(new InputAxis2(nx, ny));
        }

        private InputAxis2 ApplyDeadzone(InputAxis2 axis)
        {
            return axis.Magnitude < _config.Deadzone ? InputAxis2.Zero : axis;
        }

        private static InputAxis2 ClampToUnit(InputAxis2 axis)
        {
            return axis.SqrMagnitude > 1f ? axis.Normalized : axis;
        }
    }
}
```

NOTE on `PointerPhase.Began | PointerPhase.Moved`: flags-style case label is invalid for a non-flags enum — replace that case block by handling `Began` position-setting inside the `Began` branch: after assigning pointer ids, also set the axis via `Normalize(e, isLeftSide)` for the matching stick. Remove the `case PointerPhase.Began | PointerPhase.Moved:` line and in the `Began` branch, after each id assignment, set `_move = Normalize(e, isLeftSide);` or `_aim = Normalize(e, isLeftSide);` respectively.

- [ ] **Step 4: Run tests to verify they pass**

```bash
dotnet test RecipeRage.Tests.EditMode.csproj --filter="TapGestureDetectorTests|DualStickModelTests" --no-build -nologo
tools/ci/grep-gates.sh
```

Expected: 5 tests PASS, `GATE PASS`.

- [ ] **Step 5: Commit**

```bash
git add Assets/Playcenter/MobileCore Assets/Scripts/Tests/EditMode/Playcenter/MobileCore
git commit -m "feat(sdk): MobileCore input — InputFrame v1, dual-stick model, tap gesture detector

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 4: Input Adapter — Touch Provider

**Files:**
- Create: `Assets/Playcenter/MobileCore/Runtime/Adapters/Input/TouchDualStickProvider.cs`
- Modify: `Assets/Playcenter/MobileCore/Runtime/Core/MobileCoreContext.cs` (expose `Input` hub)
- Modify: `Assets/Playcenter/MobileCore/Runtime/Bootstrap/PlaycenterBootstrap.cs` (wire provider → model, tick model per frame)

**Interfaces:**
- Consumes: `DualStickModel`, `DualStickConfig`, `PointerEvent`, `PointerPhase` (Task 3); `UnityEngine.InputSystem` (`Touchscreen`, `Mouse`)
- Produces: `TouchDualStickProvider` (Update-driven event pump), `MobileCoreContext.Input` (`DualStickModel`)

- [ ] **Step 1: Implement the provider** (adapter — no tests per spec §8)

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Pumps Unity InputSystem touch/mouse samples into a DualStickModel as PointerEvents.
    /// The ONLY Unity-coupled input type in the module.
    /// </summary>
    public sealed class TouchDualStickProvider
    {
        private readonly DualStickModel _model;

        public TouchDualStickProvider(DualStickModel model)
        {
            _model = model;
        }

        public void Pump()
        {
            float halfW = Screen.width * 0.5f;
            float halfH = Screen.height * 0.5f;

            Touchscreen ts = Touchscreen.current;
            if (ts != null)
            {
                for (int i = 0; i < ts.touches.Count; i++)
                {
                    var touch = ts.touches[i];
                    if (!touch.isInProgress && touch.phase != UnityEngine.InputSystem.TouchPhase.Ended)
                    {
                        continue;
                    }

                    _model.OnPointer(new PointerEvent(
                        touch.touchId.ReadValue(),
                        touch.position.ReadValue().x,
                        touch.position.ReadValue().y,
                        MapPhase(touch.phase),
                        halfW,
                        halfH));
                }
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                bool pressed = mouse.leftButton.isPressed;
                Vector2 pos = mouse.position.ReadValue();
                _model.OnPointer(new PointerEvent(
                    0,
                    pos.x,
                    pos.y,
                    pressed ? PointerPhase.Moved : PointerPhase.Ended,
                    halfW,
                    halfH));
            }
        }

        private static PointerPhase MapPhase(UnityEngine.InputSystem.TouchPhase phase)
        {
            return phase switch
            {
                UnityEngine.InputSystem.TouchPhase.Began => PointerPhase.Began,
                UnityEngine.InputSystem.TouchPhase.Moved => PointerPhase.Moved,
                UnityEngine.InputSystem.TouchPhase.Stationary => PointerPhase.Moved,
                UnityEngine.InputSystem.TouchPhase.Ended => PointerPhase.Ended,
                _ => PointerPhase.Cancelled,
            };
        }
    }
}
```

- [ ] **Step 2: Expose input hub on context**

In `MobileCoreContext`:

```csharp
namespace Playcenter.MobileCore
{
    public sealed class MobileCoreContext
    {
        public IGameClock Clock { get; set; }
        public DualStickModel Input { get; set; }
        public InputFrame LatestFrame { get; internal set; }
    }
}
```

- [ ] **Step 3: Wire in bootstrap**

Add fields to `PlaycenterBootstrap`:

```csharp
        [SerializeField] private float _inputDeadzone = 0.15f;

        private TouchDualStickProvider _inputProvider;
```

In `Awake`, after clock creation:

```csharp
            Core.Input = new DualStickModel(new DualStickConfig(_inputDeadzone), _clock);
            _inputProvider = new TouchDualStickProvider(Core.Input);
```

In `Update`, after clock tick:

```csharp
            _inputProvider?.Pump();
            if (Core.Input != null)
            {
                Core.LatestFrame = Core.Input.Tick();
            }
```

- [ ] **Step 4: Verify build, gate, tests, commit**

```bash
dotnet build RecipeRage.Core.csproj -nologo
tools/ci/grep-gates.sh
dotnet test RecipeRage.Tests.EditMode.csproj --no-build -nologo
git add Assets/Playcenter/MobileCore
git commit -m "feat(sdk): MobileCore touch input provider — InputSystem pump into DualStickModel

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

Expected: build green, gate pass, full suite green.

---

### Task 5: Session Lifecycle Core

**Files:**
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Session/SessionState.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Session/ISessionScopeInstaller.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Session/ISessionScopeHandle.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Session/ISessionScopeFactory.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Session/SessionLifecycleController.cs`
- Create: `Assets/Scripts/Tests/EditMode/Playcenter/MobileCore/SessionLifecycleControllerTests.cs`

**Interfaces:**
- Consumes: `IPlaycenterServices` (Playcenter.SDK) for the handle's service surface
- Produces: `SessionLifecycleController { SessionState State; Task CreateAsync(); Task TeardownAsync(); event Action<SessionState, SessionState> Transitioned; }`, `ISessionScopeFactory`, `ISessionScopeHandle`, `ISessionScopeInstaller` — consumed by Task 6 cutover and game Composition

NOTE: this module's `ISessionScopeInstaller` is container-neutral (`void Install(object builder)` is wrong — see implementation: it uses a generic `ISessionContainerBuilder` port defined below so Core stays VContainer-free).

- [ ] **Step 1: Write the failing test**

```csharp
using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Playcenter.MobileCore;

namespace RecipeRage.Tests.Playcenter.MobileCore
{
    public sealed class SessionLifecycleControllerTests
    {
        private sealed class FakeHandle : ISessionScopeHandle
        {
            public bool Disposed { get; private set; }
            public void Dispose() => Disposed = true;
        }

        private sealed class FakeFactory : ISessionScopeFactory
        {
            public FakeHandle Handle { get; } = new FakeHandle();
            public bool InstallerSeen { get; private set; }

            public ISessionScopeHandle Create(ISessionScopeInstaller installer)
            {
                InstallerSeen = installer != null;
                return Handle;
            }
        }

        private sealed class FakeInstaller : ISessionScopeInstaller
        {
            public void Install(ISessionContainerBuilder builder) { }
        }

        [Test]
        public void CreateAsync_TransitionsNoneToActive()
        {
            var factory = new FakeFactory();
            var controller = new SessionLifecycleController(factory, new FakeInstaller());

            Task t = controller.CreateAsync();

            Assert.IsTrue(t.IsCompletedSuccessfully);
            Assert.AreEqual(SessionState.Active, controller.State);
            Assert.IsTrue(factory.InstallerSeen);
        }

        [Test]
        public void CreateAsync_WithoutInstaller_Throws()
        {
            var controller = new SessionLifecycleController(new FakeFactory(), null);

            Assert.ThrowsAsync<InvalidOperationException>(async () => await controller.CreateAsync());
            Assert.AreEqual(SessionState.None, controller.State);
        }

        [Test]
        public void CreateAsync_WhenActive_Throws()
        {
            var controller = new SessionLifecycleController(new FakeFactory(), new FakeInstaller());
            controller.CreateAsync().Wait();

            Assert.ThrowsAsync<InvalidOperationException>(async () => await controller.CreateAsync());
        }

        [Test]
        public async Task TeardownAsync_DisposesScope_ReturnsToNone()
        {
            var factory = new FakeFactory();
            var controller = new SessionLifecycleController(factory, new FakeInstaller());
            await controller.CreateAsync();

            await controller.TeardownAsync();

            Assert.AreEqual(SessionState.None, controller.State);
            Assert.IsTrue(factory.Handle.Disposed);
        }

        [Test]
        public void TeardownAsync_WhenNone_Throws()
        {
            var controller = new SessionLifecycleController(new FakeFactory(), new FakeInstaller());

            Assert.ThrowsAsync<InvalidOperationException>(async () => await controller.TeardownAsync());
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

```bash
dotnet test RecipeRage.Tests.EditMode.csproj --filter="SessionLifecycleControllerTests" --no-build -nologo
```

Expected: FAIL — types do not exist.

- [ ] **Step 3: Implement session core**

`SessionState.cs`:

```csharp
namespace Playcenter.MobileCore
{
    public enum SessionState
    {
        None,
        Creating,
        Active,
        TearingDown,
    }
}
```

`ISessionContainerBuilder.cs` (container-neutral builder port):

```csharp
namespace Playcenter.MobileCore
{
    /// <summary>
    /// Container-neutral registration surface for session installers. The game's
    /// ISessionScopeFactory implementation wraps its real container builder
    /// (RecipeRage: VContainer IContainerBuilder) behind this port.
    /// </summary>
    public interface ISessionContainerBuilder
    {
        void AddSingleton<TService>(TService instance) where TService : class;
        void AddSingleton<TService, TImpl>() where TService : class where TImpl : class, TService, new();
    }
}
```

`ISessionScopeInstaller.cs`:

```csharp
namespace Playcenter.MobileCore
{
    /// <summary>
    /// Installs session-scoped registrations. Law: every CreateAsync MUST run an
    /// installer — a bare scope has no services and fails at resolve time.
    /// </summary>
    public interface ISessionScopeInstaller
    {
        void Install(ISessionContainerBuilder builder);
    }
}
```

`ISessionScopeHandle.cs`:

```csharp
using System;

namespace Playcenter.MobileCore
{
    /// <summary>Live session scope. Dispose tears the scope down.</summary>
    public interface ISessionScopeHandle : IDisposable
    {
        T Get<T>() where T : class;
        bool TryGet<T>(out T service) where T : class;
    }
}
```

`ISessionScopeFactory.cs`:

```csharp
namespace Playcenter.MobileCore
{
    /// <summary>Implemented game-side (RecipeRage: wraps VContainer LifetimeScope child).</summary>
    public interface ISessionScopeFactory
    {
        ISessionScopeHandle Create(ISessionScopeInstaller installer);
    }
}
```

`SessionLifecycleController.cs`:

```csharp
using System;
using System.Threading.Tasks;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Fail-closed session FSM: None → Creating → Active → TearingDown → None.
    /// Illegal transitions throw. Mirrors AppFlowController's fail-closed discipline.
    /// </summary>
    public sealed class SessionLifecycleController
    {
        private readonly ISessionScopeFactory _factory;
        private readonly ISessionScopeInstaller _installer;

        private ISessionScopeHandle _scope;

        public SessionState State { get; private set; } = SessionState.None;
        public ISessionScopeHandle Scope => _scope;
        public event Action<SessionState, SessionState> Transitioned;

        public SessionLifecycleController(ISessionScopeFactory factory, ISessionScopeInstaller installer)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _installer = installer;
        }

        public Task CreateAsync()
        {
            if (_installer == null)
            {
                throw new InvalidOperationException(
                    "ISessionScopeInstaller is required. Register a session installer before CreateAsync.");
            }

            if (State != SessionState.None)
            {
                throw new InvalidOperationException($"Cannot create session from state {State}.");
            }

            Transition(SessionState.Creating);
            _scope = _factory.Create(_installer);
            Transition(SessionState.Active);
            return Task.CompletedTask;
        }

        public Task TeardownAsync()
        {
            if (State != SessionState.Active)
            {
                throw new InvalidOperationException($"Cannot tear down session from state {State}.");
            }

            Transition(SessionState.TearingDown);
            _scope?.Dispose();
            _scope = null;
            Transition(SessionState.None);
            return Task.CompletedTask;
        }

        private void Transition(SessionState next)
        {
            SessionState previous = State;
            State = next;
            Transitioned?.Invoke(previous, next);
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

```bash
dotnet test RecipeRage.Tests.EditMode.csproj --filter="SessionLifecycleControllerTests" --no-build -nologo
tools/ci/grep-gates.sh
```

Expected: 5 tests PASS, `GATE PASS`.

- [ ] **Step 5: Commit**

```bash
git add Assets/Playcenter/MobileCore Assets/Scripts/Tests/EditMode/Playcenter/MobileCore
git commit -m "feat(sdk): MobileCore session lifecycle — fail-closed FSM, container-neutral scope seam

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 6: Bot Framework Core

**Files:**
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Bots/ITaskEvaluator.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Bots/TaskPlanner.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Bots/ClaimRegistry.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Bots/IBotBudget.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Bots/BotBudget.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Bots/BotBrain.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Bots/BotHost.cs`
- Create: `Assets/Scripts/Tests/EditMode/Playcenter/MobileCore/TaskPlannerTests.cs`
- Create: `Assets/Scripts/Tests/EditMode/Playcenter/MobileCore/ClaimRegistryTests.cs`

**Interfaces:**
- Consumes: `IGameClock` (Task 2)
- Produces:
  - `ITaskEvaluator<TSnapshot, TTask> { TTask Evaluate(TSnapshot snapshot); }`
  - `TaskPlanner<TSnapshot, TTask> { void Register(ITaskEvaluator<TSnapshot,TTask> e); TTask Plan(TSnapshot s); }` (first non-null wins; empty chain → `default`)
  - `ClaimRegistry<TKey> { bool TryClaim(TKey key, string ownerId); bool Release(TKey key, string ownerId); bool IsClaimedByOther(TKey key, string ownerId); }`
  - `BotBrain<TSnapshot, TTask>` + `BotHost` — consumed by Task 9 cutover

- [ ] **Step 1: Write failing tests**

`TaskPlannerTests.cs`:

```csharp
using NUnit.Framework;
using Playcenter.MobileCore;

namespace RecipeRage.Tests.Playcenter.MobileCore
{
    public sealed class TaskPlannerTests
    {
        private sealed class Snapshot { public bool HasFire; public bool HasDelivery; }

        private sealed class FireEvaluator : ITaskEvaluator<Snapshot, string>
        {
            public string Evaluate(Snapshot s) => s.HasFire ? "extinguish" : null;
        }

        private sealed class DeliverEvaluator : ITaskEvaluator<Snapshot, string>
        {
            public string Evaluate(Snapshot s) => s.HasDelivery ? "deliver" : null;
        }

        [Test]
        public void Plan_FirstNonNullWins()
        {
            var planner = new TaskPlanner<Snapshot, string>();
            planner.Register(new DeliverEvaluator());
            planner.Register(new FireEvaluator());

            // registered deliver-first, but fire evaluator listed second → deliver wins
            Assert.AreEqual("deliver", planner.Plan(new Snapshot { HasFire = true, HasDelivery = true }));
        }

        [Test]
        public void Plan_NullPassesThrough_ToNextEvaluator()
        {
            var planner = new TaskPlanner<Snapshot, string>();
            planner.Register(new FireEvaluator());
            planner.Register(new DeliverEvaluator());

            Assert.AreEqual("deliver", planner.Plan(new Snapshot { HasFire = false, HasDelivery = true }));
        }

        [Test]
        public void Plan_EmptyChain_ReturnsDefault()
        {
            var planner = new TaskPlanner<Snapshot, string>();

            Assert.IsNull(planner.Plan(new Snapshot()));
        }
    }
}
```

`ClaimRegistryTests.cs`:

```csharp
using NUnit.Framework;
using Playcenter.MobileCore;

namespace RecipeRage.Tests.Playcenter.MobileCore
{
    public sealed class ClaimRegistryTests
    {
        [Test]
        public void TryClaim_FirstClaimSucceeds_SecondBotFails()
        {
            var registry = new ClaimRegistry<int>();

            Assert.IsTrue(registry.TryClaim(7, "bot-a"));
            Assert.IsFalse(registry.TryClaim(7, "bot-b"));
            Assert.IsTrue(registry.IsClaimedByOther(7, "bot-b"));
            Assert.IsFalse(registry.IsClaimedByOther(7, "bot-a"));
        }

        [Test]
        public void Release_FreesClaim_ForOtherBots()
        {
            var registry = new ClaimRegistry<int>();
            registry.TryClaim(7, "bot-a");

            Assert.IsTrue(registry.Release(7, "bot-a"));
            Assert.IsTrue(registry.TryClaim(7, "bot-b"));
        }

        [Test]
        public void Release_ByNonOwner_Fails()
        {
            var registry = new ClaimRegistry<int>();
            registry.TryClaim(7, "bot-a");

            Assert.IsFalse(registry.Release(7, "bot-b"));
        }

        [Test]
        public void TryClaim_EmptyOwner_Fails()
        {
            var registry = new ClaimRegistry<int>();

            Assert.IsFalse(registry.TryClaim(7, ""));
            Assert.IsFalse(registry.TryClaim(7, null));
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test RecipeRage.Tests.EditMode.csproj --filter="TaskPlannerTests|ClaimRegistryTests" --no-build -nologo
```

Expected: FAIL — types do not exist.

- [ ] **Step 3: Implement bot core**

`ITaskEvaluator.cs`:

```csharp
namespace Playcenter.MobileCore
{
    /// <summary>One link in the planner chain. Return null to pass to the next evaluator.</summary>
    public interface ITaskEvaluator<TSnapshot, TTask>
    {
        TTask Evaluate(TSnapshot snapshot);
    }
}
```

`TaskPlanner.cs`:

```csharp
using System.Collections.Generic;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Priority-chain planner: evaluators run in registration order, first non-null
    /// task wins. Games register domain evaluators (RecipeRage: fire → deliver → cook → prep).
    /// </summary>
    public sealed class TaskPlanner<TSnapshot, TTask>
    {
        private readonly List<ITaskEvaluator<TSnapshot, TTask>> _evaluators =
            new List<ITaskEvaluator<TSnapshot, TTask>>();

        public void Register(ITaskEvaluator<TSnapshot, TTask> evaluator)
        {
            _evaluators.Add(evaluator);
        }

        public TTask Plan(TSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return default;
            }

            for (int i = 0; i < _evaluators.Count; i++)
            {
                TTask task = _evaluators[i].Evaluate(snapshot);
                if (task != null)
                {
                    return task;
                }
            }

            return default;
        }
    }
}
```

`ClaimRegistry.cs`:

```csharp
using System.Collections.Generic;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Generic ownership registry: bots claim targets (stations, orders) so two bots
    /// never commit to the same one. Ported from KitchenClash BotClaimRegistry and generalized.
    /// </summary>
    public sealed class ClaimRegistry<TKey>
    {
        private readonly Dictionary<TKey, string> _claims = new Dictionary<TKey, string>();
        private readonly Dictionary<string, TKey> _ownerClaims = new Dictionary<string, TKey>();

        public bool TryClaim(TKey key, string ownerId)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return false;
            }

            if (_claims.TryGetValue(key, out string existing))
            {
                return existing == ownerId;
            }

            // one claim per owner: release previous before taking a new one
            if (_ownerClaims.TryGetValue(ownerId, out TKey previous))
            {
                _claims.Remove(previous);
            }

            _claims[key] = ownerId;
            _ownerClaims[ownerId] = key;
            return true;
        }

        public bool Release(TKey key, string ownerId)
        {
            if (!_claims.TryGetValue(key, out string existing) || existing != ownerId)
            {
                return false;
            }

            _claims.Remove(key);
            _ownerClaims.Remove(ownerId);
            return true;
        }

        public bool IsClaimedByOther(TKey key, string ownerId)
        {
            return _claims.TryGetValue(key, out string existing) && existing != ownerId;
        }
    }
}
```

`IBotBudget.cs` + `BotBudget.cs`:

```csharp
namespace Playcenter.MobileCore
{
    /// <summary>Per-tick CPU budget for bot planning. Never plan unbounded.</summary>
    public interface IBotBudget
    {
        bool TryConsume(float milliseconds);
        void ResetTick();
    }
}
```

```csharp
namespace Playcenter.MobileCore
{
    /// <summary>Time-sliced budget: up to maxMsPerTick of planning per clock tick (mc_bot_budget_ms, default 2).</summary>
    public sealed class BotBudget : IBotBudget
    {
        private readonly float _maxMsPerTick;
        private float _consumedMs;

        public BotBudget(float maxMsPerTick = 2f)
        {
            _maxMsPerTick = maxMsPerTick;
        }

        public bool TryConsume(float milliseconds)
        {
            if (_consumedMs + milliseconds > _maxMsPerTick)
            {
                return false;
            }

            _consumedMs += milliseconds;
            return true;
        }

        public void ResetTick()
        {
            _consumedMs = 0f;
        }
    }
}
```

`BotBrain.cs`:

```csharp
using System;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Per-bot decision loop: snapshot → plan → claim → task. Seeded Random keeps
    /// behavior deterministic per match seed. TTask chosen by the game.
    /// </summary>
    public sealed class BotBrain<TSnapshot, TTask>
    {
        private readonly string _botId;
        private readonly TaskPlanner<TSnapshot, TTask> _planner;
        private readonly Random _random;

        public TTask CurrentTask { get; private set; }
        public string BotId => _botId;
        public Random Random => _random;

        public BotBrain(string botId, TaskPlanner<TSnapshot, TTask> planner, int seed)
        {
            _botId = botId ?? throw new ArgumentNullException(nameof(botId));
            _planner = planner ?? throw new ArgumentNullException(nameof(planner));
            _random = new Random(seed);
        }

        public TTask Think(TSnapshot snapshot)
        {
            CurrentTask = _planner.Plan(snapshot);
            return CurrentTask;
        }
    }
}
```

`BotHost.cs`:

```csharp
using System;
using System.Collections.Generic;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Owns all bot brains; ticks them under an IBotBudget time-slice.
    /// Generic over the game's snapshot/task/intent pipeline; the game supplies
    /// snapshot + act callbacks so the host stays domain-free.
    /// </summary>
    public sealed class BotHost<TSnapshot, TTask>
    {
        private readonly List<BotBrain<TSnapshot, TTask>> _brains = new List<BotBrain<TSnapshot, TTask>>();
        private readonly IBotBudget _budget;
        private readonly IGameClock _clock;
        private readonly Func<TSnapshot> _snapshotSource;
        private readonly Action<BotBrain<TSnapshot, TTask>, TTask> _act;

        public IReadOnlyList<BotBrain<TSnapshot, TTask>> Brains => _brains;

        public BotHost(
            IBotBudget budget,
            IGameClock clock,
            Func<TSnapshot> snapshotSource,
            Action<BotBrain<TSnapshot, TTask>, TTask> act)
        {
            _budget = budget ?? throw new ArgumentNullException(nameof(budget));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _snapshotSource = snapshotSource ?? throw new ArgumentNullException(nameof(snapshotSource));
            _act = act ?? throw new ArgumentNullException(nameof(act));
            _clock.Ticked += OnTicked;
        }

        public void Add(BotBrain<TSnapshot, TTask> brain)
        {
            _brains.Add(brain);
        }

        private void OnTicked(float deltaTime)
        {
            _budget.ResetTick();
            TSnapshot snapshot = _snapshotSource();

            for (int i = 0; i < _brains.Count; i++)
            {
                if (!_budget.TryConsume(0.5f))
                {
                    break; // budget exhausted: remaining bots act next tick
                }

                TTask task = _brains[i].Think(snapshot);
                _act(_brains[i], task);
            }
        }
    }
}
```

NOTE: `TryConsume(0.5f)` uses a fixed per-think estimate. If finer accounting is needed later, wrap `Think` in a `Stopwatch` inside the game adapter and pass actual elapsed ms — keep the fixed estimate for v1 (YAGNI).

- [ ] **Step 4: Run tests to verify they pass**

```bash
dotnet test RecipeRage.Tests.EditMode.csproj --filter="TaskPlannerTests|ClaimRegistryTests" --no-build -nologo
tools/ci/grep-gates.sh
```

Expected: 7 tests PASS, `GATE PASS`.

- [ ] **Step 5: Commit**

```bash
git add Assets/Playcenter/MobileCore Assets/Scripts/Tests/EditMode/Playcenter/MobileCore
git commit -m "feat(sdk): MobileCore bot framework — task planner chain, claim registry, budgeted host

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 7: Net Glue Core — Reconnect FSM + Quality Tracker

**Files:**
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Net/ReconnectState.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Net/ReconnectConfig.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Net/BackoffPolicy.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Net/ReconnectStateMachine.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Net/ConnectionQuality.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Net/ConnectionQualityTracker.cs`
- Create: `Assets/Playcenter/MobileCore/Runtime/Core/Net/NetSessionOrchestrator.cs`
- Create: `Assets/Scripts/Tests/EditMode/Playcenter/MobileCore/ReconnectStateMachineTests.cs`
- Create: `Assets/Scripts/Tests/EditMode/Playcenter/MobileCore/BackoffPolicyTests.cs`

**Interfaces:**
- Consumes: `IGameClock` (Task 2), `Playcenter.Services.INetSession` / `NetRole` (existing)
- Produces: `ReconnectStateMachine`, `BackoffPolicy`, `ConnectionQualityTracker`, `NetSessionOrchestrator` — consumed by Task 10 cutover (`NetSessionConnectivityBridge` refactor)

- [ ] **Step 1: Write failing tests**

`ReconnectStateMachineTests.cs`:

```csharp
using NUnit.Framework;
using Playcenter.MobileCore;

namespace RecipeRage.Tests.Playcenter.MobileCore
{
    public sealed class ReconnectStateMachineTests
    {
        private static ReconnectConfig MatchConfig() => new ReconnectConfig(
            maxAttempts: 3,
            attemptIntervalSeconds: 5f,
            backoffBaseSeconds: 1f);

        [Test]
        public void MenuMode_RetriesIndefinitely()
        {
            var clock = new ManualClock();
            var sm = new ReconnectStateMachine(
                new ReconnectConfig(maxAttempts: 0, attemptIntervalSeconds: 3f, backoffBaseSeconds: 0f),
                clock,
                seed: 42);

            sm.OnDisconnected();
            for (int i = 0; i < 10; i++)
            {
                clock.Tick(3.1f);
            }

            Assert.AreEqual(ReconnectState.Reconnecting, sm.State);
            Assert.IsTrue(sm.AttemptCount >= 10);
        }

        [Test]
        public void MatchMode_FailsAfterMaxAttempts()
        {
            var clock = new ManualClock();
            var sm = new ReconnectStateMachine(MatchConfig(), clock, seed: 42);

            sm.OnDisconnected();
            for (int i = 0; i < 3; i++)
            {
                clock.Tick(5.1f);
            }
            clock.Tick(5.1f);

            Assert.AreEqual(ReconnectState.Failed, sm.State);
        }

        [Test]
        public void OnConnected_Recovers_ToConnected()
        {
            var clock = new ManualClock();
            var sm = new ReconnectStateMachine(MatchConfig(), clock, seed: 42);

            sm.OnDisconnected();
            clock.Tick(2f);
            sm.OnConnected();

            Assert.AreEqual(ReconnectState.Connected, sm.State);
            Assert.AreEqual(0, sm.AttemptCount);
        }
    }
}
```

`BackoffPolicyTests.cs`:

```csharp
using NUnit.Framework;
using Playcenter.MobileCore;

namespace RecipeRage.Tests.Playcenter.MobileCore
{
    public sealed class BackoffPolicyTests
    {
        [Test]
        public void Delay_GrowsExponentially_IsDeterministicPerSeed()
        {
            var a = new BackoffPolicy(baseSeconds: 1f, seed: 42);
            var b = new BackoffPolicy(baseSeconds: 1f, seed: 42);

            float a1 = a.NextDelay();
            float a2 = a.NextDelay();
            float b1 = b.NextDelay();
            float b2 = b.NextDelay();

            Assert.AreEqual(a1, b1, 0.0001f);
            Assert.AreEqual(a2, b2, 0.0001f);
            Assert.Greater(a2, 1.5f); // attempt 2 base = 2s, jitter ±25% keeps above 1.5
        }

        [Test]
        public void Reset_RestartsSequence()
        {
            var policy = new BackoffPolicy(1f, seed: 42);
            float first = policy.NextDelay();
            policy.NextDelay();

            policy.Reset();

            Assert.AreEqual(first, policy.NextDelay(), 0.0001f);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test RecipeRage.Tests.EditMode.csproj --filter="ReconnectStateMachineTests|BackoffPolicyTests" --no-build -nologo
```

Expected: FAIL — types do not exist.

- [ ] **Step 3: Implement net core**

`ReconnectState.cs`:

```csharp
namespace Playcenter.MobileCore
{
    public enum ReconnectState
    {
        Connected,
        Reconnecting,
        Failed,
    }
}
```

`ReconnectConfig.cs`:

```csharp
namespace Playcenter.MobileCore
{
    /// <summary>
    /// Reconnect tuning (mc_reconnect_*). maxAttempts 0 = retry forever (menu mode,
    /// wiki connectivity table: menu retries every 3s indefinitely; match = 3 × 5s then forfeit).
    /// </summary>
    public readonly struct ReconnectConfig
    {
        public int MaxAttempts { get; }
        public float AttemptIntervalSeconds { get; }
        public float BackoffBaseSeconds { get; }

        public ReconnectConfig(int maxAttempts, float attemptIntervalSeconds, float backoffBaseSeconds)
        {
            MaxAttempts = maxAttempts;
            AttemptIntervalSeconds = attemptIntervalSeconds;
            BackoffBaseSeconds = backoffBaseSeconds;
        }
    }
}
```

`BackoffPolicy.cs`:

```csharp
using System;

namespace Playcenter.MobileCore
{
    /// <summary>Exponential backoff with ±25% jitter. Seeded → deterministic in tests and replays.</summary>
    public sealed class BackoffPolicy
    {
        private readonly float _baseSeconds;
        private readonly Random _random;
        private int _attempt;

        public BackoffPolicy(float baseSeconds, int seed)
        {
            _baseSeconds = baseSeconds;
            _random = new Random(seed);
        }

        public float NextDelay()
        {
            float expo = _baseSeconds * (float)Math.Pow(2.0, _attempt);
            _attempt++;
            float jitter = 1f + ((float)_random.NextDouble() - 0.5f) * 0.5f;
            return expo * jitter;
        }

        public void Reset()
        {
            _attempt = 0;
        }
    }
}
```

`ReconnectStateMachine.cs`:

```csharp
using System;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Connected → Reconnecting → (recovered | Failed). Implements the wiki connectivity
    /// table; all timings from ReconnectConfig. Tick-driven via IGameClock.
    /// </summary>
    public sealed class ReconnectStateMachine
    {
        private readonly ReconnectConfig _config;
        private readonly BackoffPolicy _backoff;
        private float _sinceLastAttempt;

        public ReconnectState State { get; private set; } = ReconnectState.Connected;
        public int AttemptCount { get; private set; }
        public event Action ReconnectAttempted;
        public event Action ReconnectFailed;

        public ReconnectStateMachine(ReconnectConfig config, IGameClock clock, int seed)
        {
            _config = config;
            _backoff = new BackoffPolicy(config.BackoffBaseSeconds, seed);
            clock.Ticked += OnTicked;
        }

        public void OnDisconnected()
        {
            if (State == ReconnectState.Connected)
            {
                State = ReconnectState.Reconnecting;
                _sinceLastAttempt = _config.AttemptIntervalSeconds; // first attempt immediately
                AttemptCount = 0;
                _backoff.Reset();
            }
        }

        public void OnConnected()
        {
            State = ReconnectState.Connected;
            AttemptCount = 0;
            _sinceLastAttempt = 0f;
            _backoff.Reset();
        }

        private void OnTicked(float deltaTime)
        {
            if (State != ReconnectState.Reconnecting)
            {
                return;
            }

            _sinceLastAttempt += deltaTime;
            if (_sinceLastAttempt < _config.AttemptIntervalSeconds)
            {
                return;
            }

            _sinceLastAttempt = 0f;
            AttemptCount++;
            ReconnectAttempted?.Invoke();

            if (_config.MaxAttempts > 0 && AttemptCount >= _config.MaxAttempts)
            {
                State = ReconnectState.Failed;
                ReconnectFailed?.Invoke();
            }
        }
    }
}
```

NOTE: `BackoffPolicy` is injected into the state machine for future use (variable intervals); v1 uses fixed `AttemptIntervalSeconds` per the wiki table — the policy exists and is tested for titles that want backoff-based retries. YAGNI check: keep, because Task 10's orchestrator exposes it for the menu/match config split.

`ConnectionQuality.cs` + `ConnectionQualityTracker.cs`:

```csharp
namespace Playcenter.MobileCore
{
    public enum ConnectionQuality
    {
        Good,
        Degraded,
        Poor,
    }
}
```

```csharp
using System;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// RTT exponential moving average → quality tier for telemetry and UI badges.
    /// degradedThresholdMs / poorThresholdMs from config (mc_reconnect_*).
    /// </summary>
    public sealed class ConnectionQualityTracker
    {
        private readonly float _degradedMs;
        private readonly float _poorMs;
        private readonly float _smoothing;
        private float _emaMs = -1f;

        public ConnectionQuality Quality { get; private set; } = ConnectionQuality.Good;
        public float RttEmaMs => _emaMs;
        public event Action<ConnectionQuality> QualityChanged;

        public ConnectionQualityTracker(float degradedMs = 150f, float poorMs = 400f, float smoothing = 0.2f)
        {
            _degradedMs = degradedMs;
            _poorMs = poorMs;
            _smoothing = smoothing;
        }

        public void Sample(float rttMs)
        {
            _emaMs = _emaMs < 0f ? rttMs : _emaMs + _smoothing * (rttMs - _emaMs);

            ConnectionQuality next =
                _emaMs >= _poorMs ? ConnectionQuality.Poor :
                _emaMs >= _degradedMs ? ConnectionQuality.Degraded :
                ConnectionQuality.Good;

            if (next != Quality)
            {
                Quality = next;
                QualityChanged?.Invoke(next);
            }
        }
    }
}
```

`NetSessionOrchestrator.cs`:

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Playcenter.Services;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Wraps any INetSession with role validation and reconnect wiring. The sole
    /// start/stop path for net sessions in consuming games (wiki law).
    /// </summary>
    public sealed class NetSessionOrchestrator
    {
        private readonly INetSession _session;
        private readonly ReconnectStateMachine _reconnect;

        public ReconnectStateMachine Reconnect => _reconnect;
        public bool IsActive => _session.IsActive;

        public NetSessionOrchestrator(INetSession session, ReconnectStateMachine reconnect)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _reconnect = reconnect ?? throw new ArgumentNullException(nameof(reconnect));
        }

        public async Task StartAsync(NetRole role, string sessionToken, CancellationToken ct = default)
        {
            await _session.StartAsync(role, sessionToken, ct).ConfigureAwait(false);
            _reconnect.OnConnected();
        }

        public async Task StopAsync(CancellationToken ct = default)
        {
            await _session.StopAsync(ct).ConfigureAwait(false);
        }

        public void NotifyDisconnected()
        {
            _reconnect.OnDisconnected();
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

```bash
dotnet test RecipeRage.Tests.EditMode.csproj --filter="ReconnectStateMachineTests|BackoffPolicyTests" --no-build -nologo
tools/ci/grep-gates.sh
```

Expected: 5 tests PASS, `GATE PASS`.

- [ ] **Step 5: Commit**

```bash
git add Assets/Playcenter/MobileCore Assets/Scripts/Tests/EditMode/Playcenter/MobileCore
git commit -m "feat(sdk): MobileCore net glue — reconnect FSM, backoff, connection quality, orchestrator

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 8: RecipeRage Cutover — Input

**Files:**
- Create: `Assets/_KitchenClash/Infrastructure/Input/MobileCoreInputBridge.cs`
- Modify: `Assets/_KitchenClash/Composition/RootLifetimeScope.cs` (register bridge; construct `DualStickModel` with RC-backed `DualStickConfig`)
- Modify: `Assets/_KitchenClash/Infrastructure/Network/PlayerController.InputMovement.cs` (consume `InputFrame` from bridge instead of `IDualStickInput`)
- Modify: `Assets/_KitchenClash/Infrastructure/Network/PlayerInputData.cs` (add `Vector2 Aim`; keep `INetworkSerializable`)
- Delete: `Assets/_KitchenClash/Domain/Interfaces/IDualStickInput.cs`
- Delete: `Assets/_KitchenClash/Infrastructure/Input/GameplayInputMapper.cs`
- Delete: `Assets/_KitchenClash/Infrastructure/Input/TouchInputProvider.cs`
- Delete: `Assets/_KitchenClash/Infrastructure/Input/InputSystemProvider.cs`
- Delete: `Assets/_KitchenClash/Infrastructure/Input/InputProviderFactory.cs`
- Delete: `Assets/_KitchenClash/Infrastructure/Input/GameplayInputService.cs`

**Interfaces:**
- Consumes: `DualStickModel`, `InputFrame`, `InputButtons`, `DualStickConfig` (Tasks 3–4); `IConfigService` (`mc_input_*` keys)
- Produces: `MobileCoreInputBridge` (game-facing input seam; wraps `PlaycenterBootstrap.Instance.Core.Input` with a test-friendly fallback model)

- [ ] **Step 1: Run existing input-related tests to establish baseline**

```bash
dotnet test RecipeRage.Tests.EditMode.csproj --filter="Input" --no-build -nologo
```

Expected: PASS (record which tests exist; they must pass after cutover or be updated in Step 4).

- [ ] **Step 2: Create the bridge**

```csharp
using Playcenter.MobileCore;

namespace KitchenClash.Infrastructure.Input
{
    /// <summary>
    /// Game-facing seam over Playcenter.MobileCore input. Prefers the bootstrap's
    /// live model; falls back to a local model so EditMode tests and headless runs
    /// work without a scene bootstrap.
    /// </summary>
    public sealed class MobileCoreInputBridge
    {
        private readonly DualStickModel _fallback;
        private readonly ManualClock _fallbackClock;

        public MobileCoreInputBridge(DualStickConfig config)
        {
            _fallbackClock = new ManualClock();
            _fallback = new DualStickModel(config, _fallbackClock);
        }

        public DualStickModel Model =>
            PlaycenterBootstrap.Instance != null && PlaycenterBootstrap.Instance.Core.Input != null
                ? PlaycenterBootstrap.Instance.Core.Input
                : _fallback;

        public InputFrame LatestFrame =>
            PlaycenterBootstrap.Instance != null
                ? PlaycenterBootstrap.Instance.Core.LatestFrame
                : _fallback.Tick();
    }
}
```

- [ ] **Step 3: Rewire consumers**

In `PlayerController.InputMovement.cs`: replace every `IDualStickInput` read with the bridge:

- `_input.MoveInputX/_input.MoveInputY` → `frame.Move.X / frame.Move.Y`
- `_input.AimJustReleased` → `(frame.Buttons & InputButtons.AimReleased) != 0`
- `_input.AbilityPressed/SuperPressed/GadgetPressed` → corresponding `InputButtons` flags
- `_input.ChopTapped` / `ChopTapCount` → `bridge.Model.ChopTapCount` (count-driven; the game decides when a tap registers via `bridge.Model.RegisterChopTap()`)

In `PlayerInputData.cs`: add `public Vector2 Aim;` and serialize it after `Movement`:

```csharp
serializer.SerializeValue(ref Movement);
serializer.SerializeValue(ref Aim);
```

In `RootLifetimeScope.cs`: register the bridge with RC-backed config:

```csharp
builder.Register<MobileCoreInputBridge>(resolver =>
{
    var config = resolver.Resolve<IConfigService>();
    return new MobileCoreInputBridge(new DualStickConfig(
        deadzone: config.Get("mc_input_deadzone", 0.15f),
        tapWindowSeconds: config.Get("mc_input_tap_window_ms", 300) / 1000f,
        tapIdleResetSeconds: config.Get("mc_input_tap_idle_reset_ms", 500) / 1000f));
}, Lifetime.Singleton);
```

NOTE: adapt `config.Get` to the repo's actual `IConfigService` signature (check `Application/Interfaces/IConfigService.cs` — it may be `Get<T>(key, fallback)` or `TryGetConfig<T>`; match it exactly).

- [ ] **Step 4: Delete old files and fix references**

```bash
git rm Assets/_KitchenClash/Domain/Interfaces/IDualStickInput.cs \
       Assets/_KitchenClash/Infrastructure/Input/GameplayInputMapper.cs \
       Assets/_KitchenClash/Infrastructure/Input/TouchInputProvider.cs \
       Assets/_KitchenClash/Infrastructure/Input/InputSystemProvider.cs \
       Assets/_KitchenClash/Infrastructure/Input/InputProviderFactory.cs \
       Assets/_KitchenClash/Infrastructure/Input/GameplayInputService.cs
grep -rn "IDualStickInput\|GameplayInputMapper\|TouchInputProvider\|InputSystemProvider\|GameplayInputService" Assets/ --include="*.cs"
```

For each remaining reference: migrate to `MobileCoreInputBridge`. For test files referencing deleted types, update them to drive `DualStickModel` via `ManualClock` (pattern from `DualStickModelTests`).

- [ ] **Step 5: Build, test, commit**

```bash
dotnet build RecipeRage.Gameplay.csproj -nologo
dotnet test RecipeRage.Tests.EditMode.csproj --no-build -nologo
git add -A
git commit -m "refactor(gameplay): cut input over to Playcenter.MobileCore (hard cutover — legacy deleted)

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

Expected: build green, suite green, zero references to deleted types.

---

### Task 9: RecipeRage Cutover — Session + Bots

**Files:**
- Create: `Assets/_KitchenClash/Composition/VContainerSessionScopeFactory.cs`
- Create: `Assets/_KitchenClash/Composition/VContainerSessionScopeHandle.cs`
- Create: `Assets/_KitchenClash/Composition/MenuSessionScopeInstallerAdapter.cs`
- Modify: `Assets/_KitchenClash/Infrastructure/DI/SessionManager.cs` (delegate to `SessionLifecycleController`)
- Create: `Assets/_KitchenClash/Application/Services/Evaluators/ExtinguishFireEvaluator.cs`
- Create: `Assets/_KitchenClash/Application/Services/Evaluators/DeliverToServingEvaluator.cs`
- Create: `Assets/_KitchenClash/Application/Services/Evaluators/BringToCookingEvaluator.cs`
- Create: `Assets/_KitchenClash/Application/Services/Evaluators/BringToPrepEvaluator.cs`
- Create: `Assets/_KitchenClash/Application/Services/Evaluators/FetchIngredientEvaluator.cs`
- Modify: `Assets/_KitchenClash/Infrastructure/Network/Bot/BotController.cs` (consume `BotBrain` tasks via host)
- Delete: `Assets/_KitchenClash/Application/Services/BotManager.cs`
- Delete: `Assets/_KitchenClash/Application/Services/BotTaskPlanner.cs`
- Delete: `Assets/_KitchenClash/Application/Services/BotClaimRegistry.cs`

**Interfaces:**
- Consumes: `SessionLifecycleController`, `ISessionScopeFactory`, `ISessionScopeHandle`, `ISessionContainerBuilder` (Task 5); `TaskPlanner<BotPlanningSnapshot, BotTaskPlan>`, `BotBrain`, `BotHost`, `ClaimRegistry<string>`, `BotBudget` (Task 6)
- Produces: game-side session factory (VContainer), kitchen evaluators (`ITaskEvaluator<BotPlanningSnapshot, BotTaskPlan>` implementations)

- [ ] **Step 1: Baseline test run**

```bash
dotnet test RecipeRage.Tests.EditMode.csproj --filter="Bot|Session" --no-build -nologo
```

Expected: PASS (record tests — BotTaskPlanner tests exist at `Assets/Scripts/Tests/EditMode/`; they move to evaluator tests in Step 4).

- [ ] **Step 2: Session factory adapter**

`VContainerSessionScopeFactory.cs`:

```csharp
using Playcenter.MobileCore;
using VContainer.Unity;

namespace KitchenClash.Composition
{
    /// <summary>
    /// Game-side session scope factory: wraps LifetimeScope child creation behind the
    /// module's container-neutral seam. Enforces the installer law (sole
    /// MenuSessionRegistrations path) exactly as wiki mandates.
    /// </summary>
    public sealed class VContainerSessionScopeFactory : ISessionScopeFactory
    {
        private readonly LifetimeScope _root;

        public VContainerSessionScopeFactory(LifetimeScope root)
        {
            _root = root;
        }

        public ISessionScopeHandle Create(ISessionScopeInstaller installer)
        {
            LifetimeScope child = _root.CreateChild(builder =>
                installer.Install(new VContainerSessionContainerBuilder(builder)));
            return new VContainerSessionScopeHandle(child);
        }
    }
}
```

`VContainerSessionContainerBuilder.cs`:

```csharp
using Playcenter.MobileCore;
using VContainer;

namespace KitchenClash.Composition
{
    /// <summary>Adapts the module's container-neutral builder to VContainer's IContainerBuilder.</summary>
    public sealed class VContainerSessionContainerBuilder : ISessionContainerBuilder
    {
        private readonly IContainerBuilder _builder;

        public VContainerSessionContainerBuilder(IContainerBuilder builder)
        {
            _builder = builder;
        }

        public void AddSingleton<TService>(TService instance) where TService : class
        {
            _builder.RegisterInstance(instance).As<TService>();
        }

        public void AddSingleton<TService, TImpl>()
            where TService : class where TImpl : class, TService, new()
        {
            _builder.Register<TImpl>(Lifetime.Singleton).As<TService>();
        }
    }
}
```

`VContainerSessionScopeHandle.cs`:

```csharp
using Playcenter.MobileCore;
using VContainer;
using VContainer.Unity;

namespace KitchenClash.Composition
{
    public sealed class VContainerSessionScopeHandle : ISessionScopeHandle
    {
        private readonly LifetimeScope _scope;

        public VContainerSessionScopeHandle(LifetimeScope scope)
        {
            _scope = scope;
        }

        public T Get<T>() where T : class
        {
            return _scope.Container.Resolve<T>();
        }

        public bool TryGet<T>(out T service) where T : class
        {
            return _scope.Container.TryResolve(out service);
        }

        public void Dispose()
        {
            if (_scope != null)
            {
                UnityEngine.Object.Destroy(_scope.gameObject);
            }
        }
    }
}
```

- [ ] **Step 3: Slim SessionManager**

Replace `SessionManager.CreateSession` internals with delegation:

```csharp
public void CreateSession()
{
    _controller.CreateAsync().GetAwaiter().GetResult();
    _sessionScope = null; // scope now owned by controller; keep API surface via Scope property
}
```

Keep the `ISessionLifecycle` public API unchanged; construct `_controller` in the `[Inject]` constructor via `new SessionLifecycleController(new VContainerSessionScopeFactory(rootScope), installerAdapter)`. The `MenuSessionScopeInstallerAdapter` wraps the existing `MenuSessionScopeInstaller` (KitchenClash.Application `ISessionScopeInstaller`) into the module port:

```csharp
public sealed class MenuSessionScopeInstallerAdapter : Playcenter.MobileCore.ISessionScopeInstaller
{
    private readonly KitchenClash.Application.ISessionScopeInstaller _inner;

    public MenuSessionScopeInstallerAdapter(KitchenClash.Application.ISessionScopeInstaller inner)
    {
        _inner = inner;
    }

    public void Install(ISessionContainerBuilder builder)
    {
        if (builder is VContainerSessionContainerBuilder vcontainer)
        {
            _inner.Install(vcontainer.Inner); // expose Inner IContainerBuilder on the adapter
        }
    }
}
```

NOTE: add `public IContainerBuilder Inner => _builder;` to `VContainerSessionContainerBuilder` for this adapter. This keeps the game's existing `MenuSessionRegistrations` path untouched (wiki law).

- [ ] **Step 4: Port bot evaluators**

Seven evaluators, one per priority level, preserving the exact logic of the deleted `BotTaskPlanner.TryPlan*` methods. Each takes `BotDifficultyConfig` + a seeded `Random` (from `BotBrain.Random` at wire-up) via constructor. Full code below.

`Evaluators/ExtinguishFireEvaluator.cs` (P1):

```csharp
using System;
using Playcenter.MobileCore;

namespace KitchenClash.Application.Services.Evaluators
{
    /// <summary>Priority 1: extinguish fires.</summary>
    public sealed class ExtinguishFireEvaluator : ITaskEvaluator<BotPlanningSnapshot, BotTaskPlan>
    {
        private readonly BotDifficultyConfig _config;
        private readonly Random _random;

        public ExtinguishFireEvaluator(BotDifficultyConfig config, Random random)
        {
            _config = config;
            _random = random;
        }

        public BotTaskPlan Evaluate(BotPlanningSnapshot snapshot)
        {
            if (snapshot.StationsOnFire == null || snapshot.StationsOnFire.Count == 0)
            {
                return null;
            }

            if (!_config.CanExtinguishFires)
            {
                return null;
            }

            if (_config.FireExtinguishChance < 1.0f && _random.NextDouble() > _config.FireExtinguishChance)
            {
                return null;
            }

            if (snapshot.IsHoldingItem)
            {
                return null;
            }

            return new BotTaskPlan
            {
                Type = BotTaskType.ExtinguishFire,
                TargetStationId = snapshot.StationsOnFire[0],
                DelayBeforeAction = _config.ReactionDelay
            };
        }
    }
}
```

`Evaluators/DeliverToServingEvaluator.cs` (P2):

```csharp
using Playcenter.MobileCore;

namespace KitchenClash.Application.Services.Evaluators
{
    /// <summary>Priority 2: holding cooked item (or plate) → deliver to serving.</summary>
    public sealed class DeliverToServingEvaluator : ITaskEvaluator<BotPlanningSnapshot, BotTaskPlan>
    {
        private readonly BotDifficultyConfig _config;

        public DeliverToServingEvaluator(BotDifficultyConfig config)
        {
            _config = config;
        }

        public BotTaskPlan Evaluate(BotPlanningSnapshot snapshot)
        {
            if (!snapshot.IsHoldingItem || !snapshot.HeldItemIsCooked)
            {
                return null;
            }

            string targetServing = EvaluatorHelpers.PickFirst(snapshot.ServingStationIds);
            if (targetServing == null)
            {
                return null;
            }

            return new BotTaskPlan
            {
                Type = BotTaskType.DeliverToServing,
                TargetStationId = targetServing,
                DelayBeforeAction = _config.ReactionDelay
            };
        }
    }
}
```

`Evaluators/BringToCookingEvaluator.cs` (P3):

```csharp
using Playcenter.MobileCore;

namespace KitchenClash.Application.Services.Evaluators
{
    /// <summary>Priority 3: holding cut/prepped item → bring to cooking.</summary>
    public sealed class BringToCookingEvaluator : ITaskEvaluator<BotPlanningSnapshot, BotTaskPlan>
    {
        private readonly BotDifficultyConfig _config;

        public BringToCookingEvaluator(BotDifficultyConfig config)
        {
            _config = config;
        }

        public BotTaskPlan Evaluate(BotPlanningSnapshot snapshot)
        {
            if (!snapshot.IsHoldingItem || !snapshot.HeldItemIsCut)
            {
                return null;
            }

            if (snapshot.HeldItemIsCooked || snapshot.HeldItemIsBurned)
            {
                return null;
            }

            string cookingId = EvaluatorHelpers.PickFirst(snapshot.CookingStationIds);
            if (cookingId == null)
            {
                return null;
            }

            return new BotTaskPlan
            {
                Type = BotTaskType.BringToCooking,
                TargetStationId = cookingId,
                TargetIngredient = snapshot.HeldIngredientType,
                DelayBeforeAction = _config.ReactionDelay
            };
        }
    }
}
```

`Evaluators/BringToPrepEvaluator.cs` (P4):

```csharp
using Playcenter.MobileCore;

namespace KitchenClash.Application.Services.Evaluators
{
    /// <summary>Priority 4: holding raw ingredient → bring to prep.</summary>
    public sealed class BringToPrepEvaluator : ITaskEvaluator<BotPlanningSnapshot, BotTaskPlan>
    {
        private readonly BotDifficultyConfig _config;

        public BringToPrepEvaluator(BotDifficultyConfig config)
        {
            _config = config;
        }

        public BotTaskPlan Evaluate(BotPlanningSnapshot snapshot)
        {
            if (!snapshot.IsHoldingItem || !snapshot.HeldItemIsRaw)
            {
                return null;
            }

            if (snapshot.HeldItemIsCut || snapshot.HeldItemIsCooked || snapshot.HeldItemIsBurned)
            {
                return null;
            }

            string prepId = EvaluatorHelpers.PickFirst(snapshot.PrepStationIds);
            if (prepId == null)
            {
                return null;
            }

            return new BotTaskPlan
            {
                Type = BotTaskType.BringToPrep,
                TargetStationId = prepId,
                TargetIngredient = snapshot.HeldIngredientType,
                DelayBeforeAction = _config.ReactionDelay
            };
        }
    }
}
```

`Evaluators/RecoverBurnedEvaluator.cs` (P5):

```csharp
using Playcenter.MobileCore;

namespace KitchenClash.Application.Services.Evaluators
{
    /// <summary>Priority 5: holding burned item → recover (drop it).</summary>
    public sealed class RecoverBurnedEvaluator : ITaskEvaluator<BotPlanningSnapshot, BotTaskPlan>
    {
        private readonly BotDifficultyConfig _config;

        public RecoverBurnedEvaluator(BotDifficultyConfig config)
        {
            _config = config;
        }

        public BotTaskPlan Evaluate(BotPlanningSnapshot snapshot)
        {
            if (!snapshot.IsHoldingItem || !snapshot.HeldItemIsBurned)
            {
                return null;
            }

            return new BotTaskPlan
            {
                Type = BotTaskType.Recover,
                DelayBeforeAction = _config.ReactionDelay
            };
        }
    }
}
```

`Evaluators/ClaimOrderEvaluator.cs` (P6):

```csharp
using Playcenter.MobileCore;

namespace KitchenClash.Application.Services.Evaluators
{
    /// <summary>Priority 6: claim highest-priority unclaimed order.</summary>
    public sealed class ClaimOrderEvaluator : ITaskEvaluator<BotPlanningSnapshot, BotTaskPlan>
    {
        private readonly BotDifficultyConfig _config;

        public ClaimOrderEvaluator(BotDifficultyConfig config)
        {
            _config = config;
        }

        public BotTaskPlan Evaluate(BotPlanningSnapshot snapshot)
        {
            if (snapshot.ClaimedOrderId.HasValue)
            {
                return null;
            }

            if (snapshot.Orders == null || snapshot.Orders.Count == 0)
            {
                return null;
            }

            BotOrderDescriptor best = null;
            foreach (BotOrderDescriptor order in snapshot.Orders)
            {
                if (order.IsExpired || order.IsCompleted)
                {
                    continue;
                }

                if (best == null || order.Priority > best.Priority)
                {
                    best = order;
                }
            }

            if (best == null)
            {
                return null;
            }

            return new BotTaskPlan
            {
                Type = BotTaskType.ClaimOrder,
                OrderId = best.OrderId,
                DelayBeforeAction = _config.ReactionDelay
            };
        }
    }
}
```

`Evaluators/FetchIngredientEvaluator.cs` (P7):

```csharp
using System;
using KitchenClash.Domain;
using Playcenter.MobileCore;

namespace KitchenClash.Application.Services.Evaluators
{
    /// <summary>Priority 7: fetch ingredient for the claimed order (mistake chance applies).</summary>
    public sealed class FetchIngredientEvaluator : ITaskEvaluator<BotPlanningSnapshot, BotTaskPlan>
    {
        private readonly BotDifficultyConfig _config;
        private readonly Random _random;

        public FetchIngredientEvaluator(BotDifficultyConfig config, Random random)
        {
            _config = config;
            _random = random;
        }

        public BotTaskPlan Evaluate(BotPlanningSnapshot snapshot)
        {
            if (snapshot.IsHoldingItem)
            {
                return null;
            }

            if (!snapshot.ClaimedOrderId.HasValue)
            {
                return null;
            }

            string ingredientStationId = EvaluatorHelpers.PickFirst(snapshot.IngredientStationIds);
            if (ingredientStationId == null)
            {
                return null;
            }

            return new BotTaskPlan
            {
                Type = BotTaskType.FetchIngredient,
                TargetStationId = ingredientStationId,
                TargetIngredient = DetermineIngredientToFetch(snapshot),
                OrderId = snapshot.ClaimedOrderId,
                DelayBeforeAction = _config.ReactionDelay
            };
        }

        private IngredientType DetermineIngredientToFetch(BotPlanningSnapshot snapshot)
        {
            if (_config.MistakeChance > 0f && _random.NextDouble() < _config.MistakeChance)
            {
                if (snapshot.AvailableIngredients != null && snapshot.AvailableIngredients.Length > 0)
                {
                    string randomName = snapshot.AvailableIngredients[_random.Next(snapshot.AvailableIngredients.Length)];
                    if (Enum.TryParse<IngredientType>(randomName, true, out IngredientType mistakeType))
                    {
                        return mistakeType;
                    }
                }
            }

            if (snapshot.AvailableIngredients != null && snapshot.AvailableIngredients.Length > 0)
            {
                if (Enum.TryParse<IngredientType>(snapshot.AvailableIngredients[0], true, out IngredientType correctType))
                {
                    return correctType;
                }
            }

            return IngredientType.None;
        }
    }
}
```

`Evaluators/EvaluatorHelpers.cs` (shared helper + P8 fallback):

```csharp
using System.Collections.Generic;

namespace KitchenClash.Application.Services.Evaluators
{
    public static class EvaluatorHelpers
    {
        public static string PickFirst(List<string> ids)
        {
            return ids != null && ids.Count > 0 ? ids[0] : null;
        }
    }

    /// <summary>Priority 8 (terminal): always returns a wander task. Registered last.</summary>
    public sealed class WanderEvaluator : Playcenter.MobileCore.ITaskEvaluator<BotPlanningSnapshot, BotTaskPlan>
    {
        private readonly BotDifficultyConfig _config;

        public WanderEvaluator(BotDifficultyConfig config)
        {
            _config = config;
        }

        public BotTaskPlan Evaluate(BotPlanningSnapshot snapshot)
        {
            return new BotTaskPlan
            {
                Type = BotTaskType.Wander,
                DelayBeforeAction = _config.ReactionDelay
            };
        }
    }
}
```

Wire-up (in `MatchLifetimeScope` or wherever bots are spawned) — registration order IS the priority chain:

```csharp
var planner = new TaskPlanner<BotPlanningSnapshot, BotTaskPlan>();
planner.Register(new ExtinguishFireEvaluator(difficultyConfig, brain.Random));
planner.Register(new DeliverToServingEvaluator(difficultyConfig));
planner.Register(new BringToCookingEvaluator(difficultyConfig));
planner.Register(new BringToPrepEvaluator(difficultyConfig));
planner.Register(new RecoverBurnedEvaluator(difficultyConfig));
planner.Register(new ClaimOrderEvaluator(difficultyConfig));
planner.Register(new FetchIngredientEvaluator(difficultyConfig, brain.Random));
planner.Register(new WanderEvaluator(difficultyConfig));
```

Move the existing BotTaskPlanner tests to per-evaluator tests: each `Plan_*` test becomes an `Evaluate_*` test against the specific evaluator (construct evaluator with `BotDifficultyConfig.FromDifficulty(...)` + `new Random(42)`); the chain-order tests are already covered by Task 6 core `TaskPlannerTests` — delete duplicates.

- [ ] **Step 5: Rewire BotController and delete old files**

`BotController` replaces `_planner.Plan(snapshot)` with `_brain.Think(snapshot)`; `_claimRegistry` (`BotClaimRegistry.Shared`) becomes the module's `ClaimRegistry<string>` instance injected at spawn (station ids as keys). Then:

```bash
git rm Assets/_KitchenClash/Application/Services/BotManager.cs \
       Assets/_KitchenClash/Application/Services/BotTaskPlanner.cs \
       Assets/_KitchenClash/Application/Services/BotClaimRegistry.cs
grep -rn "BotTaskPlanner\|BotClaimRegistry\|BotManager" Assets/ --include="*.cs"
```

Migrate every remaining reference (MatchLifetimeScope registrations → register `TaskPlanner<BotPlanningSnapshot, BotTaskPlan>` with evaluators in priority order + `BotBrain` per spawned bot + one `BotHost` per match). Also check `Assets/_KitchenClash/Infrastructure/EOS/EOSMatchmakingService.cs` — it references `BotManager` for bot-fill; migrate to `BotHost.Add(new BotBrain<BotPlanningSnapshot, BotTaskPlan>(botId, planner, seed))` at spawn time.

- [ ] **Step 6: Build, test, commit**

```bash
dotnet build RecipeRage.Gameplay.csproj -nologo
dotnet test RecipeRage.Tests.EditMode.csproj --no-build -nologo
git add -A
git commit -m "refactor(gameplay): cut session + bots over to Playcenter.MobileCore (hard cutover — legacy deleted)

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

Expected: build green, suite green, zero references to deleted types.

---

### Task 10: RecipeRage Cutover — Net Glue + Wiki Update

**Files:**
- Modify: `Assets/_KitchenClash/Infrastructure/Network/NetSessionConnectivityBridge.cs` (delegate to `ReconnectStateMachine`)
- Modify: `Assets/_KitchenClash/Composition/RootLifetimeScope.cs` (register `NetSessionOrchestrator` + `ConnectionQualityTracker` with RC-backed configs)
- Create: `wiki/MobileCore.md`
- Modify: `wiki/LLM-Rules.md` (add MobileCore Required/Forbidden section + testing amendment note)
- Modify: `wiki/Technical.md` (testing amendment for module-specific money-path policy)
- Modify: `wiki/log.md` (append entry)

**Interfaces:**
- Consumes: `NetSessionOrchestrator`, `ReconnectStateMachine`, `ReconnectConfig`, `ConnectionQualityTracker` (Task 7); existing `NgoEosNetSession` (implements `INetSession`)
- Produces: net start/stop in game code flows only through `NetSessionOrchestrator` (wiki law enforcement)

- [ ] **Step 1: Rewire the connectivity bridge**

Replace the bridge's ad-hoc retry logic with delegation:

```csharp
using Playcenter.MobileCore;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// Feeds connectivity signals into the module's ReconnectStateMachine and
    /// stops the net session on terminal failure (host drop = forfeit, no migration v1).
    /// </summary>
    public sealed class NetSessionConnectivityBridge
    {
        private readonly NetSessionOrchestrator _orchestrator;

        public NetSessionConnectivityBridge(NetSessionOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
            _orchestrator.Reconnect.ReconnectFailed += OnReconnectFailed;
        }

        public void NotifyDisconnected()
        {
            _orchestrator.NotifyDisconnected();
        }

        public void NotifyConnected()
        {
            _orchestrator.Reconnect.OnConnected();
        }

        private void OnReconnectFailed()
        {
            _ = _orchestrator.StopAsync(); // forfeit path; UI overlays observe ReconnectState
        }
    }
}
```

In `RootLifetimeScope`, register (match the repo's `IConfigService` signature exactly as in Task 8):

```csharp
builder.Register<ReconnectStateMachine>(resolver =>
{
    var config = resolver.Resolve<IConfigService>();
    var clock = PlaycenterBootstrap.Instance.Core.Clock;
    return new ReconnectStateMachine(
        new ReconnectConfig(
            maxAttempts: config.Get("mc_reconnect_match_attempts", 3),
            attemptIntervalSeconds: config.Get("mc_reconnect_match_interval_ms", 5000) / 1000f,
            backoffBaseSeconds: config.Get("mc_reconnect_backoff_base_ms", 1000) / 1000f),
        clock,
        seed: Environment.TickCount);
}, Lifetime.Singleton);

builder.Register<NetSessionOrchestrator>(resolver =>
    new NetSessionOrchestrator(
        resolver.Resolve<INetSession>(),
        resolver.Resolve<ReconnectStateMachine>()),
    Lifetime.Singleton);

builder.Register<ConnectionQualityTracker>(resolver =>
{
    var config = resolver.Resolve<IConfigService>();
    return new ConnectionQualityTracker(
        degradedMs: config.Get("mc_reconnect_degraded_ms", 150f),
        poorMs: config.Get("mc_reconnect_poor_ms", 400f));
}, Lifetime.Singleton);
```

Update `GameStarter`/`MatchEndController` and any other direct `INetSession` consumers to go through `NetSessionOrchestrator` instead:

```bash
grep -rn "INetSession" Assets/_KitchenClash --include="*.cs" | grep -v "NetSessionOrchestrator\|NgoEosNetSession\|NetSessionConnectivityBridge"
```

Migrate each hit to the orchestrator.

- [ ] **Step 2: Build and test**

```bash
dotnet build RecipeRage.Gameplay.csproj -nologo
dotnet test RecipeRage.Tests.EditMode.csproj --no-build -nologo
tools/ci/grep-gates.sh
```

Expected: build green, suite green (existing `NetSessionDelegationTests` may need updating to target the orchestrator — update them in place), gate pass.

- [ ] **Step 3: Wiki update**

Create `wiki/MobileCore.md`:

```markdown
# Playcenter MobileCore

Reusable mobile multiplayer core: dual-stick input, session lifecycle, bot task-planner
framework, net reconnect glue. First consumer: RecipeRage (cutover complete 2026-07).

**Location:** `Assets/Playcenter/MobileCore/Runtime/`
**Spec:** `docs/superpowers/specs/2026-07-24-playcenter-mobile-core-design.md`
**Plan:** `docs/superpowers/plans/2026-07-24-playcenter-mobile-core.md`

## Layout

- `Core/` — engine-free (CI grep gate: no UnityEngine/VContainer/Netcode/Epic/Firebase/Cysharp)
- `Adapters/` — the only vendor zone (InputSystem touch provider, UnityGameClock)
- `Bootstrap/` — `PlaycenterBootstrap` MonoBehaviour, sole scene entry point

## Subsystems

| Area | Core types | Game adapters |
|------|-----------|---------------|
| Input | `DualStickModel`, `TapGestureDetector`, `InputFrame` (v1) | `TouchDualStickProvider`, `MobileCoreInputBridge` |
| Session | `SessionLifecycleController` (fail-closed FSM) | `VContainerSessionScopeFactory` (game Composition) |
| Bots | `TaskPlanner<TS,T>`, `ClaimRegistry<TK>`, `BotHost` (budgeted) | Kitchen evaluators, `BotController` |
| Net | `ReconnectStateMachine`, `BackoffPolicy`, `ConnectionQualityTracker`, `NetSessionOrchestrator` | `NetSessionConnectivityBridge` |

## Testing policy (module amendment 2026-07-24)

Money-path tests only: core state machines and planners. Thin adapters, DTOs, and
bootstrap glue are verified by inspection. This amends the blanket ">80% on all new
code" rule for this module (approved by project owner).

## RC keys

`mc_bot_budget_ms` (2) · `mc_reconnect_menu_interval_ms` (3000) · `mc_reconnect_match_attempts` (3) ·
`mc_reconnect_match_interval_ms` (5000) · `mc_reconnect_backoff_base_ms` (1000) ·
`mc_reconnect_degraded_ms` (150) · `mc_reconnect_poor_ms` (400) ·
`mc_input_deadzone` (0.15) · `mc_input_tap_window_ms` (300) · `mc_input_tap_idle_reset_ms` (500)
```

In `wiki/LLM-Rules.md`, append after the "Playcenter Shared Services" section:

```markdown
## Playcenter MobileCore — Required / Forbidden

Authoritative detail: `wiki/MobileCore.md`.
Spec: `docs/superpowers/specs/2026-07-24-playcenter-mobile-core-design.md`.

### REQUIRED

- `PlaycenterBootstrap` as the sole scene entry for the Playcenter stack; one prefab
- `IGameClock` for all time in Core — no `Time.`/`DateTime.` in Core logic
- Bot planning under `IBotBudget` time-slice — never unbounded scans per tick
- Reconnect via `ReconnectStateMachine` — no ad-hoc retry loops in game code
- Net start/stop via `NetSessionOrchestrator` — no direct `INetSession` in new code
- Seeded `Random` in `BotBrain` — deterministic bot behavior per match seed
- `InputFrame` version byte bumped on any wire-format change
- Session scope factory implemented game-side (`ISessionScopeFactory`) — module stays DI-neutral

### FORBIDDEN (MobileCore)

| Pattern | Why |
|---------|-----|
| `UnityEngine`/`VContainer`/`Netcode`/`Epic`/`Firebase`/`Cysharp` usings in `Core/` | Vendor firewall — CI grep gate |
| Second bootstrap MonoBehaviour for Playcenter stack | One entry point |
| Game-side reimplementation of dual-stick/gesture/reconnect/claim logic | Common logic lives in the module |
| Hardcoded timing/tuning in Core | Option structs + `mc_*` RC keys |
| Dual-path old/new subsystems | Hard cutover — delete in same commit |
| DI-container reference inside the module | Session factory is a game-side seam |
```

In `wiki/Technical.md` testing section, append:

```markdown
> **Amendment (2026-07-24):** Playcenter.MobileCore uses money-path testing — core state
> machines and planners only; adapters/DTOs/bootstrap verified by inspection. Approved by
> project owner. The blanket >80% rule still applies to all other new code.
```

In `wiki/log.md`, append:

```markdown
- 2026-07-24: Added `MobileCore.md` (new module: input/session/bots/net + unified bootstrap).
  LLM-Rules gained MobileCore Required/Forbidden. Testing amendment recorded for the module
  (money-path only). Spec: docs/superpowers/specs/2026-07-24-playcenter-mobile-core-design.md.
```

- [ ] **Step 4: Final verification and commit**

```bash
dotnet build RecipeRage.Gameplay.csproj -nologo
dotnet test RecipeRage.Tests.EditMode.csproj --no-build -nologo
tools/ci/grep-gates.sh
git add -A
git commit -m "refactor(network): cut net glue to MobileCore orchestrator + wiki MobileCore page and rules

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

Expected: everything green; module complete; wiki authoritative.

---

## Done Definition (whole plan)

1. `tools/ci/grep-gates.sh` passes on `Core/`.
2. All new money-path tests pass; pre-existing EditMode suite unbroken.
3. Zero references to deleted legacy types (`IDualStickInput`, `GameplayInputMapper`, `TouchInputProvider`, `InputSystemProvider`, `GameplayInputService`, `BotManager`, `BotTaskPlanner`, `BotClaimRegistry`).
4. All net start/stop in game code flows through `NetSessionOrchestrator`.
5. `wiki/MobileCore.md` + LLM-Rules section + testing amendment + log entry committed.
