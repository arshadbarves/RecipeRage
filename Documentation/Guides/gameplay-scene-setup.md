# RecipeRage Gameplay Scene Setup

This file is the source of truth for manual Unity scene, prefab, and asset setup related to gameplay runtime wiring.

Use this guide when checking or recreating the gameplay setup in `Assets/Scenes/Game.unity`.

## Status Labels

- `Must be assigned manually`: set this exact reference/value in the Inspector.
- `Intentionally left empty`: leave the field unset on purpose.
- `Auto-resolved at runtime`: the scene object may stay empty because code finds it at runtime.

## Game.unity

- Scene path: `Assets/Scenes/Game.unity`
- Root gameplay scene object used for gameplay runtime systems: `NetworkManagers`
- Runtime binder object used to publish scene references into match context: `MatchRuntimeSceneBinder`

## NetworkManagers Object

Scene object: `NetworkManagers`

This object contains the gameplay runtime network components used during the match.

### Required Components

| Component | Field | Required Value | Status |
| --- | --- | --- | --- |
| `NetworkObject` | Default component settings | Keep on object | Must be assigned manually |
| `IngredientNetworkSpawner` | `_ingredientPrefab` | `None` | Intentionally left empty |
| `RoundTimer` | no required serialized setup here | leave as-is | Must be assigned manually |
| `NetworkScoreManager` | no required serialized setup here | leave as-is | Must be assigned manually |
| `OrderManager` | `_minTimeBetweenOrders` | `5` | Must be assigned manually |
| `OrderManager` | `_maxTimeBetweenOrders` | `12` | Must be assigned manually |
| `OrderManager` | `_maxActiveOrders` | `3` | Must be assigned manually |
| `OrderManager` | `_difficultyScalingFactor` | `0.8` | Must be assigned manually |
| `OrderManager` | `_orderTimeLimit` | `120` | Must be assigned manually |
| `OrderManager` | `_baseOrderPoints` | `100` | Must be assigned manually |
| `OrderManager` | `_timeBonus` | `50` | Must be assigned manually |
| `OrderManager` | `_perfectBonus` | `25` | Must be assigned manually |
| `OrderManager` | `_availableRecipes` | `Recipe` assets with GUIDs `6a3b0d6ca40d4a6598d5a40d11110006` and `6a3b0d6ca40d4a6598d5a40d11110007` | Must be assigned manually |
| `ScoreManager` | `_baseOrderPoints` | `100` | Must be assigned manually |
| `ScoreManager` | `_timeBonus` | `50` | Must be assigned manually |
| `ScoreManager` | `_perfectBonus` | `25` | Must be assigned manually |
| `ScoreManager` | `_comboBonus` | `10` | Must be assigned manually |
| `ScoreManager` | `_comboTimeWindow` | `30` | Must be assigned manually |
| `ScoreManager` | `_orderManager` | `NetworkManagers/OrderManager` | Must be assigned manually |

### Ingredient Spawner Notes

- `IngredientNetworkSpawner._ingredientPrefab = None` is correct.
- The spawner is expected to use the prefab stored on each `Ingredient` asset.
- Do not assign a fallback ingredient prefab in the scene unless the architecture is intentionally changed.

## MatchRuntimeSceneBinder Object

Scene object: `MatchRuntimeSceneBinder`

This object publishes scene references into the app-level match context.

### Inspector Values

| Field | Required Value | Status |
| --- | --- | --- |
| `_orderManager` | `None` | Auto-resolved at runtime |
| `_scoreManager` | `None` | Auto-resolved at runtime |
| `_gamePhaseSync` | `None` | Auto-resolved at runtime |
| `_roundTimer` | `NetworkManagers/RoundTimer` | Must be assigned manually |
| `_networkScoreManager` | `NetworkManagers/NetworkScoreManager` | Must be assigned manually |
| `_mobileControlsManager` | `None` | Auto-resolved at runtime |
| `_spawnManager` | `SpawnManager` | Must be assigned manually |
| `_resolveMissingReferencesOnSceneLoad` | `true` | Must be assigned manually |

### Runtime Resolution Behavior

When `_resolveMissingReferencesOnSceneLoad` is `true`, the binder may leave these empty in the scene:

- `_orderManager`
- `_scoreManager`
- `_gamePhaseSync`
- `_mobileControlsManager`

These are found via scene lookup at runtime.

## Spawn Points

The current gameplay scene uses 8 team spawn points and 0 neutral spawn points.

### Team A

| Scene Object Name | `_teamCategory` |
| --- | --- |
| `SpawnPoint_TeamA` | `1` |
| `SpawnPoint_TeamA (1)` | `1` |
| `SpawnPoint_TeamA (2)` | `1` |
| `SpawnPoint_TeamA (3)` | `1` |

### Team B

| Scene Object Name | `_teamCategory` |
| --- | --- |
| `SpawnPoint_TeamB` | `2` |
| `SpawnPoint_TeamB (1)` | `2` |
| `SpawnPoint_TeamB (2)` | `2` |
| `SpawnPoint_TeamB (3)` | `2` |

### Counts

- `Team A = 4`
- `Team B = 4`
- `Neutral = 0`

## Network Prefabs Asset

Asset path: `Assets/DefaultNetworkPrefabs.asset`

The asset must include these gameplay prefabs:

| Prefab Path |
| --- |
| `Assets/Resources/Prefabs/Gameplay/Ingredient.prefab` |
| `Assets/Resources/Prefabs/Gameplay/Stations/IngredientCrate.prefab` |
| `Assets/Resources/Prefabs/Gameplay/Stations/PlateDispenser.prefab` |

Do not remove those entries. Ingredient spawning depends on the ingredient prefab being registered as a network prefab.

## Ingredient Assets and Prefab Wiring

Current ingredient assets verified in repo:

| Ingredient Asset | `_prefab` |
| --- | --- |
| `Assets/Resources/ScriptableObjects/Cooking/Ingredients/Tomato.asset` | `Assets/Resources/Prefabs/Gameplay/Ingredient.prefab` |
| `Assets/Resources/ScriptableObjects/Cooking/Ingredients/Steak.asset` | `Assets/Resources/Prefabs/Gameplay/Ingredient.prefab` |

### Rule

- Every gameplay ingredient asset must point to a network-registered prefab.
- The current shared prefab is `Assets/Resources/Prefabs/Gameplay/Ingredient.prefab`.

## DI and Runtime Wiring Notes

These are code-driven and should not be “fixed” in the scene:

- `IngredientNetworkSpawner` receives `INetworkObjectPool` and `INetworkGameManager` through `GameLifetimeScope`.
- Those services are root/app scoped, not session scoped.
- `PlayerController` registers with `PlayerNetworkManager` only when `NetworkObject.IsPlayerObject` is `true`.

## Verification Checklist

- Open `Assets/Scenes/Game.unity`.
- Select `NetworkManagers` and confirm `IngredientNetworkSpawner._ingredientPrefab` is `None`.
- Confirm `NetworkManagers` still contains `OrderManager`, `ScoreManager`, `RoundTimer`, and `NetworkScoreManager`.
- Select `MatchRuntimeSceneBinder` and confirm:
  - `_roundTimer -> NetworkManagers/RoundTimer`
  - `_networkScoreManager -> NetworkManagers/NetworkScoreManager`
  - `_spawnManager -> SpawnManager`
  - `_resolveMissingReferencesOnSceneLoad = true`
- Confirm all 8 spawn points have correct team categories:
  - Team A objects use `1`
  - Team B objects use `2`
- Open `Assets/DefaultNetworkPrefabs.asset` and confirm the three gameplay prefabs listed above are present.
- Open `Tomato.asset` and `Steak.asset` and confirm both point to `Assets/Resources/Prefabs/Gameplay/Ingredient.prefab`.
- Run the host flow in `Game.unity` and verify ingredient crates no longer fail to spawn ingredients.
