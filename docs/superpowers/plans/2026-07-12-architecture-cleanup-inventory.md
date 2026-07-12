# Architecture Cleanup Inventory — 2026-07-12

> Phase 2 may delete **only** rows with bucket `Legacy-delete` and `refs-checked: yes` with zero Keep-v2/Shared refs.

## Legend

| Bucket | Action |
|--------|--------|
| Keep-v2 | Keep; do not finish features |
| Shared | Keep; may fix ownership in Phase 3 |
| Legacy-delete | Delete in Phase 2 after reference gate |
| Unknown | Default keep until proven legacy-only |

## Keep-v2

| Path | Rationale | refs-checked |
|------|-----------|--------------|
| `Assets/_KitchenClash/Infrastructure/Network/PlayerCombatController.cs` | Kitchen Brawler v2 scaffold — do not delete | n/a |
| `Assets/_KitchenClash/Infrastructure/Network/LootPickup.cs` | Kitchen Brawler v2 scaffold — do not delete | n/a |
| `Assets/_KitchenClash/Infrastructure/Network/Stations/AutonomousCookingStation.cs` | Kitchen Brawler v2 scaffold — do not delete | n/a |
| `Assets/_KitchenClash/Infrastructure/Network/MatchWinConditionCoordinator.cs` | Kitchen Brawler v2 scaffold — do not delete | n/a |
| `Assets/_KitchenClash/Application/Services/ModeWinConditions.cs` | Kitchen Brawler v2 scaffold — do not delete | n/a |
| `Assets/_KitchenClash/Application/Services/IModeWinCondition.cs` | Kitchen Brawler v2 scaffold — do not delete | n/a |
| `Assets/_KitchenClash/Infrastructure/Gameplay/Abilities/ArchetypeAbilities.cs` | Kitchen Brawler v2 scaffold — do not delete | n/a |
| `Assets/Resources/ScriptableObjects/GameModes/RushService.asset` | Kitchen Brawler v2 scaffold — do not delete | n/a |
| `Assets/Resources/ScriptableObjects/GameModes/HellsKitchen.asset` | Kitchen Brawler v2 scaffold — do not delete | n/a |
| `Assets/Resources/ScriptableObjects/GameModes/LastPlateStanding.asset` | Kitchen Brawler v2 scaffold — do not delete | n/a |
| `Assets/_KitchenClash/Prefabs/Stations/AutonomousCookingStation_T1.prefab` | Kitchen Brawler v2 scaffold — do not delete | n/a |
| `Assets/_KitchenClash/Prefabs/Stations/AutonomousCookingStation_T2.prefab` | Kitchen Brawler v2 scaffold — do not delete | n/a |
| `Assets/_KitchenClash/Prefabs/Stations/AutonomousCookingStation_T3.prefab` | Kitchen Brawler v2 scaffold — do not delete | n/a |
| `Assets/Resources/Prefabs/Gameplay/LootPickup.prefab` | Kitchen Brawler v2 scaffold — do not delete | n/a |
| `Assets/_KitchenClash/Domain/Enums/ChefArchetype.cs` | Kitchen Brawler v2 scaffold — do not delete | n/a |
| `Assets/_KitchenClash/Domain/Events/CombatEvents.cs` | Kitchen Brawler v2 scaffold — do not delete | n/a |
| `Assets/Scenes/Map_RushService.unity` | Kitchen Brawler v2 scaffold — do not delete | n/a |
| `Assets/Scenes/Map_HellsKitchen.unity` | Kitchen Brawler v2 scaffold — do not delete | n/a |
| `Assets/Scenes/Map_LastPlateStanding.unity` | Kitchen Brawler v2 scaffold — do not delete | n/a |
| `Assets/_KitchenClash/Infrastructure/Network/Stations/DeliveryZone.cs` | Kitchen Brawler v2 scaffold — do not delete | n/a |

## Shared

| Path | Rationale | refs-checked |
|------|-----------|--------------|
| `Assets/_KitchenClash/Infrastructure/Network/Stations/StationBase.cs` | Base class for all stations (v1 + v2); used by AutonomousCookingStation | n/a |
| `Assets/_KitchenClash/Infrastructure/Network/Stations/ServingStation.cs` | Active in Game.unity + Map_*.unity scenes; dual-path score logic (legacy + v2 IScoreService) | n/a |
| `Assets/_KitchenClash/Infrastructure/Network/Stations/ProcessingStation.cs` | Base class for CookingStation/CuttingStation (Game.unity active); inheritance by legacy stations | n/a |
| `Assets/_KitchenClash/Infrastructure/Network/Stations/CookingStation.cs` | Inherits ProcessingStation; used by bots (BotTaskPlanner) for snapshot planning; Match path | n/a |
| `Assets/_KitchenClash/Infrastructure/Network/Stations/CuttingStation.cs` | Active in Game.unity (line 3006); inherits ProcessingStation | n/a |
| `Assets/_KitchenClash/Infrastructure/Network/Stations/IngredientCrate.cs` | Active in Game.unity + Resources/Prefabs; resolves IngredientNetworkSpawner via IMatchContext | n/a |
| `Assets/_KitchenClash/Infrastructure/Network/Stations/PlateDispenser.cs` | Active in Resources/Prefabs; used in bot planning (BotPlanningSnapshot) | n/a |
| `Assets/_KitchenClash/Infrastructure/Network/Stations/CounterStation.cs` | Used by BotKitchenSnapshot.FindNearestAvailableCounter; bot claim logic | n/a |
| `Assets/_KitchenClash/Infrastructure/Network/Stations/SinkStation.cs` | Linked by ServingStation.OnNetworkSpawn; dual-path dish cleanup | n/a |
| `Assets/_KitchenClash/Infrastructure/Network/Stations/TrashBin.cs` | Active station type; inherits StationBase | n/a |
| `Assets/Prefabs/Gameplay/Stations/CookingPot.prefab` | Active in Game.unity (line 3916); legacy match path | n/a |
| `Assets/Prefabs/Gameplay/Stations/CuttingStation.prefab` | Active in Game.unity (line 3006); legacy match path | n/a |
| `Assets/Prefabs/Gameplay/Stations/AssemblyStation.prefab` | Active in Game.unity (18 references); legacy match path | n/a |
| `Assets/Prefabs/Gameplay/Stations/ServingStation.prefab` | Active in Game.unity (line 1826); legacy match path | n/a |
| `Assets/Resources/Prefabs/Gameplay/Stations/IngredientCrate.prefab` | Active in Resources; match runtime ingredient spawning | n/a |
| `Assets/Resources/Prefabs/Gameplay/Stations/PlateDispenser.prefab` | Active in Resources; bot planning reference | n/a |
| `Assets/_KitchenClash/Infrastructure/Network/PlayerController.cs` | Core player controller; has v2 dish tracking (AutonomousCookingStation integration); match path | n/a |
| `Assets/_KitchenClash/Infrastructure/Network/Bot/BotController.cs` | Bot AI controller; uses StationBase, CounterStation; dual-path match runtime | n/a |
| `Assets/_KitchenClash/Infrastructure/Network/Bot/BotKitchenSnapshot.cs` | Bot state snapshot; FindNearestAvailableCounter uses CounterStation | n/a |
| `Assets/_KitchenClash/Application/Services/BotTaskPlanner.cs` | Bot task planning; references CookingStationIds, ServingStationIds, PlateDispenserIds | n/a |
| `Assets/_KitchenClash/Application/Services/BotPlanningSnapshot.cs` | Bot planning state; lists CookingStationIds, ServingStationIds, PlateDispenserIds | n/a |
| `Assets/_KitchenClash/Infrastructure/Network/MatchRuntimeSceneBinder.cs` | Scene runtime binder; includes DeliveryZone + all station types in discovery | n/a |
| `Assets/Scenes/Game.unity` | Active legacy match scene; references CookingPot, CuttingStation, AssemblyStation, ServingStation | n/a |

## Legacy-delete

| Path | Rationale | refs-checked |
|------|-----------|--------------|
| `Assets/Scripts/Editor/GameModeGenerator.cs` | Editor tool with FreeForAll/TeamBattle helpers; no mode assets load these formats; self-ref only | yes |

## Unknown

| Path | Rationale | refs-checked |
|------|-----------|--------------|
| `Assets/Scenes/Maps/rookie_kitchen.unity` | Map scene with legacy station references (CookingStation, ServingStation); unclear if v2 replaces | n/a |
| `Assets/Scenes/Maps/taco_truck.unity` | Map scene with legacy station references (CookingStation, ServingStation); unclear if v2 replaces | n/a |
| `Assets/Scenes/Maps/volcano_kitchen.unity` | Map scene with legacy station references (CookingStation, ServingStation); unclear if v2 replaces | n/a |
| `Assets/Scenes/Maps/pirate_pot.unity` | Map scene with legacy station references (CookingStation, ServingStation); unclear if v2 replaces | n/a |
| `Assets/Scenes/Maps/space_station.unity` | Map scene with legacy station references (CookingStation, ServingStation); unclear if v2 replaces | n/a |
| `Assets/Scenes/Maps/burger_boulevard.unity` | Map scene with legacy station references (CookingStation, ServingStation); unclear if v2 replaces | n/a |
| `Assets/Scenes/Maps/sushi_shuffle.unity` | Map scene with legacy station references (CookingStation, ServingStation); unclear if v2 replaces | n/a |
| `Assets/Scenes/Maps/clash_kitchen.unity` | Map scene with legacy station references (CookingStation, ServingStation); unclear if v2 replaces | n/a |
| `Assets/Scripts/Editor/MapSceneGenerator.cs` | Editor tool for generating map scenes with AutonomousCookingStation placeholders; unclear if obsolete | n/a |
| `Assets/_KitchenClash/Infrastructure/Network/Stations/StationNetworkController.cs` | Station network controller; unclear current usage | n/a |

## Dual-path notes

- `Game.unity` still references legacy station instance names (`CookingPot`, `CuttingStation`, `AssemblyStation`). Treat live match-path stations as Shared/Unknown until a v2 scene fully replaces them.
- `MenuLifetimeScope` is the session/menu child scope in code (docs historically said `SessionLifetimeScope`).
- `ServingStation` has dual-path score logic: legacy NetworkScoreManager fallback + v2 IScoreService path (lines 98, 105, 172).
- `IngredientCrate` resolves `IngredientNetworkSpawner` through `IMatchContext` (v2 pattern), not `FindObjectOfType`.
- `PlayerController` has v2 dish tracking field (line 80) for `AutonomousCookingStation` integration.
- Bot systems (`BotTaskPlanner`, `BotPlanningSnapshot`, `BotKitchenSnapshot`) consume both legacy station types and v2 station IDs.
