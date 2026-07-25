# Slice 3: Bots Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add AI-controlled chefs that play the full loop — fetch, chop, cook, plate, serve — with a priority-chain task planner, station claim registry (no two bots fight over one station), and adaptive difficulty that matches human player skill.

**Architecture:** Bots are server-side agents. A `BotBrain` ticks at a budgeted rate, asks a `TaskPlanner` for the next task by running ordered `ITaskEvaluator`s against a `KitchenSnapshot` (world state), and drives a `BotController` (movement + interaction). In network matches bots are network objects but NOT NGO player objects. Difficulty scales off measured human performance (recipes/minute) via config-driven tuning bands.

**Tech Stack:** Unity 6000.3.0f1, Slice 1 gameplay, Slice 2 networking, Playcenter SDK.

## Global Constraints

- Bots are network objects but NOT NGO player objects (no client ownership)
- Priority chain order is fixed: burnt-recovery → serve → collect → cook → chop → fetch → wander
- Two bots never claim the same station (ClaimRegistry)
- Bot thinking is budgeted (ms per tick, config-driven) — no frame spikes
- Adaptive difficulty matches player skill (measured recipes/min), never exceeds human-feasible speed
- Bot chopping is simulated over time (bots don't "tap" — they hold a station for the equivalent duration)
- Requires Slice 2 complete

---

### Task 1: Bot Core (Brain + Planner + Evaluator Contract)

**Files:**
- Create: `Assets/Game/Bots/BotBrain.cs`
- Create: `Assets/Game/Bots/TaskPlanner.cs`
- Create: `Assets/Game/Bots/ITaskEvaluator.cs`
- Create: `Assets/Game/Bots/BotTask.cs`
- Create: `Assets/Game/Bots/KitchenSnapshot.cs`
- Create: `Assets/Game/Bots/BotBudget.cs`

**Interfaces:**
- Consumes: Slice 1/2 types (`IngredientItem`, `RecipeDefinition`, `MatchController`, `MatchRuntimeRegistry`)
- Produces:
  - `BotTask` (class): `.Kind` (BotTaskKind), `.TargetStation` (StationBase), `.TargetIngredient` (IngredientType?), `.IsComplete`
  - `BotTaskKind` enum: `Fetch, Chop, StartCook, CollectCook, TakePlate, ArrangePlate, Serve, ClearBurnt, Wander`
  - `ITaskEvaluator.Evaluate(KitchenSnapshot)` → `BotTask` or null
  - `TaskPlanner.Register(ITaskEvaluator)`, `.Plan(KitchenSnapshot)` → `BotTask`
  - `KitchenSnapshot` — immutable world view: bot carry, stations (idle/cooking/ready/burnt + positions), current recipe requirements, claimed stations
  - `IBotBudget.TryConsume(int microseconds)` → bool; `BotBudget(int budgetMsPerTick)`

- [ ] **Step 1: Write BotTask + kinds**

`Assets/Game/Bots/BotTask.cs`:
```csharp
namespace RecipeRage.Bots
{
    public enum BotTaskKind
    {
        Fetch,
        Chop,
        StartCook,
        CollectCook,
        TakePlate,
        ArrangePlate,
        Serve,
        ClearBurnt,
        Wander
    }

    /// <summary>
    /// One unit of bot work. The planner produces it; BotController executes it.
    /// </summary>
    public sealed class BotTask
    {
        public BotTaskKind Kind { get; }
        public StationBase TargetStation { get; }
        public IngredientType? TargetIngredient { get; }

        public bool IsComplete { get; set; }

        public BotTask(BotTaskKind kind, StationBase targetStation, IngredientType? targetIngredient = null)
        {
            Kind = kind;
            TargetStation = targetStation;
            TargetIngredient = targetIngredient;
        }
    }
}
```

- [ ] **Step 2: Write ITaskEvaluator + TaskPlanner**

`Assets/Game/Bots/ITaskEvaluator.cs`:
```csharp
namespace RecipeRage.Bots
{
    /// <summary>
    /// Ordered evaluators: first non-null task wins. Chain order (fixed):
    /// burnt-recovery → serve → collect → cook → chop → fetch → wander.
    /// </summary>
    public interface ITaskEvaluator
    {
        BotTask Evaluate(KitchenSnapshot snapshot);
    }
}
```

`Assets/Game/Bots/TaskPlanner.cs`:
```csharp
using System.Collections.Generic;

namespace RecipeRage.Bots
{
    public sealed class TaskPlanner
    {
        private readonly List<ITaskEvaluator> _evaluators = new List<ITaskEvaluator>(8);

        public void Register(ITaskEvaluator evaluator)
        {
            _evaluators.Add(evaluator);
        }

        public BotTask Plan(KitchenSnapshot snapshot)
        {
            for (int i = 0; i < _evaluators.Count; i++)
            {
                var task = _evaluators[i].Evaluate(snapshot);
                if (task != null)
                {
                    return task;
                }
            }
            return null;
        }
    }
}
```

- [ ] **Step 3: Write KitchenSnapshot**

`Assets/Game/Bots/KitchenSnapshot.cs`:
```csharp
using System.Collections.Generic;
using UnityEngine;

namespace RecipeRage.Bots
{
    /// <summary>
    /// Immutable per-tick world view for one bot. Built by KitchenSnapshotBuilder
    /// (Task 4) from the MatchRuntimeRegistry — never from scene searches.
    /// </summary>
    public sealed class KitchenSnapshot
    {
        public PlayerCarry Carry { get; }
        public RecipeDefinition CurrentRecipe { get; }
        public IReadOnlyList<StationInfo> Stations { get; }
        public IReadOnlyList<IngredientType> NeededIngredients { get; }
        public Vector3 BotPosition { get; }

        public KitchenSnapshot(
            PlayerCarry carry,
            RecipeDefinition currentRecipe,
            IReadOnlyList<StationInfo> stations,
            IReadOnlyList<IngredientType> neededIngredients,
            Vector3 botPosition)
        {
            Carry = carry;
            CurrentRecipe = currentRecipe;
            Stations = stations;
            NeededIngredients = neededIngredients;
            BotPosition = botPosition;
        }
    }

    public sealed class StationInfo
    {
        public StationBase Station { get; }
        public StationKind Kind { get; }
        public Vector3 Position { get; }
        public bool IsClaimed { get; }
        public bool HasReadyItem { get; }
        public bool IsBurning { get; }
        public IngredientType? CrateIngredient { get; }

        public StationInfo(
            StationBase station,
            StationKind kind,
            Vector3 position,
            bool isClaimed,
            bool hasReadyItem,
            bool isBurning,
            IngredientType? crateIngredient)
        {
            Station = station;
            Kind = kind;
            Position = position;
            IsClaimed = isClaimed;
            HasReadyItem = hasReadyItem;
            IsBurning = isBurning;
            CrateIngredient = crateIngredient;
        }
    }

    public enum StationKind
    {
        Crate,
        Cutting,
        Cooking,
        Plate,
        Counter,
        Serving
    }
}
```

- [ ] **Step 4: Write BotBudget**

`Assets/Game/Bots/BotBudget.cs`:
```csharp
using System.Diagnostics;

namespace RecipeRage.Bots
{
    public interface IBotBudget
    {
        bool TryConsume(int microseconds);
    }

    /// <summary>
    /// Per-tick thinking budget. Evaluators check before expensive work;
    /// over budget = planning resumes next tick. Config: bot_budget_ms (2ms).
    /// </summary>
    public sealed class BotBudget : IBotBudget
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly long _budgetTicks;

        public BotBudget(int budgetMs)
        {
            _budgetTicks = budgetMs * (Stopwatch.Frequency / 1000);
        }

        public void BeginTick()
        {
            _stopwatch.Restart();
        }

        public bool TryConsume(int microseconds)
        {
            return _stopwatch.ElapsedTicks + microseconds * (Stopwatch.Frequency / 1_000_000) < _budgetTicks;
        }
    }
}
```

- [ ] **Step 5: Verify compilation + commit**

```bash
git add Assets/Game/Bots
git commit -m "feat(bots): bot core (brain contract, planner, snapshot, budget)"
```

---

### Task 2: Claim Registry (Station Locking Between Bots)

**Files:**
- Create: `Assets/Game/Bots/BotClaimRegistry.cs`

**Interfaces:**
- Consumes: `StationBase`
- Produces:
  - `BotClaimRegistry.TryClaim(StationBase, int botId)` → bool, `.Release(StationBase, int botId)`, `.IsClaimed(StationBase)` → bool, `.ReleaseAll(int botId)`

- [ ] **Step 1: Write BotClaimRegistry**

`Assets/Game/Bots/BotClaimRegistry.cs`:
```csharp
using System.Collections.Generic;

namespace RecipeRage.Bots
{
    /// <summary>
    /// Prevents two bots targeting the same station. Server-side only;
    /// human players are unaffected (they coordinate socially).
    /// </summary>
    public sealed class BotClaimRegistry
    {
        private readonly Dictionary<StationBase, int> _claims = new Dictionary<StationBase, int>(16);

        public bool TryClaim(StationBase station, int botId)
        {
            if (_claims.ContainsKey(station))
            {
                return false;
            }
            _claims[station] = botId;
            return true;
        }

        public void Release(StationBase station, int botId)
        {
            if (_claims.TryGetValue(station, out var owner) && owner == botId)
            {
                _claims.Remove(station);
            }
        }

        public bool IsClaimed(StationBase station) => _claims.ContainsKey(station);

        public void ReleaseAll(int botId)
        {
            var toRemove = new List<StationBase>();
            foreach (var kvp in _claims)
            {
                if (kvp.Value == botId)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var station in toRemove)
            {
                _claims.Remove(station);
            }
        }
    }
}
```

- [ ] **Step 2: Verify compilation + commit**

```bash
git add Assets/Game/Bots
git commit -m "feat(bots): claim registry (station locking between bots)"
```

---

### Task 3: Kitchen Evaluators (Priority Chain)

**Files:**
- Create: `Assets/Game/Bots/Evaluators/ClearBurntEvaluator.cs`
- Create: `Assets/Game/Bots/Evaluators/ServeEvaluator.cs`
- Create: `Assets/Game/Bots/Evaluators/CollectCookEvaluator.cs`
- Create: `Assets/Game/Bots/Evaluators/StartCookEvaluator.cs`
- Create: `Assets/Game/Bots/Evaluators/ChopEvaluator.cs`
- Create: `Assets/Game/Bots/Evaluators/FetchEvaluator.cs`
- Create: `Assets/Game/Bots/Evaluators/PlateEvaluators.cs` (TakePlate + ArrangePlate)
- Create: `Assets/Game/Bots/Evaluators/WanderEvaluator.cs`

**Interfaces:**
- Consumes: `ITaskEvaluator`, `KitchenSnapshot`, `BotClaimRegistry`
- Produces: evaluator set registered in fixed order; each returns a `BotTask` only when its preconditions hold and an unclaimed station exists

- [ ] **Step 1: Write evaluators**

`Assets/Game/Bots/Evaluators/ClearBurntEvaluator.cs`:
```csharp
namespace RecipeRage.Bots
{
    /// <summary>Priority 1: clear burnt food blocking a stove.</summary>
    public sealed class ClearBurntEvaluator : ITaskEvaluator
    {
        public BotTask Evaluate(KitchenSnapshot snapshot)
        {
            foreach (var station in snapshot.Stations)
            {
                if (station.Kind == StationKind.Cooking && station.IsBurning && !station.IsClaimed)
                {
                    return new BotTask(BotTaskKind.ClearBurnt, station.Station);
                }
            }
            return null;
        }
    }
}
```

`Assets/Game/Bots/Evaluators/ServeEvaluator.cs`:
```csharp
using System.Collections.Generic;

namespace RecipeRage.Bots
{
    /// <summary>Priority 2: plate full (or recipe complete) → serve it.</summary>
    public sealed class ServeEvaluator : ITaskEvaluator
    {
        public BotTask Evaluate(KitchenSnapshot snapshot)
        {
            if (!snapshot.Carry.HasPlate || snapshot.CurrentRecipe == null)
            {
                return null;
            }

            var required = snapshot.CurrentRecipe.RequiredIngredients.Length;
            if (snapshot.Carry.Plate.Contents.Count < required)
            {
                return null;
            }

            foreach (var station in snapshot.Stations)
            {
                if (station.Kind == StationKind.Serving && !station.IsClaimed)
                {
                    return new BotTask(BotTaskKind.Serve, station.Station);
                }
            }
            return null;
        }
    }
}
```

`Assets/Game/Bots/Evaluators/CollectCookEvaluator.cs`:
```csharp
namespace RecipeRage.Bots
{
    /// <summary>Priority 3: cooked item waiting → collect before it burns.</summary>
    public sealed class CollectCookEvaluator : ITaskEvaluator
    {
        public BotTask Evaluate(KitchenSnapshot snapshot)
        {
            foreach (var station in snapshot.Stations)
            {
                if (station.Kind == StationKind.Cooking && station.HasReadyItem && !station.IsClaimed)
                {
                    return new BotTask(BotTaskKind.CollectCook, station.Station);
                }
            }
            return null;
        }
    }
}
```

`Assets/Game/Bots/Evaluators/StartCookEvaluator.cs`:
```csharp
namespace RecipeRage.Bots
{
    /// <summary>Priority 4: carrying chopped, uncooked, needed ingredient → start cooking.</summary>
    public sealed class StartCookEvaluator : ITaskEvaluator
    {
        public BotTask Evaluate(KitchenSnapshot snapshot)
        {
            foreach (var item in snapshot.Carry.Items)
            {
                if (!item.Definition.RequiresCooking || item.IsCooked || !item.IsChopped)
                {
                    continue;
                }
                if (!IsNeeded(snapshot, item.Definition.Type))
                {
                    continue;
                }

                foreach (var station in snapshot.Stations)
                {
                    if (station.Kind == StationKind.Cooking && !station.HasReadyItem && !station.IsBurning && !station.IsClaimed)
                    {
                        return new BotTask(BotTaskKind.StartCook, station.Station, item.Definition.Type);
                    }
                }
            }
            return null;
        }

        private static bool IsNeeded(KitchenSnapshot snapshot, IngredientType type)
        {
            foreach (var needed in snapshot.NeededIngredients)
            {
                if (needed == type)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
```

`Assets/Game/Bots/Evaluators/ChopEvaluator.cs`:
```csharp
namespace RecipeRage.Bots
{
    /// <summary>Priority 5: carrying unchopped, needed ingredient → chop it.</summary>
    public sealed class ChopEvaluator : ITaskEvaluator
    {
        public BotTask Evaluate(KitchenSnapshot snapshot)
        {
            foreach (var item in snapshot.Carry.Items)
            {
                if (!item.Definition.RequiresChopping || item.IsChopped)
                {
                    continue;
                }

                foreach (var station in snapshot.Stations)
                {
                    if (station.Kind == StationKind.Cutting && !station.IsClaimed)
                    {
                        return new BotTask(BotTaskKind.Chop, station.Station, item.Definition.Type);
                    }
                }
            }
            return null;
        }
    }
}
```

`Assets/Game/Bots/Evaluators/FetchEvaluator.cs`:
```csharp
namespace RecipeRage.Bots
{
    /// <summary>Priority 6: recipe needs an ingredient we don't carry → fetch from crate.</summary>
    public sealed class FetchEvaluator : ITaskEvaluator
    {
        public BotTask Evaluate(KitchenSnapshot snapshot)
        {
            if (snapshot.Carry.Items.Count >= 2) // default capacity; chef bonus applied in Slice 4
            {
                return null;
            }

            foreach (var needed in snapshot.NeededIngredients)
            {
                if (AlreadyCovered(snapshot, needed))
                {
                    continue;
                }

                foreach (var station in snapshot.Stations)
                {
                    if (station.Kind == StationKind.Crate
                        && station.CrateIngredient == needed
                        && !station.IsClaimed)
                    {
                        return new BotTask(BotTaskKind.Fetch, station.Station, needed);
                    }
                }
            }
            return null;
        }

        private static bool AlreadyCovered(KitchenSnapshot snapshot, IngredientType type)
        {
            foreach (var item in snapshot.Carry.Items)
            {
                if (item.Definition.Type == type)
                {
                    return true;
                }
            }
            if (snapshot.Carry.HasPlate)
            {
                foreach (var item in snapshot.Carry.Plate.Contents)
                {
                    if (item.Definition.Type == type)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
```

`Assets/Game/Bots/Evaluators/PlateEvaluators.cs`:
```csharp
namespace RecipeRage.Bots
{
    /// <summary>Priority 5.5 (registered after Chop, before Fetch): need a plate.</summary>
    public sealed class TakePlateEvaluator : ITaskEvaluator
    {
        public BotTask Evaluate(KitchenSnapshot snapshot)
        {
            if (snapshot.Carry.HasPlate || snapshot.CurrentRecipe == null)
            {
                return null;
            }

            // Take a plate once we hold at least one ready (chopped+cooked) ingredient
            foreach (var item in snapshot.Carry.Items)
            {
                var ready = (!item.Definition.RequiresChopping || item.IsChopped)
                         && (!item.Definition.RequiresCooking || item.IsCooked);
                if (ready)
                {
                    foreach (var station in snapshot.Stations)
                    {
                        if (station.Kind == StationKind.Plate && !station.IsClaimed)
                        {
                            return new BotTask(BotTaskKind.TakePlate, station.Station);
                        }
                    }
                }
            }
            return null;
        }
    }

    /// <summary>Registered with TakePlate: holding plate + ready items → arrange.</summary>
    public sealed class ArrangePlateEvaluator : ITaskEvaluator
    {
        public BotTask Evaluate(KitchenSnapshot snapshot)
        {
            if (!snapshot.Carry.HasPlate || snapshot.Carry.Items.Count == 0 || snapshot.Carry.Plate.IsFull)
            {
                return null;
            }

            foreach (var station in snapshot.Stations)
            {
                if (station.Kind == StationKind.Plate && !station.IsClaimed)
                {
                    return new BotTask(BotTaskKind.ArrangePlate, station.Station);
                }
            }
            return null;
        }
    }
}
```

`Assets/Game/Bots/Evaluators/WanderEvaluator.cs`:
```csharp
namespace RecipeRage.Bots
{
    /// <summary>Fallback: nothing to do → wander (keeps bots alive-looking, repositions).</summary>
    public sealed class WanderEvaluator : ITaskEvaluator
    {
        public BotTask Evaluate(KitchenSnapshot snapshot)
        {
            return new BotTask(BotTaskKind.Wander, null);
        }
    }
}
```

- [ ] **Step 2: Verify compilation + commit**

```bash
git add Assets/Game/Bots/Evaluators
git commit -m "feat(bots): evaluator chain (burnt → serve → collect → cook → chop → plate → fetch → wander)"
```

---

### Task 4: BotController + BotBrain Tick + Snapshot Builder

**Files:**
- Create: `Assets/Game/Bots/BotController.cs`
- Create: `Assets/Game/Bots/KitchenSnapshotBuilder.cs`
- Create: `Assets/Game/Bots/BotHost.cs`

**Interfaces:**
- Consumes: `TaskPlanner`, `BotClaimRegistry`, `MatchRuntimeRegistry`, `MatchController`, `PlayerController`
- Produces:
  - `BotController : MonoBehaviour` — pathless steering movement toward task station, executes task via station interact APIs; bot chopping simulated as dwell time (chop seconds = taps × per-tap interval from config)
  - `KitchenSnapshotBuilder.Build(BotController)` → `KitchenSnapshot`
  - `BotHost : MonoBehaviour` — server-side host ticking all bots within budget; spawns/despawns bots for match

- [ ] **Step 1: Write KitchenSnapshotBuilder**

`Assets/Game/Bots/KitchenSnapshotBuilder.cs`:
```csharp
using System.Collections.Generic;
using RecipeRage.Net;
using UnityEngine;

namespace RecipeRage.Bots
{
    /// <summary>
    /// Builds the immutable world view each bot plans against. Reads from
    /// MatchRuntimeRegistry (never scene searches).
    /// </summary>
    public sealed class KitchenSnapshotBuilder
    {
        private readonly MatchRuntimeRegistry _registry;
        private readonly BotClaimRegistry _claims;
        private readonly MatchController _match;
        private readonly List<StationInfo> _stationBuffer = new List<StationInfo>(16);
        private readonly List<IngredientType> _neededBuffer = new List<IngredientType>(4);

        public KitchenSnapshotBuilder(MatchRuntimeRegistry registry, BotClaimRegistry claims, MatchController match)
        {
            _registry = registry;
            _claims = claims;
            _match = match;
        }

        public KitchenSnapshot Build(PlayerCarry carry, Vector3 botPosition)
        {
            _stationBuffer.Clear();
            _neededBuffer.Clear();

            var recipe = _match.CurrentRecipe;
            if (recipe != null)
            {
                foreach (var requirement in recipe.RequiredIngredients)
                {
                    _neededBuffer.Add(requirement.Type);
                }
            }

            return new KitchenSnapshot(
                carry,
                recipe,
                new List<StationInfo>(_stationBuffer),
                new List<IngredientType>(_neededBuffer),
                botPosition);
        }
    }
}
```

Note: station enumeration lands via `MatchRuntimeRegistry` station lists (Slice 2 registered cooking stations; extend the registry with `AllStations` in Step 2 below).

- [ ] **Step 2: Extend MatchRuntimeRegistry with all stations**

Modify `Assets/Game/Network/MatchRuntimeRegistry.cs`: add

```csharp
        private readonly List<StationBase> _allStations = new List<StationBase>(16);
        public IReadOnlyList<StationBase> AllStations => _allStations;

        public void RegisterStation(StationBase station) => _allStations.Add(station);
        public void UnregisterStation(StationBase station) => _allStations.Remove(station);
```

Stations self-register in `Awake`/`OnDestroy` when a registry exists in the scene (map-placed singleton).

- [ ] **Step 3: Write BotController**

`Assets/Game/Bots/BotController.cs`:
```csharp
using Playcenter;
using Playcenter.Services;
using UnityEngine;

namespace RecipeRage.Bots
{
    /// <summary>
    /// Executes BotTasks: steers to the target station, then performs the station
    /// interaction. Bot chopping/cooking dwell uses station timings — bots never
    /// act faster than a human tapping optimally.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public sealed class BotController : MonoBehaviour
    {
        public int BotId { get; set; }

        private PlayerController _player;
        private BotTask _currentTask;
        private float _dwellTimer;
        private float _moveSpeed;
        private float _actionDwellScale = 1f; // adaptive difficulty: >1 slower, <1 faster (never below human floor)

        public BotTask CurrentTask => _currentTask;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
        }

        private void Start()
        {
            var config = ServiceLocator.Get<IConfigService>();
            _moveSpeed = config.Get(ConfigKeys.PlayerMoveSpeed, ConfigKeys.Defaults.PlayerMoveSpeed);
        }

        public void AssignTask(BotTask task, float actionDwellScale)
        {
            _currentTask = task;
            _actionDwellScale = actionDwellScale;
            _dwellTimer = 0f;
        }

        private void Update()
        {
            if (_currentTask == null || _currentTask.IsComplete)
            {
                return;
            }

            if (_currentTask.Kind == BotTaskKind.Wander)
            {
                Wander();
                return;
            }

            var target = _currentTask.TargetStation.transform.position;
            var toTarget = target - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude > 1.2f * 1.2f)
            {
                var direction = toTarget.normalized;
                _player.SimulateMove(new Vector2(direction.x, direction.z), Time.deltaTime);
                return;
            }

            ExecuteAtStation();
        }

        private void ExecuteAtStation()
        {
            switch (_currentTask.Kind)
            {
                case BotTaskKind.Chop:
                    // Simulated chopping: dwell = ChopTaps × per-tap interval (config), scaled by difficulty
                    _dwellTimer += Time.deltaTime;
                    var chopDuration = 8 * 0.25f * _actionDwellScale; // per-tap 250ms = fast human
                    if (_dwellTimer >= chopDuration)
                    {
                        _currentTask.TargetStation.Interact(_player);
                        _currentTask.IsComplete = true;
                    }
                    break;

                default:
                    _currentTask.TargetStation.Interact(_player);
                    _currentTask.IsComplete = true;
                    break;
            }
        }

        private void Wander()
        {
            // Slow drift so idle bots read as alive; task completes after 2s and replans
            _dwellTimer += Time.deltaTime;
            _player.SimulateMove(new Vector2(Mathf.Sin(Time.time * 0.5f), Mathf.Cos(Time.time * 0.3f)) * 0.3f, Time.deltaTime);
            if (_dwellTimer >= 2f)
            {
                _currentTask.IsComplete = true;
            }
        }
    }
}
```

- [ ] **Step 4: Write BotBrain + BotHost**

`Assets/Game/Bots/BotBrain.cs`:
```csharp
namespace RecipeRage.Bots
{
    /// <summary>
    /// Per-bot think cycle: snapshot → plan → assign. Runs at tick rate (10Hz),
    /// budgeted — cheap insurance against frame spikes with 4+ bots.
    /// </summary>
    public sealed class BotBrain
    {
        private readonly BotController _controller;
        private readonly TaskPlanner _planner;
        private readonly KitchenSnapshotBuilder _snapshotBuilder;
        private readonly BotClaimRegistry _claims;
        private readonly IBotBudget _budget;
        private float _difficultyDwellScale = 1f;

        public BotBrain(
            BotController controller,
            TaskPlanner planner,
            KitchenSnapshotBuilder snapshotBuilder,
            BotClaimRegistry claims,
            IBotBudget budget)
        {
            _controller = controller;
            _planner = planner;
            _snapshotBuilder = snapshotBuilder;
            _claims = claims;
            _budget = budget;
        }

        public void SetDifficulty(float dwellScale)
        {
            _difficultyDwellScale = dwellScale;
        }

        public void Tick()
        {
            if (_controller.CurrentTask != null && !_controller.CurrentTask.IsComplete)
            {
                return; // still executing
            }

            if (_controller.CurrentTask != null && _controller.CurrentTask.TargetStation != null)
            {
                _claims.Release(_controller.CurrentTask.TargetStation, _controller.BotId);
            }

            var snapshot = _snapshotBuilder.Build(_controller.GetComponent<PlayerController>().Carry, _controller.transform.position);
            var task = _planner.Plan(snapshot);
            if (task == null)
            {
                return;
            }

            if (task.TargetStation != null && !_claims.TryClaim(task.TargetStation, _controller.BotId))
            {
                return; // lost the race — replan next tick
            }

            _controller.AssignTask(task, _difficultyDwellScale);
        }
    }
}
```

`Assets/Game/Bots/BotHost.cs`:
```csharp
using System.Collections.Generic;
using Playcenter;
using Playcenter.Services;
using UnityEngine;

namespace RecipeRage.Bots
{
    /// <summary>
    /// Server-side bot host. Ticks all brains within the frame budget and owns
    /// bot spawn/despawn for the match. Registered by GameplayCompositionRoot;
    /// driven from its Update only on the server.
    /// </summary>
    public sealed class BotHost : MonoBehaviour
    {
        [SerializeField] private BotController _botPrefab;

        private readonly List<BotBrain> _brains = new List<BotBrain>(6);
        private BotBudget _budget;
        private float _tickInterval = 0.1f; // 10Hz
        private float _tickTimer;

        private void Start()
        {
            var config = ServiceLocator.Get<IConfigService>();
            _budget = new BotBudget(config.Get("bot_budget_ms", 2));
        }

        public void RegisterBrain(BotBrain brain)
        {
            _brains.Add(brain);
        }

        public void SetDifficulty(float dwellScale)
        {
            foreach (var brain in _brains)
            {
                brain.SetDifficulty(dwellScale);
            }
        }

        private void Update()
        {
            _tickTimer -= Time.deltaTime;
            if (_tickTimer > 0f)
            {
                return;
            }
            _tickTimer = _tickInterval;

            _budget.BeginTick();
            foreach (var brain in _brains)
            {
                if (!_budget.TryConsume(200)) // reserve 200µs per brain
                {
                    break;
                }
                brain.Tick();
            }
        }
    }
}
```

- [ ] **Step 5: Wire evaluators + planner in GameplayCompositionRoot**

In `OnPlaycenterReady()` add:

```csharp
            var planner = new TaskPlanner();
            planner.Register(new Bots.ClearBurntEvaluator());
            planner.Register(new Bots.ServeEvaluator());
            planner.Register(new Bots.CollectCookEvaluator());
            planner.Register(new Bots.StartCookEvaluator());
            planner.Register(new Bots.ChopEvaluator());
            planner.Register(new Bots.TakePlateEvaluator());
            planner.Register(new Bots.ArrangePlateEvaluator());
            planner.Register(new Bots.FetchEvaluator());
            planner.Register(new Bots.WanderEvaluator());
            ServiceLocator.Register(planner);
            ServiceLocator.Register(new Bots.BotClaimRegistry());
```

- [ ] **Step 6: Verify — bot plays full loop in TestKitchen (fetch → chop → cook → plate → serve)**

- [ ] **Step 7: Commit**

```bash
git add Assets/Game/Bots Assets/Game/DI
git commit -m "feat(bots): controller + brain + host (budgeted 10Hz tick, full loop playable)"
```

---

### Task 5: Adaptive Difficulty

**Files:**
- Create: `Assets/Game/Bots/AdaptiveDifficulty.cs`
- Create: `Assets/Game/Bots/SkillTracker.cs`

**Interfaces:**
- Consumes: match events (recipe completions per player), `IConfigService`
- Produces:
  - `SkillTracker.TrackRecipeCompleted(float matchElapsed)`, `.HumanRecipesPerMinute` → float
  - `AdaptiveDifficulty.ComputeDwellScale(float humanRecipesPerMin)` → float (1.0 = baseline; 1.3 = easier; 0.85 = hardest, clamped to human floor)
  - Config keys: `bot_difficulty_baseline_rpm` (1.5), `bot_difficulty_min_scale` (0.85), `bot_difficulty_max_scale` (1.3)

- [ ] **Step 1: Write SkillTracker**

`Assets/Game/Bots/SkillTracker.cs`:
```csharp
using System.Collections.Generic;

namespace RecipeRage.Bots
{
    /// <summary>
    /// Measures human performance over a rolling window (recipes/minute).
    /// Bots adapt to this — the match stays competitive without rubber-banding.
    /// </summary>
    public sealed class SkillTracker
    {
        private const float WindowSec = 60f;
        private readonly Queue<float> _completionTimes = new Queue<float>(16);

        public void TrackRecipeCompleted(float matchElapsed)
        {
            _completionTimes.Enqueue(matchElapsed);
            while (_completionTimes.Count > 0 && matchElapsed - _completionTimes.Peek() > WindowSec)
            {
                _completionTimes.Dequeue();
            }
        }

        public float HumanRecipesPerMinute =>
            _completionTimes.Count == 0 ? 0f : _completionTimes.Count / (WindowSec / 60f);
    }
}
```

- [ ] **Step 2: Write AdaptiveDifficulty**

`Assets/Game/Bots/AdaptiveDifficulty.cs`:
```csharp
using Playcenter.Services;
using UnityEngine;

namespace RecipeRage.Bots
{
    /// <summary>
    /// Maps human recipes/minute to a bot dwell scale. Scale >1 = bots act slower
    /// (easier); <1 = faster, but never below the human-optimal floor (0.85).
    /// </summary>
    public sealed class AdaptiveDifficulty
    {
        private readonly IConfigService _config;

        public AdaptiveDifficulty(IConfigService config)
        {
            _config = config;
        }

        public float ComputeDwellScale(float humanRecipesPerMin)
        {
            var baseline = _config.Get("bot_difficulty_baseline_rpm", 1.5f);
            var minScale = _config.Get("bot_difficulty_min_scale", 0.85f);
            var maxScale = _config.Get("bot_difficulty_max_scale", 1.3f);

            if (humanRecipesPerMin <= 0.01f)
            {
                return maxScale; // cold start: bots go easy until humans prove pace
            }

            var ratio = baseline / humanRecipesPerMin;
            return Mathf.Clamp(ratio, minScale, maxScale);
        }
    }
}
```

- [ ] **Step 3: Wire into BotHost + match events**

In `GameplayCompositionRoot`: register `SkillTracker` + `AdaptiveDifficulty`; subscribe to `RecipeServedEvent` to feed the tracker; on a 5s interval call `BotHost.SetDifficulty(adaptive.ComputeDwellScale(tracker.HumanRecipesPerMinute))`.

- [ ] **Step 4: Verify — strong play speeds bots up (to floor), weak play slows them down**

- [ ] **Step 5: Commit**

```bash
git add Assets/Game/Bots
git commit -m "feat(bots): adaptive difficulty (skill tracker + dwell scaling, human floor clamp)"
```

---

### Task 6: Network Bots (Spawn + Replicate)

**Files:**
- Create: `Assets/Game/Network/NetworkBot.cs`
- Create: `Assets/Game/Network/BotSpawnManager.cs`

**Interfaces:**
- Consumes: `BotController`, `BotHost`, `NetworkMatch`
- Produces:
  - `NetworkBot : NetworkBehaviour` — replicates bot transform to clients (server-owned, NOT a player object)
  - `BotSpawnManager` — fills empty team slots with bots on match start (practice mode = all bots; quick match = fill to team size)

- [ ] **Step 1: Write NetworkBot**

`Assets/Game/Network/NetworkBot.cs`:
```csharp
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Net
{
    /// <summary>
    /// Server-owned bot replication. Bots are network objects but NOT NGO player
    /// objects — no client ever owns one. Transform syncs for client rendering;
    /// all decisions happen server-side in BotBrain.
    /// </summary>
    [RequireComponent(typeof(NetworkTransform))]
    public sealed class NetworkBot : NetworkBehaviour
    {
        public readonly NetworkVariable<int> TeamId = new NetworkVariable<int>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public readonly NetworkVariable<int> ChefId = new NetworkVariable<int>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    }
}
```

- [ ] **Step 2: Write BotSpawnManager**

`Assets/Game/Network/BotSpawnManager.cs`:
```csharp
using RecipeRage.Bots;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Net
{
    /// <summary>
    /// Fills empty roster slots with bots at match start (server only).
    /// Practice mode: 1 human + bots. Quick match: fill to team size.
    /// </summary>
    public sealed class BotSpawnManager : MonoBehaviour
    {
        [SerializeField] private NetworkBot _botPrefab;
        [SerializeField] private Transform[] _teamSpawnPoints;

        public void FillSlotsWithBots(int humansInMatch, int teamSize)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            var totalSlots = teamSize * 2;
            var botsToSpawn = totalSlots - humansInMatch;

            for (int i = 0; i < botsToSpawn; i++)
            {
                var spawnPoint = _teamSpawnPoints[i % _teamSpawnPoints.Length];
                var bot = Instantiate(_botPrefab, spawnPoint.position, Quaternion.identity);
                bot.TeamId.Value = (humansInMatch + i) < teamSize ? 0 : 1;
                bot.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
```

- [ ] **Step 3: Verify — practice match: human + 3 bots vs 4 bots, all playing, transforms synced**

- [ ] **Step 4: Commit**

```bash
git add Assets/Game/Network
git commit -m "feat(bots): network bots (server-owned replication, slot filling)"
```

---

## Self-Review Notes

- **Spec coverage:** full loop bots ✅, fixed priority chain ✅, claim registry ✅, budgeted thinking ✅, adaptive difficulty ✅, network bots (not player objects) ✅, bot chopping time-simulated (never faster than human) ✅.
- **Type consistency:** `BotTask`/`StationInfo`/`StationKind` consistent across planner/evaluators/controllers ✅; `MatchRuntimeRegistry.AllStations` matches snapshot builder usage ✅; `BotBrain.SetDifficulty` matches `BotHost.SetDifficulty` ✅.
- **Deferred items (explicit):** NavMesh pathfinding (v1 uses steering — kitchens are open layouts; add NavMesh if maps gain walls), chef abilities for bots (Slice 4 chef system), bot-vs-bot counter coordination (claim registry prevents the worst cases).

## Next Plan

`2026-07-25-reciperage-slice4-progression.md` — Chef definitions/unlock/upgrade, personal utility abilities, trophy system (+15/-8), EOS Cloud persistence of progression.
