# Slice 1: Core Gameplay Loop Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the complete single-player gameplay loop — player movement, ingredient fetch, tap-burst chopping, autonomous cooking with burn, plate arrangement, serving, recipe tracking, match flow, and the forced first-launch tutorial.

**Architecture:** Gameplay logic lives in `Assets/Game/` and consumes SDK services (EventBus, Config, Logging) via ServiceLocator. Stations are plain MonoBehaviours (no networking yet — that lands in Slice 2). Domain events flow over the EventBus; AudioSystem subscribes to them. All tunables read through IConfigService.

**Tech Stack:** Unity 6000.3.0f1, C#, Playcenter SDK (Phase 0), Unity primitives (no NGO yet).

## Global Constraints

- Namespace: `RecipeRage` for all game code
- Game code contains gameplay logic ONLY — wallet/auth/save/config come from the SDK
- Chopping = fixed tap count (8/10/12 by tier); faster tapping = faster completion
- Cooking = autonomous station timer; burn after grace window; no score penalty
- Carry capacity = 2 items for ALL chefs (Marco ability adds +1, not in this slice)
- Recipe timers and burn progress bars are visible to ALL players (not chef abilities)
- No points displayed in HUD — recipe completion count only
- Requires Phase 0 complete (all SDK services registered)

---

### Task 1: Ingredient Domain + ScriptableObjects

**Files:**
- Create: `Assets/Game/Gameplay/Ingredient/IngredientType.cs`
- Create: `Assets/Game/Gameplay/Ingredient/IngredientDefinition.cs`
- Create: `Assets/Game/Gameplay/Ingredient/IngredientItem.cs`
- Create: `Assets/Game/Gameplay/Recipe/RecipeDefinition.cs`
- Create: `Assets/Game/Gameplay/Recipe/RecipeTier.cs`
- Create: `Assets/Game/Gameplay/Recipe/RecipeCatalog.cs`

**Interfaces:**
- Consumes: nothing (pure domain)
- Produces:
  - `IngredientType` enum: `Tomato, Onion, Garlic, Lettuce, Mushroom, Chicken, Beef, Fish, Rice, Pasta`
  - `IngredientDefinition` (SO): `.Type`, `.DisplayName`, `.Icon`, `.RequiresChopping`, `.RequiresCooking`, `.ChopTaps`, `.CookSeconds`
  - `IngredientItem`: `.Definition`, `.IsChopped`, `.IsCooked`, `.IsBurnt`, `.Chop()`, `.Cook()`, `.Burn()`
  - `RecipeDefinition` (SO): `.Id`, `.DisplayName`, `.Tier`, `.RequiredIngredients` (array of `IngredientRequirement`), `.Icon`
  - `IngredientRequirement`: `.Type`, `.RequiresChopped`, `.RequiresCooked`
  - `IRecipeCatalog.GetRandomRecipeList(int easy, int medium, int hard)` → `List<RecipeDefinition>`

- [ ] **Step 1: Write ingredient domain**

`Assets/Game/Gameplay/Ingredient/IngredientType.cs`:
```csharp
namespace RecipeRage
{
    public enum IngredientType
    {
        Tomato,
        Onion,
        Garlic,
        Lettuce,
        Mushroom,
        Chicken,
        Beef,
        Fish,
        Rice,
        Pasta
    }
}
```

`Assets/Game/Gameplay/Ingredient/IngredientDefinition.cs`:
```csharp
using UnityEngine;

namespace RecipeRage
{
    [CreateAssetMenu(fileName = "Ingredient", menuName = "RecipeRage/Ingredient Definition")]
    public sealed class IngredientDefinition : ScriptableObject
    {
        [SerializeField] private IngredientType _type;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private bool _requiresChopping = true;
        [SerializeField] private bool _requiresCooking = true;
        [SerializeField] private int _chopTaps = 8;
        [SerializeField] private float _cookSeconds = 12f;

        public IngredientType Type => _type;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public bool RequiresChopping => _requiresChopping;
        public bool RequiresCooking => _requiresCooking;
        public int ChopTaps => _chopTaps;
        public float CookSeconds => _cookSeconds;
    }
}
```

`Assets/Game/Gameplay/Ingredient/IngredientItem.cs`:
```csharp
namespace RecipeRage
{
    /// <summary>
    /// Runtime state of one ingredient instance moving through the kitchen.
    /// </summary>
    public sealed class IngredientItem
    {
        public IngredientDefinition Definition { get; }
        public bool IsChopped { get; private set; }
        public bool IsCooked { get; private set; }
        public bool IsBurnt { get; private set; }

        public IngredientItem(IngredientDefinition definition)
        {
            Definition = definition;
        }

        public void Chop()
        {
            if (!IsChopped)
            {
                IsChopped = true;
            }
        }

        public void Cook()
        {
            if (!IsCooked && !IsBurnt)
            {
                IsCooked = true;
            }
        }

        public void Burn()
        {
            IsBurnt = true;
        }
    }
}
```

- [ ] **Step 2: Write recipe domain**

`Assets/Game/Gameplay/Recipe/RecipeTier.cs`:
```csharp
namespace RecipeRage
{
    public enum RecipeTier
    {
        Easy = 1,
        Medium = 2,
        Hard = 3
    }
}
```

`Assets/Game/Gameplay/Recipe/RecipeDefinition.cs`:
```csharp
using System;
using UnityEngine;

namespace RecipeRage
{
    [CreateAssetMenu(fileName = "Recipe", menuName = "RecipeRage/Recipe Definition")]
    public sealed class RecipeDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private RecipeTier _tier = RecipeTier.Easy;
        [SerializeField] private Sprite _icon;
        [SerializeField] private IngredientRequirement[] _requiredIngredients = Array.Empty<IngredientRequirement>();

        public string Id => _id;
        public string DisplayName => _displayName;
        public RecipeTier Tier => _tier;
        public Sprite Icon => _icon;
        public IngredientRequirement[] RequiredIngredients => _requiredIngredients;
    }

    [Serializable]
    public sealed class IngredientRequirement
    {
        [SerializeField] private IngredientType _type;
        [SerializeField] private bool _requiresChopped = true;
        [SerializeField] private bool _requiresCooked = true;

        public IngredientType Type => _type;
        public bool RequiresChopped => _requiresChopped;
        public bool RequiresCooked => _requiresCooked;
    }
}
```

`Assets/Game/Gameplay/Recipe/RecipeCatalog.cs`:
```csharp
using System.Collections.Generic;
using UnityEngine;

namespace RecipeRage
{
    public interface IRecipeCatalog
    {
        /// <summary>
        /// Builds a shuffled match list: same list generated from the same seed
        /// (server generates in Slice 2, both teams receive identical lists).
        /// </summary>
        List<RecipeDefinition> GetRandomRecipeList(int easy, int medium, int hard, int seed);
    }

    public sealed class RecipeCatalog : IRecipeCatalog
    {
        private readonly List<RecipeDefinition> _easy = new List<RecipeDefinition>();
        private readonly List<RecipeDefinition> _medium = new List<RecipeDefinition>();
        private readonly List<RecipeDefinition> _hard = new List<RecipeDefinition>();

        public RecipeCatalog(RecipeDefinition[] allRecipes)
        {
            foreach (var recipe in allRecipes)
            {
                switch (recipe.Tier)
                {
                    case RecipeTier.Easy: _easy.Add(recipe); break;
                    case RecipeTier.Medium: _medium.Add(recipe); break;
                    case RecipeTier.Hard: _hard.Add(recipe); break;
                }
            }
        }

        public List<RecipeDefinition> GetRandomRecipeList(int easy, int medium, int hard, int seed)
        {
            var rng = new System.Random(seed);
            var result = new List<RecipeDefinition>(easy + medium + hard);
            PickRandom(_easy, easy, rng, result);
            PickRandom(_medium, medium, rng, result);
            PickRandom(_hard, hard, rng, result);
            return result;
        }

        private static void PickRandom(List<RecipeDefinition> source, int count, System.Random rng, List<RecipeDefinition> result)
        {
            var pool = new List<RecipeDefinition>(source);
            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                int index = rng.Next(pool.Count);
                result.Add(pool[index]);
                pool.RemoveAt(index);
            }
        }
    }
}
```

- [ ] **Step 3: Create starter content assets**

In the editor, create under `Assets/Art/Data/`:
- 10 `IngredientDefinition` assets (Tomato, Onion, Garlic, Lettuce, Mushroom, Chicken, Beef, Fish, Rice, Pasta) — set ChopTaps 8/10/12 and CookSeconds 12/15/18 across tiers
- 12 `RecipeDefinition` assets: 4 Easy (2 ingredients), 4 Medium (3), 4 Hard (3, higher taps/cook via ingredient defs)

- [ ] **Step 4: Verify compilation + commit**

```bash
git add Assets/Game/Gameplay Assets/Art/Data
git commit -m "feat(gameplay): ingredient + recipe domain with starter content"
```

---

### Task 2: Gameplay Events + Config Keys

**Files:**
- Create: `Assets/Game/Gameplay/Cooking/GameplayEvents.cs`
- Create: `Assets/Game/Gameplay/ConfigKeys.cs`

**Interfaces:**
- Consumes: `IEventBus`, `IConfigService` (SDK)
- Produces (event types consumed by AudioSystem, HUD, indicators in later tasks):
  - `IngredientFetchedEvent(IngredientType)`, `IngredientChoppedEvent(IngredientType)`, `CookingStartedEvent(int stationId)`, `CookingCompletedEvent(int stationId)`, `IngredientBurntEvent(int stationId)`, `PlateTakenEvent`, `IngredientPlatedEvent(IngredientType)`, `RecipeServedEvent(string recipeId)`, `MatchStartedEvent`, `MatchEndedEvent(bool won, int teamRecipes, int enemyRecipes)`

- [ ] **Step 1: Write events**

`Assets/Game/Gameplay/Cooking/GameplayEvents.cs`:
```csharp
namespace RecipeRage
{
    public readonly struct IngredientFetchedEvent
    {
        public IngredientType Type { get; }
        public IngredientFetchedEvent(IngredientType type) { Type = type; }
    }

    public readonly struct IngredientChoppedEvent
    {
        public IngredientType Type { get; }
        public IngredientChoppedEvent(IngredientType type) { Type = type; }
    }

    public readonly struct CookingStartedEvent
    {
        public int StationId { get; }
        public CookingStartedEvent(int stationId) { StationId = stationId; }
    }

    public readonly struct CookingCompletedEvent
    {
        public int StationId { get; }
        public CookingCompletedEvent(int stationId) { StationId = stationId; }
    }

    public readonly struct IngredientBurntEvent
    {
        public int StationId { get; }
        public IngredientBurntEvent(int stationId) { StationId = stationId; }
    }

    public readonly struct PlateTakenEvent { }

    public readonly struct IngredientPlatedEvent
    {
        public IngredientType Type { get; }
        public IngredientPlatedEvent(IngredientType type) { Type = type; }
    }

    public readonly struct RecipeServedEvent
    {
        public string RecipeId { get; }
        public RecipeServedEvent(string recipeId) { RecipeId = recipeId; }
    }

    public readonly struct MatchStartedEvent { }

    public readonly struct MatchEndedEvent
    {
        public bool Won { get; }
        public int TeamRecipes { get; }
        public int EnemyRecipes { get; }
        public MatchEndedEvent(bool won, int teamRecipes, int enemyRecipes)
        {
            Won = won;
            TeamRecipes = teamRecipes;
            EnemyRecipes = enemyRecipes;
        }
    }
}
```

- [ ] **Step 2: Write config keys**

`Assets/Game/Gameplay/ConfigKeys.cs`:
```csharp
namespace RecipeRage
{
    /// <summary>
    /// All tunables. Every value externalized via IConfigService.Get(key, default).
    /// </summary>
    public static class ConfigKeys
    {
        public const string MatchDurationSec = "match_duration_sec";
        public const string BurnGraceSec = "burn_grace_sec";
        public const string PlayerMoveSpeed = "player_move_speed";
        public const string CarryCapacity = "carry_capacity";
        public const string PlateCapacity = "plate_capacity";
        public const string InteractRange = "interact_range";
        public const string RecipesEasy2v2 = "recipes_easy_2v2";
        public const string RecipesMedium2v2 = "recipes_medium_2v2";
        public const string RecipesHard2v2 = "recipes_hard_2v2";

        public static class Defaults
        {
            public const float MatchDurationSec = 300f;
            public const float BurnGraceSec = 5f;
            public const float PlayerMoveSpeed = 5f;
            public const int CarryCapacity = 2;
            public const int PlateCapacity = 4;
            public const float InteractRange = 2f;
            public const int RecipesEasy2v2 = 4;
            public const int RecipesMedium2v2 = 4;
            public const int RecipesHard2v2 = 4;
        }
    }
}
```

- [ ] **Step 3: Verify compilation + commit**

```bash
git add Assets/Game/Gameplay
git commit -m "feat(gameplay): domain events + config keys"
```

---

### Task 3: Player Controller (Movement + Carry + Interact)

**Files:**
- Create: `Assets/Game/Gameplay/Player/PlayerController.cs`
- Create: `Assets/Game/Gameplay/Player/PlayerCarry.cs`
- Create: `Assets/Game/Gameplay/Station/IInteractable.cs`

**Interfaces:**
- Consumes: `IInputService` (Phase 0), `IConfigService`, `IEventBus`
- Produces:
  - `PlayerController` (MonoBehaviour): moves via `IInputService.MoveAxis`, finds nearest `IInteractable` within range, calls `Interact(player)` on `InteractPressed`
  - `PlayerCarry`: `.Items` (IReadOnlyList of `IngredientItem`), `.Plate` (Plate), `.TryAdd(IngredientItem)` → bool, `.Remove(IngredientItem)`, `.TryTakePlate()` → bool, `.HasPlate`
  - `IInteractable`: `.CanInteract(PlayerController)` → bool, `.Interact(PlayerController)`, `.GetPrompt()` → string

- [ ] **Step 1: Write IInteractable + PlayerCarry**

`Assets/Game/Gameplay/Station/IInteractable.cs`:
```csharp
namespace RecipeRage
{
    public interface IInteractable
    {
        bool CanInteract(PlayerController player);
        void Interact(PlayerController player);
        string GetPrompt();
    }
}
```

`Assets/Game/Gameplay/Player/PlayerCarry.cs`:
```csharp
using System.Collections.Generic;
using Playcenter.Services;

namespace RecipeRage
{
    /// <summary>
    /// What the player is holding: up to CarryCapacity ingredients and optionally one plate.
    /// Capacity default is 2 for ALL chefs (config-driven).
    /// </summary>
    public sealed class PlayerCarry
    {
        private readonly List<IngredientItem> _items = new List<IngredientItem>(4);
        private readonly int _capacity;

        public IReadOnlyList<IngredientItem> Items => _items;
        public Plate Plate { get; private set; }
        public bool HasPlate => Plate != null;

        public PlayerCarry(IConfigService config)
        {
            _capacity = config.Get(ConfigKeys.CarryCapacity, ConfigKeys.Defaults.CarryCapacity);
        }

        public bool TryAdd(IngredientItem item)
        {
            if (_items.Count >= _capacity)
            {
                return false;
            }
            _items.Add(item);
            return true;
        }

        public bool Remove(IngredientItem item)
        {
            return _items.Remove(item);
        }

        public void TakePlate(Plate plate)
        {
            Plate = plate;
        }

        public Plate ReleasePlate()
        {
            var plate = Plate;
            Plate = null;
            return plate;
        }
    }
}
```

- [ ] **Step 2: Write PlayerController**

`Assets/Game/Gameplay/Player/PlayerController.cs`:
```csharp
using Playcenter;
using Playcenter.Services;
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Top-down movement + proximity interaction. Single-player for now;
    /// NetworkPlayer wraps this in Slice 2.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        private CharacterController _characterController;
        private IInputService _input;
        private IConfigService _config;
        private float _moveSpeed;
        private float _interactRange;

        public PlayerCarry Carry { get; private set; }

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            _input = ServiceLocator.Get<IInputService>();
            _config = ServiceLocator.Get<IConfigService>();
            _moveSpeed = _config.Get(ConfigKeys.PlayerMoveSpeed, ConfigKeys.Defaults.PlayerMoveSpeed);
            _interactRange = _config.Get(ConfigKeys.InteractRange, ConfigKeys.Defaults.InteractRange);
            Carry = new PlayerCarry(_config);
        }

        private void Update()
        {
            var move = new Vector3(_input.MoveAxis.x, 0f, _input.MoveAxis.y);
            _characterController.Move(move * (_moveSpeed * Time.deltaTime));

            if (_input.InteractPressed)
            {
                TryInteract();
            }
        }

        private void TryInteract()
        {
            var nearest = FindNearestInteractable();
            if (nearest != null && nearest.CanInteract(this))
            {
                nearest.Interact(this);
            }
        }

        private IInteractable FindNearestInteractable()
        {
            var hits = Physics.OverlapSphere(transform.position, _interactRange);
            IInteractable nearest = null;
            var nearestDist = float.MaxValue;

            foreach (var hit in hits)
            {
                var interactable = hit.GetComponent<IInteractable>();
                if (interactable == null)
                {
                    continue;
                }

                var dist = (hit.transform.position - transform.position).sqrMagnitude;
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = interactable;
                }
            }
            return nearest;
        }
    }
}
```

- [ ] **Step 3: Verify compilation + commit**

```bash
git add Assets/Game/Gameplay/Player Assets/Game/Gameplay/Station
git commit -m "feat(gameplay): player controller (movement, carry, proximity interact)"
```

---

### Task 4: Stations (Crate, Cutting, Cooking, Plate, Counter, Serving)

**Files:**
- Create: `Assets/Game/Gameplay/Station/StationBase.cs`
- Create: `Assets/Game/Gameplay/Station/IngredientCrate.cs`
- Create: `Assets/Game/Gameplay/Station/CuttingStation.cs`
- Create: `Assets/Game/Gameplay/Station/CookingStation.cs`
- Create: `Assets/Game/Gameplay/Station/PlateStation.cs`
- Create: `Assets/Game/Gameplay/Station/CounterStation.cs`
- Create: `Assets/Game/Gameplay/Station/ServingStation.cs`
- Create: `Assets/Game/Gameplay/Cooking/Plate.cs`
- Create: `Assets/Game/Gameplay/Cooking/StationProgressView.cs`

**Interfaces:**
- Consumes: `IInteractable`, `PlayerController`, `PlayerCarry`, `IngredientItem`, `Plate`, `IEventBus`, `IConfigService`, gameplay events (Task 2)
- Produces:
  - `StationBase` — shared prompt/interaction shell
  - `IngredientCrate(IngredientDefinition)` — `Interact` gives raw ingredient, publishes `IngredientFetchedEvent`
  - `CuttingStation` — accepts unchopped ingredient from carry, tap-burst chop (`OnChopTap()`), publishes `IngredientChoppedEvent`
  - `CookingStation` — accepts chopped ingredient, autonomous timer, burn after grace; exposes `Progress01`, `IsBurning`, `HasReadyItem`, `TryCollect(out IngredientItem)`; publishes cooking/burn events
  - `PlateStation` — gives `Plate`, publishes `PlateTakenEvent`
  - `Plate` — `.TryArrange(IngredientItem)` → bool, `.Contents`, `.IsFull`
  - `CounterStation` — stores up to 2 items for later pickup
  - `ServingStation(IRecipeCatalog, MatchController)` — validates plate vs current recipe, publishes `RecipeServedEvent`
  - `StationProgressView` — world-space progress bar + burn warning above station (visible to ALL players)

- [ ] **Step 1: Write StationBase**

`Assets/Game/Gameplay/Station/StationBase.cs`:
```csharp
using UnityEngine;

namespace RecipeRage
{
    public abstract class StationBase : MonoBehaviour, IInteractable
    {
        [SerializeField] protected string _stationName = "Station";

        public virtual bool CanInteract(PlayerController player) => true;

        public abstract void Interact(PlayerController player);

        public virtual string GetPrompt() => $"Use {_stationName}";
    }
}
```

- [ ] **Step 2: Write IngredientCrate**

`Assets/Game/Gameplay/Station/IngredientCrate.cs`:
```csharp
using Playcenter;
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Dispenses raw ingredients. Which ingredient each crate holds is set per-map.
    /// </summary>
    public sealed class IngredientCrate : StationBase
    {
        [SerializeField] private IngredientDefinition _ingredient;

        private IEventBus _eventBus;

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
        }

        public override bool CanInteract(PlayerController player)
        {
            return player.Carry.Items.Count < 4; // hard cap guard; capacity check inside TryAdd
        }

        public override void Interact(PlayerController player)
        {
            var item = new IngredientItem(_ingredient);
            if (player.Carry.TryAdd(item))
            {
                _eventBus.Publish(new IngredientFetchedEvent(_ingredient.Type));
            }
        }

        public override string GetPrompt() => $"Take {_ingredient.DisplayName}";
    }
}
```

- [ ] **Step 3: Write CuttingStation**

`Assets/Game/Gameplay/Station/CuttingStation.cs`:
```csharp
using Playcenter;
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Tap-burst chopping. Player places an unchopped ingredient, then taps the
    /// Chop button ChopTaps times. Faster tapping = done sooner. Pure skill —
    /// no chef ability affects tap count.
    /// </summary>
    public sealed class CuttingStation : StationBase
    {
        private IngredientItem _current;
        private int _tapsRemaining;
        private PlayerController _placingPlayer;
        private IEventBus _eventBus;

        public bool HasIngredient => _current != null;
        public float Progress01 => _current == null ? 0f : 1f - (float)_tapsRemaining / _current.Definition.ChopTaps;

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _stationName = "Cutting Board";
        }

        private void Update()
        {
            // Chop input is global (ChopPressed) while this station has an item
            // and the placing player is still nearby — Slice 2 scopes this per-player.
            if (_current != null && _placingPlayer != null && ServiceLocator.Get<IInputService>().ChopPressed)
            {
                OnChopTap();
            }
        }

        public override bool CanInteract(PlayerController player)
        {
            return _current == null && HasUnchoppedIngredient(player);
        }

        public override void Interact(PlayerController player)
        {
            var item = TakeFirstUnchopped(player);
            if (item == null)
            {
                return;
            }

            _current = item;
            _tapsRemaining = item.Definition.ChopTaps;
            _placingPlayer = player;
        }

        public override string GetPrompt() => _current == null ? "Place ingredient to chop" : $"Chop! ({_tapsRemaining} taps)";

        private void OnChopTap()
        {
            _tapsRemaining--;
            if (_tapsRemaining <= 0)
            {
                _current.Chop();
                _eventBus.Publish(new IngredientChoppedEvent(_current.Definition.Type));
                _placingPlayer.Carry.TryAdd(_current);
                _current = null;
                _placingPlayer = null;
            }
        }

        private static bool HasUnchoppedIngredient(PlayerController player)
        {
            foreach (var item in player.Carry.Items)
            {
                if (item.Definition.RequiresChopping && !item.IsChopped)
                {
                    return true;
                }
            }
            return false;
        }

        private static IngredientItem TakeFirstUnchopped(PlayerController player)
        {
            foreach (var item in player.Carry.Items)
            {
                if (item.Definition.RequiresChopping && !item.IsChopped)
                {
                    player.Carry.Remove(item);
                    return item;
                }
            }
            return null;
        }
    }
}
```

- [ ] **Step 4: Write CookingStation**

`Assets/Game/Gameplay/Station/CookingStation.cs`:
```csharp
using Playcenter;
using Playcenter.Services;
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Autonomous cooking. Player places a chopped ingredient and walks away.
    /// Cooks on a timer (ALL players see the progress bar). After cooking, a
    /// burn-grace window starts; uncollected food burns (no score penalty,
    /// just wasted time). Interaction while ready collects the item.
    /// </summary>
    public sealed class CookingStation : StationBase
    {
        private enum Phase { Idle, Cooking, Ready, Burnt }

        private Phase _phase = Phase.Idle;
        private IngredientItem _current;
        private float _timer;
        private float _burnGrace;
        private IEventBus _eventBus;

        public int StationId => GetInstanceID();
        public float Progress01 { get; private set; }
        public bool IsBurning => _phase == Phase.Burnt;
        public bool HasReadyItem => _phase == Phase.Ready;
        public bool IsActive => _phase == Phase.Cooking || _phase == Phase.Ready;

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _burnGrace = ServiceLocator.Get<IConfigService>().Get(ConfigKeys.BurnGraceSec, ConfigKeys.Defaults.BurnGraceSec);
            _stationName = "Stove";
        }

        private void Update()
        {
            switch (_phase)
            {
                case Phase.Cooking:
                    _timer -= Time.deltaTime;
                    Progress01 = 1f - Mathf.Clamp01(_timer / _current.Definition.CookSeconds);
                    if (_timer <= 0f)
                    {
                        _phase = Phase.Ready;
                        _timer = _burnGrace;
                        Progress01 = 1f;
                        _current.Cook();
                        _eventBus.Publish(new CookingCompletedEvent(StationId));
                    }
                    break;

                case Phase.Ready:
                    _timer -= Time.deltaTime;
                    Progress01 = Mathf.Clamp01(_timer / _burnGrace); // drains = burn warning
                    if (_timer <= 0f)
                    {
                        _phase = Phase.Burnt;
                        _current.Burn();
                        _eventBus.Publish(new IngredientBurntEvent(StationId));
                    }
                    break;

                case Phase.Burnt:
                    // Burnt item sits until cleared by interaction (trash it)
                    break;
            }
        }

        public override bool CanInteract(PlayerController player)
        {
            if (_phase == Phase.Idle)
            {
                return HasCookableIngredient(player);
            }
            return _phase == Phase.Ready || _phase == Phase.Burnt;
        }

        public override void Interact(PlayerController player)
        {
            switch (_phase)
            {
                case Phase.Idle:
                    var item = TakeFirstCookable(player);
                    if (item != null)
                    {
                        _current = item;
                        _timer = item.Definition.CookSeconds;
                        _phase = Phase.Cooking;
                        Progress01 = 0f;
                        _eventBus.Publish(new CookingStartedEvent(StationId));
                    }
                    break;

                case Phase.Ready:
                    if (player.Carry.TryAdd(_current))
                    {
                        _current = null;
                        _phase = Phase.Idle;
                        Progress01 = 0f;
                    }
                    break;

                case Phase.Burnt:
                    // Clear burnt food (trash)
                    _current = null;
                    _phase = Phase.Idle;
                    Progress01 = 0f;
                    break;
            }
        }

        public override string GetPrompt() => _phase switch
        {
            Phase.Idle => "Place ingredient to cook",
            Phase.Cooking => "Cooking...",
            Phase.Ready => "Collect!",
            Phase.Burnt => "Clear burnt food",
            _ => string.Empty,
        };

        private static bool HasCookableIngredient(PlayerController player)
        {
            foreach (var item in player.Carry.Items)
            {
                if (item.Definition.RequiresCooking && !item.IsCooked && item.IsChopped)
                {
                    return true;
                }
            }
            return false;
        }

        private static IngredientItem TakeFirstCookable(PlayerController player)
        {
            foreach (var item in player.Carry.Items)
            {
                if (item.Definition.RequiresCooking && !item.IsCooked && item.IsChopped)
                {
                    player.Carry.Remove(item);
                    return item;
                }
            }
            return null;
        }
    }
}
```

- [ ] **Step 5: Write Plate + PlateStation**

`Assets/Game/Gameplay/Cooking/Plate.cs`:
```csharp
using System.Collections.Generic;

namespace RecipeRage
{
    /// <summary>
    /// A physical plate. Holds up to capacity arranged ingredients, consumed on serve.
    /// </summary>
    public sealed class Plate
    {
        private readonly List<IngredientItem> _contents = new List<IngredientItem>(4);
        private readonly int _capacity;

        public IReadOnlyList<IngredientItem> Contents => _contents;
        public bool IsFull => _contents.Count >= _capacity;

        public Plate(int capacity)
        {
            _capacity = capacity;
        }

        public bool TryArrange(IngredientItem item)
        {
            if (IsFull)
            {
                return false;
            }
            _contents.Add(item);
            return true;
        }
    }
}
```

`Assets/Game/Gameplay/Station/PlateStation.cs`:
```csharp
using Playcenter;
using Playcenter.Services;

namespace RecipeRage
{
    /// <summary>
    /// Dispenses empty plates (one at a time per player). Also accepts ingredients
    /// onto the held plate (arrange) when the player already holds one.
    /// </summary>
    public sealed class PlateStation : StationBase
    {
        private IEventBus _eventBus;
        private int _plateCapacity;

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _plateCapacity = ServiceLocator.Get<IConfigService>().Get(ConfigKeys.PlateCapacity, ConfigKeys.Defaults.PlateCapacity);
            _stationName = "Plate Station";
        }

        public override bool CanInteract(PlayerController player)
        {
            return !player.Carry.HasPlate || HasArrangeableItem(player);
        }

        public override void Interact(PlayerController player)
        {
            if (!player.Carry.HasPlate)
            {
                player.Carry.TakePlate(new Plate(_plateCapacity));
                _eventBus.Publish(new PlateTakenEvent());
                return;
            }

            // Arrange one carried item onto the held plate
            foreach (var item in player.Carry.Items)
            {
                if (player.Carry.Plate.TryArrange(item))
                {
                    player.Carry.Remove(item);
                    _eventBus.Publish(new IngredientPlatedEvent(item.Definition.Type));
                    return;
                }
            }
        }

        public override string GetPrompt() => "Take plate / Arrange ingredient";

        private static bool HasArrangeableItem(PlayerController player)
        {
            return player.Carry.Items.Count > 0 && !player.Carry.Plate.IsFull;
        }
    }
}
```

- [ ] **Step 6: Write CounterStation**

`Assets/Game/Gameplay/Station/CounterStation.cs`:
```csharp
using System.Collections.Generic;
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Temporary storage for up to 2 items. Place with one interact, take back with another.
    /// </summary>
    public sealed class CounterStation : StationBase
    {
        private readonly List<IngredientItem> _stored = new List<IngredientItem>(2);
        private const int MaxStored = 2;

        private void Start()
        {
            _stationName = "Counter";
        }

        public override bool CanInteract(PlayerController player)
        {
            return (_stored.Count < MaxStored && player.Carry.Items.Count > 0)
                || (_stored.Count > 0 && player.Carry.Items.Count < 4);
        }

        public override void Interact(PlayerController player)
        {
            // Take back first (if carrying space), else place
            if (_stored.Count > 0 && player.Carry.Items.Count == 0)
            {
                var item = _stored[_stored.Count - 1];
                _stored.RemoveAt(_stored.Count - 1);
                player.Carry.TryAdd(item);
                return;
            }

            if (_stored.Count < MaxStored && player.Carry.Items.Count > 0)
            {
                var item = player.Carry.Items[player.Carry.Items.Count - 1];
                player.Carry.Remove(item);
                _stored.Add(item);
            }
        }

        public override string GetPrompt() => $"Counter ({_stored.Count}/{MaxStored})";
    }
}
```

- [ ] **Step 7: Write ServingStation (depends on Task 5 MatchController)**

`Assets/Game/Gameplay/Station/ServingStation.cs`:
```csharp
using Playcenter;

namespace RecipeRage
{
    /// <summary>
    /// Validates the held plate against the current recipe and serves it.
    /// </summary>
    public sealed class ServingStation : StationBase
    {
        private IEventBus _eventBus;
        private MatchController _match;

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _match = ServiceLocator.Get<MatchController>();
            _stationName = "Serving Counter";
        }

        public override bool CanInteract(PlayerController player)
        {
            return player.Carry.HasPlate;
        }

        public override void Interact(PlayerController player)
        {
            var plate = player.Carry.ReleasePlate();
            if (_match.TryServePlate(plate))
            {
                _eventBus.Publish(new RecipeServedEvent(_match.CurrentRecipeId));
            }
            else
            {
                // Validation failed — hand the plate back so nothing is lost
                player.Carry.TakePlate(plate);
            }
        }

        public override string GetPrompt() => "Serve dish";
    }
}
```

- [ ] **Step 8: Write StationProgressView**

`Assets/Game/Gameplay/Cooking/StationProgressView.cs`:
```csharp
using UnityEngine;
using UnityEngine.UI;

namespace RecipeRage
{
    /// <summary>
    /// World-space progress bar above a CookingStation. Visible to ALL players.
    /// Yellow while cooking, red pulsing during burn grace. Off-screen mirroring
    /// is handled by OffScreenIndicator (Task 7).
    /// </summary>
    public sealed class StationProgressView : MonoBehaviour
    {
        [SerializeField] private CookingStation _station;
        [SerializeField] private Image _fillImage;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Color _cookingColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private Color _burnColor = new Color(1f, 0.2f, 0.1f);

        private void Update()
        {
            var active = _station.IsActive;
            _canvasGroup.alpha = active ? 1f : 0f;
            if (!active)
            {
                return;
            }

            _fillImage.fillAmount = _station.Progress01;
            _fillImage.color = _station.HasReadyItem ? _burnColor : _cookingColor;
        }
    }
}
```

- [ ] **Step 9: Verify compilation + commit**

```bash
git add Assets/Game/Gameplay
git commit -m "feat(gameplay): all stations (crate, cutting, cooking, plate, counter, serving) + plate + progress view"
```

---

### Task 5: Match Controller (Recipe List + Timer + Win Condition)

**Files:**
- Create: `Assets/Game/Gameplay/Match/MatchController.cs`
- Create: `Assets/Game/Gameplay/Match/MatchState.cs`

**Interfaces:**
- Consumes: `IRecipeCatalog`, `IConfigService`, `IEventBus`, `ITimeService`, `Plate`, gameplay events
- Produces:
  - `MatchController`: `.StartMatch(int seed)`, `.TryServePlate(Plate)` → bool, `.CurrentRecipe` → RecipeDefinition, `.CurrentRecipeId` → string, `.CompletedCount`, `.TotalCount`, `.RemainingSeconds`, `.IsMatchOver`, `event Action OnRecipeCompleted`, `event Action<bool> OnMatchEnded`

- [ ] **Step 1: Write MatchState + MatchController**

`Assets/Game/Gameplay/Match/MatchState.cs`:
```csharp
using System.Collections.Generic;

namespace RecipeRage
{
    public sealed class MatchState
    {
        public List<RecipeDefinition> RecipeList;
        public int CurrentIndex;
        public float RemainingSeconds;
        public bool IsOver;
    }
}
```

`Assets/Game/Gameplay/Match/MatchController.cs`:
```csharp
using System;
using System.Collections.Generic;
using Playcenter;
using Playcenter.Services;

namespace RecipeRage
{
    /// <summary>
    /// Runs one match: recipe list, countdown timer, plate validation, win condition.
    /// Single-player scope — enemy team is simulated at 0 for now; Slice 2 syncs real teams.
    /// </summary>
    public sealed class MatchController
    {
        private readonly IRecipeCatalog _catalog;
        private readonly IConfigService _config;
        private readonly IEventBus _eventBus;
        private readonly ITimeService _time;

        private MatchState _state;
        private float _matchDuration;

        public event Action OnRecipeCompleted;
        public event Action<bool> OnMatchEnded;

        public RecipeDefinition CurrentRecipe =>
            _state != null && _state.CurrentIndex < _state.RecipeList.Count
                ? _state.RecipeList[_state.CurrentIndex]
                : null;

        public string CurrentRecipeId => CurrentRecipe != null ? CurrentRecipe.Id : string.Empty;
        public int CompletedCount => _state?.CurrentIndex ?? 0;
        public int TotalCount => _state?.RecipeList.Count ?? 0;
        public float RemainingSeconds => _state?.RemainingSeconds ?? 0f;
        public bool IsMatchOver => _state?.IsOver ?? true;

        public MatchController(IRecipeCatalog catalog, IConfigService config, IEventBus eventBus, ITimeService time)
        {
            _catalog = catalog;
            _config = config;
            _eventBus = eventBus;
            _time = time;
        }

        public void StartMatch(int seed)
        {
            var easy = _config.Get(ConfigKeys.RecipesEasy2v2, ConfigKeys.Defaults.RecipesEasy2v2);
            var medium = _config.Get(ConfigKeys.RecipesMedium2v2, ConfigKeys.Defaults.RecipesMedium2v2);
            var hard = _config.Get(ConfigKeys.RecipesHard2v2, ConfigKeys.Defaults.RecipesHard2v2);
            _matchDuration = _config.Get(ConfigKeys.MatchDurationSec, ConfigKeys.Defaults.MatchDurationSec);

            _state = new MatchState
            {
                RecipeList = _catalog.GetRandomRecipeList(easy, medium, hard, seed),
                CurrentIndex = 0,
                RemainingSeconds = _matchDuration,
                IsOver = false
            };

            _eventBus.Publish(new MatchStartedEvent());
        }

        public void Tick()
        {
            if (_state == null || _state.IsOver)
            {
                return;
            }

            _state.RemainingSeconds -= _time.DeltaTime;
            if (_state.RemainingSeconds <= 0f)
            {
                EndMatch(false);
            }
        }

        public bool TryServePlate(Plate plate)
        {
            if (_state == null || _state.IsOver || CurrentRecipe == null)
            {
                return false;
            }

            if (!ValidatePlate(plate, CurrentRecipe))
            {
                return false;
            }

            _state.CurrentIndex++;
            OnRecipeCompleted?.Invoke();

            if (_state.CurrentIndex >= _state.RecipeList.Count)
            {
                EndMatch(true);
            }
            return true;
        }

        private void EndMatch(bool completedAll)
        {
            _state.IsOver = true;
            _eventBus.Publish(new MatchEndedEvent(completedAll, _state.CurrentIndex, 0));
            OnMatchEnded?.Invoke(completedAll);
        }

        private static bool ValidatePlate(Plate plate, RecipeDefinition recipe)
        {
            var requirements = recipe.RequiredIngredients;
            if (plate.Contents.Count != requirements.Length)
            {
                return false;
            }

            var used = new bool[plate.Contents.Count];
            foreach (var requirement in requirements)
            {
                var matched = false;
                for (int i = 0; i < plate.Contents.Count; i++)
                {
                    if (used[i])
                    {
                        continue;
                    }

                    var item = plate.Contents[i];
                    if (item.Definition.Type != requirement.Type || item.IsBurnt)
                    {
                        continue;
                    }
                    if (requirement.RequiresChopped && !item.IsChopped)
                    {
                        continue;
                    }
                    if (requirement.RequiresCooked && !item.IsCooked)
                    {
                        continue;
                    }

                    used[i] = true;
                    matched = true;
                    break;
                }

                if (!matched)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
```

- [ ] **Step 2: Register in GameplayCompositionRoot**

Modify `Assets/Game/DI/GameplayCompositionRoot.cs` — inside `OnPlaycenterReady()`, after `_stateMachine` creation, add:

```csharp
            var recipeCatalog = new RecipeCatalog(_allRecipes);
            var matchController = new MatchController(
                recipeCatalog,
                ServiceLocator.Get<IConfigService>(),
                ServiceLocator.Get<IEventBus>(),
                ServiceLocator.Get<ITimeService>());

            ServiceLocator.Register<IRecipeCatalog>(recipeCatalog);
            ServiceLocator.Register(matchController);
```

And add the field + serialization to the class:

```csharp
        [Header("Content")]
        [SerializeField] private RecipeDefinition[] _allRecipes;
```

And in `Update()`, after `_stateMachine.Update(...)`:

```csharp
            if (ServiceLocator.TryGet<MatchController>(out var match))
            {
                match.Tick();
            }
```

- [ ] **Step 3: Verify compilation + commit**

```bash
git add Assets/Game
git commit -m "feat(gameplay): match controller (recipe list, timer, plate validation, win condition)"
```

---

### Task 6: Test Kitchen Scene + Playable Loop

**Files:**
- Create: `Assets/Scenes/TestKitchen.unity` (editor)
- Create: `Assets/Game/Gameplay/Match/TestMatchBootstrap.cs`

**Interfaces:**
- Consumes: everything from Tasks 1-5
- Produces: playable single-player kitchen (fetch → chop → cook → plate → serve → complete recipes)

- [ ] **Step 1: Write TestMatchBootstrap**

`Assets/Game/Gameplay/Match/TestMatchBootstrap.cs`:
```csharp
using Playcenter;
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Dev bootstrap: starts a match on scene load. Removed when real flow lands in Slice 2.
    /// </summary>
    public sealed class TestMatchBootstrap : MonoBehaviour
    {
        private void Start()
        {
            ServiceLocator.Get<MatchController>().StartMatch(seed: 42);
        }
    }
}
```

- [ ] **Step 2: Build TestKitchen scene in editor**

`Assets/Scenes/TestKitchen.unity` layout (simple tutorial-style kitchen):
1. Floor plane + walls (top-down camera at (0, 12, -8), rotation (60, 0, 0))
2. Player: capsule + CharacterController + `PlayerController`
3. Stations (cubes with trigger colliders):
   - 2× `IngredientCrate` (assign Tomato, Onion defs)
   - 2× `CuttingStation`
   - 2× `CookingStation` (+ world-space canvas with `StationProgressView`)
   - 1× `PlateStation`
   - 1× `CounterStation`
   - 1× `ServingStation`
4. `TestMatchBootstrap` GameObject
5. Assign `_allRecipes` on `GameplayCompositionRoot` in Boot scene
6. Add TestKitchen to Build Settings

- [ ] **Step 3: Verify — play in editor**

Walkthrough: fetch tomato → chop (8 right-clicks) → cook (wait 12s) → collect → fetch onion → chop → cook → collect → take plate → arrange both → serve → recipe completes → next recipe appears. Match ends after timer or all recipes done.

Expected console: `[Analytics] match events`, `IngredientChoppedEvent` → (audio stubs silent until clips assigned).

- [ ] **Step 4: Commit**

```bash
git add Assets/Scenes Assets/Game
git commit -m "feat(gameplay): test kitchen scene — playable single-player loop"
```

---

### Task 7: Off-Screen Indicators

**Files:**
- Create: `Assets/Game/Gameplay/Indicators/OffScreenIndicator.cs`
- Create: `Assets/Game/Gameplay/Indicators/OffScreenIndicatorController.cs`

**Interfaces:**
- Consumes: `CookingStation` registry (found via scene scan at match start — replaced by MatchRuntimeRegistry in Slice 2), Camera.main
- Produces:
  - `OffScreenIndicatorController` — tracks active CookingStations, spawns HUD-edge indicators with direction arrow + icon + mirrored progress bar; pulsing red when burning; stacks vertically per edge

- [ ] **Step 1: Write OffScreenIndicator**

`Assets/Game/Gameplay/Indicators/OffScreenIndicator.cs`:
```csharp
using UnityEngine;
using UnityEngine.UI;

namespace RecipeRage
{
    /// <summary>
    /// One HUD-edge indicator: direction arrow, status icon, mirrored progress bar.
    /// Yellow/orange while cooking; pulsing red when burn grace is draining.
    /// </summary>
    public sealed class OffScreenIndicator : MonoBehaviour
    {
        [SerializeField] private RectTransform _arrow;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _progressFill;
        [SerializeField] private Color _cookingColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private Color _burnColor = new Color(1f, 0.2f, 0.1f);
        [SerializeField] private float _pulseSpeed = 6f;

        private CookingStation _station;

        public CookingStation Station => _station;

        public void Bind(CookingStation station)
        {
            _station = station;
        }

        private void Update()
        {
            if (_station == null || !_station.IsActive)
            {
                return;
            }

            _progressFill.fillAmount = _station.Progress01;

            if (_station.HasReadyItem) // burn grace draining = urgent
            {
                var pulse = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) * 0.5f;
                _icon.color = Color.Lerp(_burnColor, Color.white, pulse * 0.5f);
                _progressFill.color = _burnColor;
            }
            else
            {
                _icon.color = _cookingColor;
                _progressFill.color = _cookingColor;
            }
        }

        public void SetEdgePosition(Vector2 anchoredPosition, float rotationZ)
        {
            ((RectTransform)transform).anchoredPosition = anchoredPosition;
            _arrow.localEulerAngles = new Vector3(0f, 0f, rotationZ);
        }
    }
}
```

- [ ] **Step 2: Write OffScreenIndicatorController**

`Assets/Game/Gameplay/Indicators/OffScreenIndicatorController.cs`:
```csharp
using System.Collections.Generic;
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Mirrors off-screen station progress onto HUD edges. For each active
    /// CookingStation outside the camera view, places an indicator on the
    /// nearest screen edge with an arrow pointing toward it. Indicators on the
    /// same edge stack vertically.
    /// </summary>
    public sealed class OffScreenIndicatorController : MonoBehaviour
    {
        [SerializeField] private OffScreenIndicator _indicatorPrefab;
        [SerializeField] private RectTransform _indicatorRoot;
        [SerializeField] private float _edgePadding = 60f;
        [SerializeField] private float _stackSpacing = 56f;

        private readonly Dictionary<CookingStation, OffScreenIndicator> _active =
            new Dictionary<CookingStation, OffScreenIndicator>();

        private CookingStation[] _stations;
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
            _stations = FindObjectsOfType<CookingStation>(); // Slice 2: MatchRuntimeRegistry
        }

        private void LateUpdate()
        {
            var edgeCounts = new Dictionary<int, int>();

            foreach (var station in _stations)
            {
                var shouldShow = station.IsActive && !IsOnScreen(station.transform.position);

                if (shouldShow && !_active.ContainsKey(station))
                {
                    var indicator = Instantiate(_indicatorPrefab, _indicatorRoot);
                    indicator.Bind(station);
                    _active.Add(station, indicator);
                }
                else if (!shouldShow && _active.TryGetValue(station, out var stale))
                {
                    Destroy(stale.gameObject);
                    _active.Remove(station);
                }
            }

            foreach (var kvp in _active)
            {
                var viewport = _camera.WorldToViewportPoint(kvp.Key.transform.position);
                var edge = GetEdge(viewport, out var rotation);
                var stackIndex = edgeCounts.TryGetValue(edge, out var count) ? count : 0;
                edgeCounts[edge] = stackIndex + 1;

                var pos = GetEdgePosition(edge, stackIndex);
                kvp.Value.SetEdgePosition(pos, rotation);
            }
        }

        private bool IsOnScreen(Vector3 worldPosition)
        {
            var viewport = _camera.WorldToViewportPoint(worldPosition);
            return viewport.z > 0f
                && viewport.x >= 0f && viewport.x <= 1f
                && viewport.y >= 0f && viewport.y <= 1f;
        }

        private int GetEdge(Vector3 viewport, out float rotationZ)
        {
            // 0=left, 1=right, 2=top, 3=bottom
            if (viewport.x < 0f) { rotationZ = 180f; return 0; }
            if (viewport.x > 1f) { rotationZ = 0f; return 1; }
            if (viewport.y > 1f) { rotationZ = 90f; return 2; }
            rotationZ = -90f;
            return 3;
        }

        private Vector2 GetEdgePosition(int edge, int stackIndex)
        {
            var rect = _indicatorRoot.rect;
            var offset = _edgePadding + stackIndex * _stackSpacing;
            return edge switch
            {
                0 => new Vector2(-rect.width / 2f + _edgePadding, -rect.height / 2f + offset),
                1 => new Vector2(rect.width / 2f - _edgePadding, -rect.height / 2f + offset),
                2 => new Vector2(-rect.width / 2f + offset, rect.height / 2f - _edgePadding),
                _ => new Vector2(-rect.width / 2f + offset, -rect.height / 2f + _edgePadding),
            };
        }
    }
}
```

- [ ] **Step 3: Build indicator prefab + HUD canvas in TestKitchen**

1. HUD Canvas (screen space overlay) → `_indicatorRoot` RectTransform
2. `OffScreenIndicator` prefab: arrow (rotated triangle image), icon, progress bar
3. Add `OffScreenIndicatorController` to HUD canvas, wire references

- [ ] **Step 4: Verify — cook something, walk away until stove is off-screen**

Expected: indicator appears on the correct edge with arrow pointing at the stove, progress mirrors the station bar; when burn grace starts it pulses red; collecting the item removes the indicator.

- [ ] **Step 5: Commit**

```bash
git add Assets/Game/Gameplay/Indicators Assets/Scenes
git commit -m "feat(gameplay): off-screen station progress indicators (arrow + icon + progress)"
```

---

### Task 8: Tutorial Map (Forced First Launch)

**Files:**
- Create: `Assets/Game/Gameplay/Tutorial/TutorialStep.cs`
- Create: `Assets/Game/Gameplay/Tutorial/TutorialController.cs`
- Create: `Assets/Game/Gameplay/Tutorial/TutorialState.cs` (state machine state)
- Create: `Assets/Scenes/Tutorial.unity` (editor)

**Interfaces:**
- Consumes: `IGameStateMachine`, `ISaveService` (`tutorial_completed` key), `MatchController`, player + stations
- Produces:
  - `TutorialController` — sequential guided steps (move, fetch, chop, cook, plate, arrange, serve, burn warning), highlight arrows + text prompts, no time limit, can't fail (burnt items reset)
  - `TutorialState : IGameState` — entered on first launch when `tutorial_completed == false`; marks complete and transitions to MainMenuState on finish

- [ ] **Step 1: Write TutorialStep**

`Assets/Game/Gameplay/Tutorial/TutorialStep.cs`:
```csharp
using System;
using UnityEngine;

namespace RecipeRage
{
    [Serializable]
    public sealed class TutorialStep
    {
        public string Instruction;
        public Transform HighlightTarget;
        public TutorialCondition Condition;
    }

    public enum TutorialCondition
    {
        MovedDistance,
        FetchedIngredient,
        ChoppedIngredient,
        CookingStarted,
        CookingCollected,
        PlateTaken,
        IngredientPlated,
        RecipeServed,
        BurnWarningShown
    }
}
```

- [ ] **Step 2: Write TutorialController**

`Assets/Game/Gameplay/Tutorial/TutorialController.cs`:
```csharp
using System;
using Playcenter;
using Playcenter.Services;
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Guided first-launch tutorial. Steps advance on gameplay events. No timer,
    /// no failure — burnt items are cleared and the step retries.
    /// </summary>
    public sealed class TutorialController : MonoBehaviour
    {
        [SerializeField] private TutorialStep[] _steps;
        [SerializeField] private GameObject _highlightArrow;
        [SerializeField] private TMPro.TextMeshProUGUI _instructionLabel;

        private int _currentStep;
        private IEventBus _eventBus;
        private Vector3 _playerStartPosition;
        private PlayerController _player;

        public event Action OnTutorialCompleted;

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _player = FindObjectOfType<PlayerController>(); // tutorial scene: single player
            _playerStartPosition = _player.transform.position;

            SubscribeEvents();
            ShowStep(0);
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _eventBus.Subscribe<IngredientFetchedEvent>(OnFetched);
            _eventBus.Subscribe<IngredientChoppedEvent>(OnChopped);
            _eventBus.Subscribe<CookingStartedEvent>(OnCookingStarted);
            _eventBus.Subscribe<CookingCompletedEvent>(OnCookingCompleted);
            _eventBus.Subscribe<PlateTakenEvent>(OnPlateTaken);
            _eventBus.Subscribe<IngredientPlatedEvent>(OnPlated);
            _eventBus.Subscribe<RecipeServedEvent>(OnServed);
            _eventBus.Subscribe<IngredientBurntEvent>(OnBurnt);
        }

        private void UnsubscribeEvents()
        {
            _eventBus.Unsubscribe<IngredientFetchedEvent>(OnFetched);
            _eventBus.Unsubscribe<IngredientChoppedEvent>(OnChopped);
            _eventBus.Unsubscribe<CookingStartedEvent>(OnCookingStarted);
            _eventBus.Unsubscribe<CookingCompletedEvent>(OnCookingCompleted);
            _eventBus.Unsubscribe<PlateTakenEvent>(OnPlateTaken);
            _eventBus.Unsubscribe<IngredientPlatedEvent>(OnPlated);
            _eventBus.Unsubscribe<RecipeServedEvent>(OnServed);
            _eventBus.Unsubscribe<IngredientBurntEvent>(OnBurnt);
        }

        private void Update()
        {
            if (Current == null)
            {
                return;
            }

            if (Current.Condition == TutorialCondition.MovedDistance
                && Vector3.Distance(_player.transform.position, _playerStartPosition) > 2f)
            {
                Advance();
            }
        }

        private TutorialStep Current =>
            _currentStep < _steps.Length ? _steps[_currentStep] : null;

        private void ShowStep(int index)
        {
            _currentStep = index;
            if (Current == null)
            {
                _instructionLabel.text = "You're ready. Let's cook!";
                _highlightArrow.SetActive(false);
                OnTutorialCompleted?.Invoke();
                return;
            }

            _instructionLabel.text = Current.Instruction;
            _highlightArrow.SetActive(Current.HighlightTarget != null);
            if (Current.HighlightTarget != null)
            {
                _highlightArrow.transform.position = Current.HighlightTarget.position + Vector3.up * 2f;
            }
        }

        private void Advance() => ShowStep(_currentStep + 1);

        private void AdvanceIf(TutorialCondition condition)
        {
            if (Current != null && Current.Condition == condition)
            {
                Advance();
            }
        }

        private void OnFetched(IngredientFetchedEvent e) => AdvanceIf(TutorialCondition.FetchedIngredient);
        private void OnChopped(IngredientChoppedEvent e) => AdvanceIf(TutorialCondition.ChoppedIngredient);
        private void OnCookingStarted(CookingStartedEvent e) => AdvanceIf(TutorialCondition.CookingStarted);
        private void OnCookingCompleted(CookingCompletedEvent e) => AdvanceIf(TutorialCondition.CookingCollected);
        private void OnPlateTaken(PlateTakenEvent e) => AdvanceIf(TutorialCondition.PlateTaken);
        private void OnPlated(IngredientPlatedEvent e) => AdvanceIf(TutorialCondition.IngredientPlated);
        private void OnServed(RecipeServedEvent e) => AdvanceIf(TutorialCondition.RecipeServed);
        private void OnBurnt(IngredientBurntEvent e) => AdvanceIf(TutorialCondition.BurnWarningShown);
    }
}
```

- [ ] **Step 3: Write TutorialState + first-launch gate**

`Assets/Game/Gameplay/Tutorial/TutorialState.cs`:
```csharp
using Playcenter;
using Playcenter.Services;
using UnityEngine.SceneManagement;

namespace RecipeRage
{
    /// <summary>
    /// Loads the tutorial scene, waits for completion, marks tutorial_completed,
    /// then returns to the main menu.
    /// </summary>
    public sealed class TutorialState : IGameState
    {
        public void Enter()
        {
            SceneManager.LoadSceneAsync("Tutorial");
            // TutorialController.OnTutorialCompleted is wired to CompleteTutorial
            // via a scene bridge in the Tutorial scene (TutorialSceneBridge below).
        }

        public void Exit() { }
        public void Update(float deltaTime) { }

        public static void CompleteTutorial()
        {
            ServiceLocator.Get<ISaveService>().Save("tutorial_completed", true);
            SceneManager.LoadSceneAsync("Boot");
            ServiceLocator.Get<IGameStateMachine>().ChangeState(new MainMenuState());
        }
    }
}
```

Add the gate in `GameplayCompositionRoot.OnPlaycenterReady()` — replace the final `ChangeState(new MainMenuState())` with:

```csharp
            var tutorialDone = ServiceLocator.Get<ISaveService>().Load("tutorial_completed", false);
            _stateMachine.ChangeState(tutorialDone ? (IGameState)new MainMenuState() : new TutorialState());
```

- [ ] **Step 4: Build Tutorial scene in editor**

`Assets/Scenes/Tutorial.unity`: one of each station (crate/cutting/cooking/plate/serving), player, `TutorialController` with 10 steps wired (instructions from spec section "Tutorial Map"), highlight arrow prefab, instruction label. Bridge button/hook calls `TutorialState.CompleteTutorial()` on completion.

- [ ] **Step 5: Verify — delete save, launch game**

Expected: tutorial loads first; each step advances on the matching action; completing the last step marks `tutorial_completed`, returns to main menu. Relaunching skips the tutorial.

- [ ] **Step 6: Commit**

```bash
git add Assets/Game/Gameplay/Tutorial Assets/Scenes Assets/Game/DI
git commit -m "feat(gameplay): forced first-launch tutorial map (10 guided steps)"
```

---

## Self-Review Notes

- **Spec coverage:** fetch/chop/cook/plate/serve loop ✅, fixed-tap chopping ✅, autonomous cooking + burn (no score penalty) ✅, carry capacity 2 ✅, progress bars for ALL players ✅, off-screen indicators ✅, tutorial ✅, no points in HUD ✅ (HUD UI itself is Slice 5; events needed by HUD are published here).
- **Type consistency:** `Plate` is produced by `PlateStation` and consumed by `ServingStation`/`MatchController.TryServePlate` ✅; `MatchController.CurrentRecipeId` used by `ServingStation` ✅; `StationId` (`GetInstanceID`) used consistently across events/indicators (Slice 2 replaces with network ids) ✅.
- **Deferred items (explicit):** HUD recipe list/timer UI (Slice 5, consumes events published here), Marco's +1 carry ability (Slice 4), enemy team score in `MatchEndedEvent` (Slice 2 syncs), real chop-input scoping per player (Slice 2 multiplayer).

## Next Plan

`2026-07-25-reciperage-slice2-multiplayer.md` — NGO + EOS transport, NetworkPlayer/Station/Match, lobby/matchmaking, team compositions + countdown, EOS Cloud Storage wiring.
