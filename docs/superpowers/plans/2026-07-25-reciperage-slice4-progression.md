# Slice 4: Progression Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the full progression layer — 4 launch chefs (+2 "Coming Soon" slots), personal utility abilities, level 1→10 upgrades with Brawl Stars-style progressive coin costs, chef XP, trophy system (win +15 / loss -8), lobby chef selection wired into matchmaking, and EOS Cloud persistence of all progression.

**Architecture:** Game-specific logic lives in `Assets/Game/Progression/` and consumes SDK services (`IWalletService`, `ISaveService`, `IAnalyticsService`). Chef stats apply at gameplay construction time (PlayerController reads the selected chef's ability modifiers). All progression state is JSON-saved through the SDK save layer (EOS Cloud in production, local fallback in dev).

**Tech Stack:** Unity 6000.3.0f1, Playcenter SDK (Phase 0), Slice 1-3 gameplay/networking/bots.

## Global Constraints

- 4 chefs at launch: Gordon, Julia, Marco, Gustavo (+2 locked "Coming Soon" slots)
- Personal utility abilities ONLY — movement speed, pickup/drop speed, carry capacity, dash. No chop/cook speed effects
- Carry capacity default 2 for ALL chefs; Marco's ability adds +1 (L5) and +1 (L10)
- Upgrade costs: 100/200/400/700/1100/1700/2600/4000/6200 coins (17,000 total to max)
- Coins are earned and spent only, never lost. Trophies: win +15, loss -8
- Chef selection happens in Lobby before matchmaking (locked when queue starts)
- Progression persists via ISaveService (EOS Cloud transport from Slice 2)
- Requires Slice 3 complete

---

### Task 1: Chef Definitions + Ability Model

**Files:**
- Create: `Assets/Game/Progression/Chef/ChefId.cs`
- Create: `Assets/Game/Progression/Chef/ChefAbilityType.cs`
- Create: `Assets/Game/Progression/Chef/ChefDefinition.cs`
- Create: `Assets/Game/Progression/Chef/ChefAbilityModifier.cs`
- Create: `Assets/Game/Progression/Chef/ChefCatalog.cs`

**Interfaces:**
- Consumes: nothing new
- Produces:
  - `ChefId` enum: `Gordon, Julia, Marco, Gustavo, Locked5, Locked6`
  - `ChefAbilityType` enum: `MoveSpeed, PickupDropSpeed, CarryCapacity, Dash`
  - `ChefDefinition` (SO): `.Id`, `.DisplayName`, `.Rarity`, `.UnlockCost`, `.AbilityType`, `.AbilityPerLevel` (float[10]), `.Portrait`, `.ModelPrefab`
  - `ChefRarity` enum: `Common, Rare, Epic, Legendary`
  - `ChefAbilityModifier` (struct): `.MoveSpeedMultiplier`, `.PickupDropSpeedMultiplier`, `.CarryCapacityBonus`, `.HasDash`, `.DashCooldownSec`
  - `IChefCatalog.Get(ChefId)` → `ChefDefinition`, `.All` → IReadOnlyList, `.BuildModifier(ChefId, int level)` → `ChefAbilityModifier`

- [ ] **Step 1: Write enums + modifier**

`Assets/Game/Progression/Chef/ChefId.cs`:
```csharp
namespace RecipeRage
{
    public enum ChefId
    {
        Gordon = 0,
        Julia = 1,
        Marco = 2,
        Gustavo = 3,
        Locked5 = 4,  // "Coming Soon"
        Locked6 = 5   // "Coming Soon"
    }
}
```

`Assets/Game/Progression/Chef/ChefAbilityType.cs`:
```csharp
namespace RecipeRage
{
    public enum ChefAbilityType
    {
        MoveSpeed,          // Gordon: +1% per level (max +10%)
        PickupDropSpeed,    // Julia: +1.5% per level (max +15%)
        CarryCapacity,      // Marco: +1 at L5, +1 at L10 (max 4 items)
        Dash                // Gustavo: -2s cooldown per level (30s → 10s min)
    }

    public enum ChefRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }
}
```

`Assets/Game/Progression/Chef/ChefAbilityModifier.cs`:
```csharp
namespace RecipeRage
{
    /// <summary>
    /// Resolved ability values for a chef at a level. Personal utility ONLY —
    /// affects the owning player, never shared stations or cook/chop speed.
    /// </summary>
    public readonly struct ChefAbilityModifier
    {
        public float MoveSpeedMultiplier { get; }
        public float PickupDropSpeedMultiplier { get; }
        public int CarryCapacityBonus { get; }
        public bool HasDash { get; }
        public float DashCooldownSec { get; }

        public ChefAbilityModifier(
            float moveSpeedMultiplier,
            float pickupDropSpeedMultiplier,
            int carryCapacityBonus,
            bool hasDash,
            float dashCooldownSec)
        {
            MoveSpeedMultiplier = moveSpeedMultiplier;
            PickupDropSpeedMultiplier = pickupDropSpeedMultiplier;
            CarryCapacityBonus = carryCapacityBonus;
            HasDash = hasDash;
            DashCooldownSec = dashCooldownSec;
        }

        public static ChefAbilityModifier None => new ChefAbilityModifier(1f, 1f, 0, false, 0f);
    }
}
```

- [ ] **Step 2: Write ChefDefinition + ChefCatalog**

`Assets/Game/Progression/Chef/ChefDefinition.cs`:
```csharp
using UnityEngine;

namespace RecipeRage
{
    [CreateAssetMenu(fileName = "Chef", menuName = "RecipeRage/Chef Definition")]
    public sealed class ChefDefinition : ScriptableObject
    {
        [SerializeField] private ChefId _id;
        [SerializeField] private string _displayName;
        [SerializeField] private ChefRarity _rarity;
        [SerializeField] private int _unlockCost;
        [SerializeField] private ChefAbilityType _abilityType;
        [Tooltip("Ability value per level (index 0 = level 1). Interpretation depends on ability type.")]
        [SerializeField] private float[] _abilityPerLevel = new float[10];
        [SerializeField] private Sprite _portrait;
        [SerializeField] private GameObject _modelPrefab;

        public ChefId Id => _id;
        public string DisplayName => _displayName;
        public ChefRarity Rarity => _rarity;
        public int UnlockCost => _unlockCost;
        public ChefAbilityType AbilityType => _abilityType;
        public float[] AbilityPerLevel => _abilityPerLevel;
        public Sprite Portrait => _portrait;
        public GameObject ModelPrefab => _modelPrefab;
    }
}
```

`Assets/Game/Progression/Chef/ChefCatalog.cs`:
```csharp
using System.Collections.Generic;

namespace RecipeRage
{
    public interface IChefCatalog
    {
        IReadOnlyList<ChefDefinition> All { get; }
        ChefDefinition Get(ChefId id);
        ChefAbilityModifier BuildModifier(ChefId id, int level);
    }

    public sealed class ChefCatalog : IChefCatalog
    {
        private readonly Dictionary<ChefId, ChefDefinition> _byId = new Dictionary<ChefId, ChefDefinition>(8);

        public IReadOnlyList<ChefDefinition> All { get; }

        public ChefCatalog(ChefDefinition[] chefs)
        {
            All = chefs;
            foreach (var chef in chefs)
            {
                _byId[chef.Id] = chef;
            }
        }

        public ChefDefinition Get(ChefId id) => _byId.TryGetValue(id, out var chef) ? chef : null;

        public ChefAbilityModifier BuildModifier(ChefId id, int level)
        {
            var chef = Get(id);
            if (chef == null)
            {
                return ChefAbilityModifier.None;
            }

            var index = Mathf_Clamp(level - 1, 0, chef.AbilityPerLevel.Length - 1);
            var value = chef.AbilityPerLevel[index];

            return chef.AbilityType switch
            {
                ChefAbilityType.MoveSpeed => new ChefAbilityModifier(1f + value, 1f, 0, false, 0f),
                ChefAbilityType.PickupDropSpeed => new ChefAbilityModifier(1f, 1f + value, 0, false, 0f),
                ChefAbilityType.CarryCapacity => new ChefAbilityModifier(1f, 1f, (int)value, false, 0f),
                ChefAbilityType.Dash => new ChefAbilityModifier(1f, 1f, 0, true, value),
                _ => ChefAbilityModifier.None,
            };
        }

        private static int Mathf_Clamp(int value, int min, int max)
        {
            return value < min ? min : value > max ? max : value;
        }
    }
}
```

- [ ] **Step 3: Create 4 chef assets + 2 locked slots**

In the editor, under `Assets/Art/Data/Chefs/`:
- **Gordon** (Common, cost 0): MoveSpeed, per-level `0.01 … 0.10`
- **Julia** (Common, cost 0): PickupDropSpeed, per-level `0.015 … 0.15`
- **Marco** (Rare, cost 500): CarryCapacity, per-level `0,0,0,0,1,1,1,1,1,2`
- **Gustavo** (Epic, cost 2000): Dash, per-level `30,28,26,24,22,20,18,16,14,10`
- **Locked5 / Locked6** placeholder assets (rarity Rare/Legendary, "Coming Soon", no prefab)

- [ ] **Step 4: Verify compilation + commit**

```bash
git add Assets/Game/Progression Assets/Art/Data/Chefs
git commit -m "feat(progression): chef definitions + ability model (4 chefs + 2 locked)"
```

---

### Task 2: Chef Progression Service (Unlock / Upgrade / XP / Level)

**Files:**
- Create: `Assets/Game/Progression/Chef/ChefProgressionService.cs`
- Create: `Assets/Game/Progression/Chef/ChefProgressData.cs`
- Create: `Assets/Game/Progression/Chef/ChefUpgradeCosts.cs`

**Interfaces:**
- Consumes: `IWalletService`, `ISaveService`, `IAnalyticsService`, `IChefCatalog`
- Produces:
  - `IChefProgressionService.IsUnlocked(ChefId)` → bool, `.TryUnlock(ChefId)` → bool, `.GetLevel(ChefId)` → int, `.GetXp(ChefId)` → int, `.GetUpgradeCost(ChefId)` → int, `.TryUpgrade(ChefId)` → bool, `.AddXp(ChefId, int)`, `.GetSelectedChef()` → ChefId, `.SelectChef(ChefId)`, `event Action<ChefId> OnChefUnlocked / OnChefUpgraded / OnChefSelected`
  - `ChefUpgradeCosts.ForLevel(int currentLevel)` → int (static table per spec)

- [ ] **Step 1: Write upgrade cost table**

`Assets/Game/Progression/Chef/ChefUpgradeCosts.cs`:
```csharp
namespace RecipeRage
{
    /// <summary>
    /// Brawl Stars-style progressive costs. Total to max one chef: 17,000 coins.
    /// </summary>
    public static class ChefUpgradeCosts
    {
        private static readonly int[] Costs = { 100, 200, 400, 700, 1100, 1700, 2600, 4000, 6200 };

        public const int MaxLevel = 10;

        public static int ForLevel(int currentLevel)
        {
            if (currentLevel < 1 || currentLevel >= MaxLevel)
            {
                return 0;
            }
            return Costs[currentLevel - 1];
        }
    }
}
```

- [ ] **Step 2: Write progress data**

`Assets/Game/Progression/Chef/ChefProgressData.cs`:
```csharp
using System;
using System.Collections.Generic;

namespace RecipeRage
{
    [Serializable]
    public sealed class ChefProgressData
    {
        public List<ChefProgressEntry> Chefs = new List<ChefProgressEntry>();
        public int SelectedChefId;
    }

    [Serializable]
    public sealed class ChefProgressEntry
    {
        public int ChefId;
        public bool Unlocked;
        public int Level = 1;
        public int Xp;
    }
}
```

- [ ] **Step 3: Write ChefProgressionService**

`Assets/Game/Progression/Chef/ChefProgressionService.cs`:
```csharp
using System;
using System.Collections.Generic;
using Playcenter.Services;

namespace RecipeRage
{
    public interface IChefProgressionService
    {
        event Action<ChefId> OnChefUnlocked;
        event Action<ChefId> OnChefUpgraded;
        event Action<ChefId> OnChefSelected;

        bool IsUnlocked(ChefId id);
        bool TryUnlock(ChefId id);
        int GetLevel(ChefId id);
        int GetXp(ChefId id);
        int GetUpgradeCost(ChefId id);
        bool TryUpgrade(ChefId id);
        void AddXp(ChefId id, int xp);
        ChefId GetSelectedChef();
        void SelectChef(ChefId id);
        ChefAbilityModifier GetSelectedModifier();
    }

    /// <summary>
    /// Game-specific chef progression. Uses SDK wallet/save/analytics — the SDK
    /// knows coins exist, but knows nothing about chefs.
    /// </summary>
    public sealed class ChefProgressionService : IChefProgressionService
    {
        private const string SaveKey = "chef_progress";

        private readonly IChefCatalog _catalog;
        private readonly IWalletService _wallet;
        private readonly ISaveService _save;
        private readonly IAnalyticsService _analytics;
        private readonly ChefProgressData _data;

        public event Action<ChefId> OnChefUnlocked;
        public event Action<ChefId> OnChefUpgraded;
        public event Action<ChefId> OnChefSelected;

        public ChefProgressionService(
            IChefCatalog catalog,
            IWalletService wallet,
            ISaveService save,
            IAnalyticsService analytics)
        {
            _catalog = catalog;
            _wallet = wallet;
            _save = save;
            _analytics = analytics;
            _data = _save.Load(SaveKey, new ChefProgressData());

            // Starters are always unlocked
            EnsureUnlocked(ChefId.Gordon);
            EnsureUnlocked(ChefId.Julia);
        }

        public bool IsUnlocked(ChefId id) => GetEntry(id)?.Unlocked ?? false;

        public bool TryUnlock(ChefId id)
        {
            var chef = _catalog.Get(id);
            if (chef == null || IsUnlocked(id))
            {
                return false;
            }

            if (!_wallet.TrySpendCoins(chef.UnlockCost))
            {
                return false;
            }

            var entry = GetOrCreateEntry(id);
            entry.Unlocked = true;
            Persist();

            _analytics.TrackEvent("chef_unlocked", new Dictionary<string, object>
            {
                { "chefId", id.ToString() },
                { "cost", chef.UnlockCost }
            });
            OnChefUnlocked?.Invoke(id);
            return true;
        }

        public int GetLevel(ChefId id) => GetEntry(id)?.Level ?? 1;
        public int GetXp(ChefId id) => GetEntry(id)?.Xp ?? 0;

        public int GetUpgradeCost(ChefId id) => ChefUpgradeCosts.ForLevel(GetLevel(id));

        public bool TryUpgrade(ChefId id)
        {
            if (!IsUnlocked(id))
            {
                return false;
            }

            var level = GetLevel(id);
            if (level >= ChefUpgradeCosts.MaxLevel)
            {
                return false;
            }

            var cost = ChefUpgradeCosts.ForLevel(level);
            if (!_wallet.TrySpendCoins(cost))
            {
                return false;
            }

            GetOrCreateEntry(id).Level = level + 1;
            Persist();

            _analytics.TrackEvent("chef_upgraded", new Dictionary<string, object>
            {
                { "chefId", id.ToString() },
                { "level", level + 1 },
                { "cost", cost }
            });
            OnChefUpgraded?.Invoke(id);
            return true;
        }

        public void AddXp(ChefId id, int xp)
        {
            if (xp <= 0 || !IsUnlocked(id))
            {
                return;
            }
            GetOrCreateEntry(id).Xp += xp;
            Persist();
        }

        public ChefId GetSelectedChef() => (ChefId)_data.SelectedChefId;

        public void SelectChef(ChefId id)
        {
            if (!IsUnlocked(id))
            {
                return;
            }
            _data.SelectedChefId = (int)id;
            Persist();
            OnChefSelected?.Invoke(id);
        }

        public ChefAbilityModifier GetSelectedModifier()
        {
            var id = GetSelectedChef();
            return _catalog.BuildModifier(id, GetLevel(id));
        }

        private void EnsureUnlocked(ChefId id)
        {
            var entry = GetOrCreateEntry(id);
            if (!entry.Unlocked)
            {
                entry.Unlocked = true;
                Persist();
            }
        }

        private ChefProgressEntry GetEntry(ChefId id)
        {
            foreach (var entry in _data.Chefs)
            {
                if (entry.ChefId == (int)id)
                {
                    return entry;
                }
            }
            return null;
        }

        private ChefProgressEntry GetOrCreateEntry(ChefId id)
        {
            var entry = GetEntry(id);
            if (entry == null)
            {
                entry = new ChefProgressEntry { ChefId = (int)id };
                _data.Chefs.Add(entry);
            }
            return entry;
        }

        private void Persist()
        {
            _save.Save(SaveKey, _data);
        }
    }
}
```

- [ ] **Step 4: Register in GameplayCompositionRoot**

In `OnPlaycenterReady()`:

```csharp
            var chefCatalog = new ChefCatalog(_allChefs);
            var chefProgression = new ChefProgressionService(
                chefCatalog,
                ServiceLocator.Get<IWalletService>(),
                ServiceLocator.Get<ISaveService>(),
                ServiceLocator.Get<IAnalyticsService>());
            ServiceLocator.Register<IChefCatalog>(chefCatalog);
            ServiceLocator.Register<IChefProgressionService>(chefProgression);
```

Add field: `[SerializeField] private ChefDefinition[] _allChefs;` and assign the 6 assets in the Boot scene.

- [ ] **Step 5: Verify — unlock Marco with stub coins, upgrade Gordon, select chef, restart, state persists**

- [ ] **Step 6: Commit**

```bash
git add Assets/Game/Progression Assets/Game/DI
git commit -m "feat(progression): chef progression service (unlock/upgrade/XP/select, Brawl Stars costs)"
```

---

### Task 3: Apply Abilities to Gameplay (Player + Dash)

**Files:**
- Modify: `Assets/Game/Gameplay/Player/PlayerController.cs`
- Modify: `Assets/Game/Gameplay/Player/PlayerCarry.cs`
- Create: `Assets/Game/Gameplay/Player/DashAbility.cs`

**Interfaces:**
- Consumes: `IChefProgressionService.GetSelectedModifier()`, `ChefAbilityModifier`
- Produces:
  - `PlayerController.ApplyChefModifier(ChefAbilityModifier)` — adjusts move speed, carry capacity, enables dash
  - `DashAbility` — triggered by double-tap right stick / dedicated button (config), 3m dash, cooldown per chef level

- [ ] **Step 1: Apply modifier in PlayerController**

Modify `Assets/Game/Gameplay/Player/PlayerController.cs` — add:

```csharp
        private ChefAbilityModifier _chefModifier = ChefAbilityModifier.None;
        private DashAbility _dash;

        /// <summary>Called at match start with the player's selected chef modifier.</summary>
        public void ApplyChefModifier(ChefAbilityModifier modifier)
        {
            _chefModifier = modifier;
            _moveSpeed = _baseMoveSpeed * modifier.MoveSpeedMultiplier;
            Carry.SetCapacityBonus(modifier.CarryCapacityBonus);

            if (modifier.HasDash)
            {
                _dash = new DashAbility(_characterController, modifier.DashCooldownSec);
            }
        }
```

Rename `_moveSpeed` assignment in `Start()` to `_baseMoveSpeed` and call `ApplyChefModifier(ChefAbilityModifier.None)` as default.

- [ ] **Step 2: Capacity bonus in PlayerCarry**

Modify `Assets/Game/Gameplay/Player/PlayerCarry.cs`:

```csharp
        private int _capacityBonus;

        public void SetCapacityBonus(int bonus)
        {
            _capacityBonus = bonus;
        }

        public bool TryAdd(IngredientItem item)
        {
            if (_items.Count >= _capacity + _capacityBonus)
            {
                return false;
            }
            _items.Add(item);
            return true;
        }
```

(Make `_capacity` a stored field from constructor; adjust TryAdd accordingly.)

- [ ] **Step 3: Write DashAbility**

`Assets/Game/Gameplay/Player/DashAbility.cs`:
```csharp
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Gustavo's active ability: 3m burst in move direction, cooldown per chef
    /// level (30s → 10s at L10). Usable once per cooldown; input: double-tap
    /// the move stick direction (mobile-friendly, no extra button).
    /// </summary>
    public sealed class DashAbility
    {
        private const float DashDistance = 3f;
        private const float DashDuration = 0.15f;
        private const float DoubleTapWindow = 0.3f;

        private readonly CharacterController _characterController;
        private readonly float _cooldownSec;

        private float _cooldownRemaining;
        private float _dashRemaining;
        private Vector3 _dashDirection;
        private Vector2 _lastMoveAxis;
        private float _timeSinceLastTap = float.MaxValue;

        public bool IsReady => _cooldownRemaining <= 0f;
        public float Cooldown01 => _cooldownSec <= 0f ? 1f : 1f - _cooldownRemaining / _cooldownSec;

        public DashAbility(CharacterController characterController, float cooldownSec)
        {
            _characterController = characterController;
            _cooldownSec = cooldownSec;
        }

        public void Tick(Vector2 moveAxis, float deltaTime)
        {
            if (_cooldownRemaining > 0f)
            {
                _cooldownRemaining -= deltaTime;
            }

            _timeSinceLastTap += deltaTime;

            // Double-tap detection: stick released then pushed again within window
            if (moveAxis.sqrMagnitude < 0.1f && _lastMoveAxis.sqrMagnitude > 0.5f)
            {
                if (_timeSinceLastTap < DoubleTapWindow && IsReady)
                {
                    StartDash(_lastMoveAxis);
                }
                _timeSinceLastTap = 0f;
            }
            _lastMoveAxis = moveAxis;

            if (_dashRemaining > 0f)
            {
                _dashRemaining -= deltaTime;
                _characterController.Move(_dashDirection * (DashDistance / DashDuration * deltaTime));
            }
        }

        private void StartDash(Vector2 direction)
        {
            _dashDirection = new Vector3(direction.x, 0f, direction.y).normalized;
            _dashRemaining = DashDuration;
            _cooldownRemaining = _cooldownSec;
        }
    }
}
```

Call `_dash?.Tick(_input.MoveAxis, Time.deltaTime)` from `PlayerController.Update()`.

- [ ] **Step 4: Apply modifier at match start**

In `TestMatchBootstrap` (Slice 1) and `NetworkPlayer.OnNetworkSpawn` (Slice 2):

```csharp
            var modifier = ServiceLocator.Get<IChefProgressionService>().GetSelectedModifier();
            GetComponent<PlayerController>().ApplyChefModifier(modifier);
```

- [ ] **Step 5: Verify — Gordon moves faster at L5 vs L1; Marco carries 3 at L5; Gustavo dashes with cooldown**

- [ ] **Step 6: Commit**

```bash
git add Assets/Game/Gameplay/Player Assets/Game/Network
git commit -m "feat(progression): apply chef abilities to gameplay (speed, capacity, dash)"
```

---

### Task 4: Trophy System (+15 / -8)

**Files:**
- Create: `Assets/Game/Progression/Trophy/TrophyService.cs`
- Create: `Assets/Game/Progression/Trophy/TrophyData.cs`

**Interfaces:**
- Consumes: `ISaveService`, `IAnalyticsService`, `MatchEndedEvent`
- Produces:
  - `ITrophyService.Trophies` → int, `.ApplyMatchResult(bool won)`, `event Action<int> OnTrophiesChanged`
  - Award on match end: win +15, loss -8 (floor 0)

- [ ] **Step 1: Write TrophyData + TrophyService**

`Assets/Game/Progression/Trophy/TrophyData.cs`:
```csharp
using System;

namespace RecipeRage
{
    [Serializable]
    public sealed class TrophyData
    {
        public int Trophies;
    }
}
```

`Assets/Game/Progression/Trophy/TrophyService.cs`:
```csharp
using System;
using System.Collections.Generic;
using Playcenter;
using Playcenter.Services;

namespace RecipeRage
{
    public interface ITrophyService
    {
        event Action<int> OnTrophiesChanged;
        int Trophies { get; }
        void ApplyMatchResult(bool won);
    }

    /// <summary>
    /// Brawl Stars-style trophies: win +15, loss -8 (floor 0). Separate from
    /// coins — coins are never lost, trophies rise and fall.
    /// </summary>
    public sealed class TrophyService : ITrophyService
    {
        private const string SaveKey = "trophies";
        private const int WinAmount = 15;
        private const int LossAmount = -8;

        private readonly ISaveService _save;
        private readonly IAnalyticsService _analytics;
        private readonly TrophyData _data;

        public event Action<int> OnTrophiesChanged;

        public int Trophies => _data.Trophies;

        public TrophyService(ISaveService save, IAnalyticsService analytics)
        {
            _save = save;
            _analytics = analytics;
            _data = _save.Load(SaveKey, new TrophyData());
        }

        public void ApplyMatchResult(bool won)
        {
            var delta = won ? WinAmount : LossAmount;
            _data.Trophies = Math.Max(0, _data.Trophies + delta);
            _save.Save(SaveKey, _data);

            _analytics.TrackEvent("trophies_changed", new Dictionary<string, object>
            {
                { "won", won },
                { "delta", delta },
                { "total", _data.Trophies }
            });
            OnTrophiesChanged?.Invoke(_data.Trophies);
        }
    }
}
```

- [ ] **Step 2: Wire to match end**

In `GameplayCompositionRoot.OnPlaycenterReady()`:

```csharp
            var trophyService = new TrophyService(
                ServiceLocator.Get<ISaveService>(),
                ServiceLocator.Get<IAnalyticsService>());
            ServiceLocator.Register<ITrophyService>(trophyService);

            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus.Subscribe<MatchEndedEvent>(e =>
            {
                trophyService.ApplyMatchResult(e.Won);

                // Match rewards: coins + chef XP (spec: 50 win / 20 loss + 5 per recipe)
                var wallet = ServiceLocator.Get<IWalletService>();
                var coins = e.Won ? 50 : 20;
                coins += e.TeamRecipes * 5;
                wallet.AddCoins(coins);

                var progression = ServiceLocator.Get<IChefProgressionService>();
                progression.AddXp(progression.GetSelectedChef(), 25);
            });
```

- [ ] **Step 3: Verify — win adds 15 trophies + coins + XP; loss removes 8 (floor 0); persists across restart**

- [ ] **Step 4: Commit**

```bash
git add Assets/Game/Progression Assets/Game/DI
git commit -m "feat(progression): trophy system (+15/-8, floor 0) + match reward wiring"
```

---

### Task 5: Lobby Chef Selection (Pre-Matchmaking)

**Files:**
- Create: `Assets/Game/Network/LobbyState.cs`
- Create: `Assets/Game/Network/ChefSelectionSync.cs`
- Modify: `Assets/Game/Network/NetworkTeamRoster.cs`
- Modify: `Assets/Game/Network/MatchmakingController.cs`

**Interfaces:**
- Consumes: `IChefProgressionService`, `MatchmakingController`, `NetworkTeamRoster`
- Produces:
  - `LobbyState : IGameState` — chef grid + Play button (UI in Slice 5; state logic here). Play → locks chef → `QuickMatch(teamSize)` → TeamCompositionState
  - `ChefSelectionSync` — `NetworkVariable<int> SelectedChefId` per player, replicated into `NetworkTeamRoster` entries

- [ ] **Step 1: Write LobbyState**

`Assets/Game/Network/LobbyState.cs`:
```csharp
using Playcenter;
using Playcenter.Services;
using RecipeRage.Net;

namespace RecipeRage
{
    /// <summary>
    /// Pre-matchmaking lobby: player picks their chef (Brawl Stars-style),
    /// then taps Play → chef locks → matchmaking starts. There is NO separate
    /// pre-match chef select screen — the choice rides into the roster.
    /// </summary>
    public sealed class LobbyState : IGameState
    {
        private readonly int _teamSize;

        public LobbyState(int teamSize)
        {
            _teamSize = teamSize;
        }

        public void Enter()
        {
            ServiceLocator.Get<ILoggingService>().Log($"[Flow] Lobby entered (team {_teamSize})");
            // Slice 5 shows the chef grid UI here.
        }

        public void Exit() { }
        public void Update(float deltaTime) { }

        /// <summary>Called by the Play button (Slice 5 UI wires this).</summary>
        public void OnPlayPressed()
        {
            var progression = ServiceLocator.Get<IChefProgressionService>();
            var selected = progression.GetSelectedChef();
            ServiceLocator.Get<ILoggingService>().Log($"[Flow] Play pressed, chef locked: {selected}");

            var matchmaking = ServiceLocator.Get<MatchmakingController>();
            matchmaking.OnMatchFound += OnMatchFound;
            matchmaking.QuickMatch(_teamSize);
        }

        private void OnMatchFound()
        {
            var matchmaking = ServiceLocator.Get<MatchmakingController>();
            matchmaking.OnMatchFound -= OnMatchFound;
            ServiceLocator.Get<IGameStateMachine>().ChangeState(new TeamCompositionState());
        }
    }
}
```

- [ ] **Step 2: Sync chef selection into roster**

Modify `Assets/Game/Network/NetworkTeamRoster.cs` — in server spawn loop, replace `ChefId = 0` with the player's locked chef:

```csharp
                int chefId = 0;
                var playerObj = client.PlayerObject;
                if (playerObj != null)
                {
                    var selection = playerObj.GetComponent<ChefSelectionSync>();
                    if (selection != null)
                    {
                        chefId = selection.SelectedChefId.Value;
                    }
                }

                Players.Add(new PlayerRosterEntry
                {
                    ClientId = client.ClientId,
                    ChefId = chefId,
                    TeamId = index < teamSize ? 0 : 1
                });
```

Create `Assets/Game/Network/ChefSelectionSync.cs`:

```csharp
using Playcenter;
using Unity.Netcode;

namespace RecipeRage.Net
{
    /// <summary>
    /// Carries the player's locked chef choice into the match. Set on the client
    /// before connect; server reads it when building the roster.
    /// </summary>
    public sealed class ChefSelectionSync : NetworkBehaviour
    {
        public readonly NetworkVariable<int> SelectedChefId = new NetworkVariable<int>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                SelectedChefId.Value = (int)ServiceLocator.Get<IChefProgressionService>().GetSelectedChef();
            }
        }
    }
}
```

Add `ChefSelectionSync` to the NetworkPlayer prefab.

- [ ] **Step 3: Wire MainMenu → Lobby**

Modify `MainMenuState` (Slice 1 placeholder) — add a dev entry point:

```csharp
        public void Update(float deltaTime)
        {
            // Dev: press P to enter 2v2 lobby (Slice 5 replaces with Play button UI)
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.P))
            {
                ServiceLocator.Get<IGameStateMachine>().ChangeState(new RecipeRage.LobbyState(teamSize: 2));
            }
        }
```

- [ ] **Step 4: Verify — MainMenu → (P) → Lobby → Play → matchmaking → composition shows selected chef ids**

- [ ] **Step 5: Commit**

```bash
git add Assets/Game/Network Assets/Game/Gameplay
git commit -m "feat(progression): lobby chef selection synced into match roster"
```

---

### Task 6: Progression Persistence Verification (EOS Cloud Path)

**Files:**
- Modify: `Assets/Playcenter/Core/DI/PlaycenterCompositionRoot.cs`

**Interfaces:**
- Consumes: `EOSCloudSaveService.Preload` (Phase 0), all progression save keys
- Produces: progression survives app restart via cloud-first save path

- [ ] **Step 1: Preload progression keys after auth**

Modify `PlaycenterCompositionRoot.InitializeSDK()` — after `IAuthService` init:

```csharp
            yield return ServiceLocator.Get<IAuthService>().Initialize();

            if (ServiceLocator.Get<ISaveService>() is EOSCloudSaveService cloudSave)
            {
                yield return cloudSave.Preload(new[]
                {
                    "chef_progress", "trophies", "wallet_coins", "tutorial_completed", "friend_code"
                }).AsCoroutine();
            }
```

Add the `AsCoroutine` extension (Task extension in `Assets/Playcenter/Core/TaskExtensions.cs`):

```csharp
using System.Collections;
using System.Threading.Tasks;

namespace Playcenter
{
    public static class TaskExtensions
    {
        public static IEnumerator AsCoroutine(this Task task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }
        }
    }
}
```

- [ ] **Step 2: Verify — unlock/upgrade/trophies → quit → relaunch → all values restored**

- [ ] **Step 3: Commit**

```bash
git add Assets/Playcenter
git commit -m "feat(services): preload progression keys from cloud after auth"
```

---

## Self-Review Notes

- **Spec coverage:** 4 chefs + 2 locked ✅, personal utility abilities only ✅, carry capacity 2 default / Marco +1/+1 ✅, Brawl Stars costs (17,000 total) ✅, trophies +15/-8 ✅, coins never lost ✅, lobby chef selection (no pre-match screen) ✅, EOS Cloud persistence ✅, match rewards (50/20 + 5/recipe coins, 25 XP) ✅.
- **Type consistency:** `ChefAbilityModifier` built by `ChefCatalog.BuildModifier` and consumed by `PlayerController.ApplyChefModifier` ✅; `IChefProgressionService.GetSelectedChef()` used identically in `LobbyState`, `ChefSelectionSync`, match rewards ✅; `MatchEndedEvent` fields match Slice 1/2 definition ✅.
- **Deferred items (explicit):** Chef grid / lobby UI (Slice 5), chef XP level curve beyond XP accumulation (XP thresholds per level can ship with UI), "Coming Soon" chef 5/6 design (post-launch), trophy-based matchmaking brackets (needs player population data).

## Next Plan

`2026-07-25-reciperage-slice5-monetization-polish.md` — IAP catalog + purchase grants, rewarded ads, cosmetics, all UI Toolkit screens with premium animations, 3D chef showcase, friends UI, themed maps, audio content, HUD.
