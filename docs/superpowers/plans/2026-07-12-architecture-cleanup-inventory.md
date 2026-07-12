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
| `Assets/_KitchenClash/Infrastructure/Analytics/FirebaseAnalyticsService.cs` | Only registered IAnalyticsService implementation; live in RootLifetimeScope line 123 | yes |
| `Assets/_KitchenClash/Infrastructure/Firebase/FirebaseConfigProvider.cs` | IConfigProvider implementation; live behind FIREBASE_REMOTE_CONFIG define (RootLifetimeScope line 117) | yes |
| `Assets/_KitchenClash/Infrastructure/Firebase/ConfigModels.cs` | Firebase config DTOs; consumed by FirebaseConfigProvider | yes |

## Legacy-delete

| Path | Rationale | refs-checked | Status |
|------|-----------|--------------|--------|
| ~~`Assets/Scripts/Editor/GameModeGenerator.cs`~~ | Editor tool with FreeForAll/TeamBattle helpers; no mode assets load these formats; self-ref only | yes | **EDITED**: Removed classic mode presets (SetClassicDefaults, SetTeamBattleDefaults, SetFreeForAllDefaults, SetRankedDefaults); kept editor window + Time Attack/Survival presets |
| ~~`Assets/Resources/ScriptableObjects/GameModes/FreeForAll.asset`~~ | Legacy mode asset; no active refs | yes | **DELETED** |
| ~~`Assets/Resources/ScriptableObjects/GameModes/RankedMode.asset`~~ | Legacy mode asset; no active refs | yes | **DELETED** |
| ~~`Assets/Resources/ScriptableObjects/GameModes/TeamBattle.asset`~~ | Legacy mode asset; no active refs | yes | **DELETED** |
| ~~`Assets/Resources/ScriptableObjects/GameModes/classicMode.asset`~~ | Legacy mode asset; no active refs | yes | **DELETED** |

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

## Task 7 note (2026-07-12)

No v1 mechanic code found. Exhaustive search for `class *Combo`, `class *Desperation`, `class *HeatChallenge`, `class *HandOff`, `class RouterService`, `interface IRouter` returned zero results. Remaining "combo" references are field names (`ComboCount`, `ComboMultiplier`) in Keep-v2 domain models and obsolete config keys, not v1 mechanic classes.

## Task 8 note (2026-07-12)

Station surface purge blocked on v2 scene replacement — follow-on track.
Game.unity still depends on CookingPot, CuttingStation, AssemblyStation and related Shared station scripts/prefabs.
No station scripts or prefabs deleted. AutonomousCookingStation + T1–T3 prefabs remain Keep-v2.

## Task 9 note (2026-07-12)

Firebase code retained in **Shared** bucket. FirebaseAnalyticsService is the only registered IAnalyticsService implementation (RootLifetimeScope line 123). FirebaseConfigProvider is live behind FIREBASE_REMOTE_CONFIG define (active for Android). No alternative implementations exist. Per brief decision rule: "If RootLifetimeScope still registers Firebase as the live IAnalyticsService / config provider → Shared (do not delete)." Package removal out of scope.

## DI audit

**Scope names:** RootLifetimeScope (app-lifetime) → MenuLifetimeScope (session/menu) → MatchLifetimeScope (per-match)

### Core registrations by scope

| Service | Registered in | Should be | Violation? | Notes |
|---------|---------------|-----------|------------|-------|
| **Root network primitives** |
| INetworkObjectPool | Root (line 156) | Root | **no** | App-lifetime, shared across sessions |
| INetworkGameManager | Root (line 157) | Root | **no** | App-lifetime, shared across sessions |
| IPlayerNetworkManager | Root (line 155) | Root | **no** | App-lifetime, shared across sessions |
| NetworkManager | Root (line 148 instance) | Root | **no** | Unity singleton on GameBootstrap GO |
| **Root core services** |
| IEventBus | Root (line 53) | Root | **no** | App-lifetime event bus |
| ILoggingService | Root (line 54) | Root | **no** | App-lifetime logging |
| IUIService | Root (line 104) | Root | **no** | Root UI stack manager |
| IGameStateManager | Root (line 111) | Root | **no** | App-lifetime state machine |
| IPlayerDataService | Root (line 112) | Root | **no** | Player data across sessions |
| ISaveService | Root (line 114) | Root | **no** | Save/load across sessions |
| IConfigService / IRemoteConfigService | Root (line 118/120) | Root | **no** | Remote config across sessions |
| IAuthService | Root (line 136) | Root | **no** | Authentication across sessions |
| ITutorialService | Root (line 140–141) | Root | **no** | SessionLoadingState (root) needs this; only depends on root services |
| IAudioService | Root (line 80) | Root | **no** | Audio across sessions |
| ILocalizationManager | Root (line 105) | Root | **no** | Localization across sessions |
| **Menu/session services** |
| ISessionContext | Menu (line 19) | Menu | **no** | Session-scoped context |
| IMatchService | Menu (line 22) | Menu | **no** | Match history/stats |
| IEconomyService | Menu (line 23) | Menu | **no** | Economy operations |
| IDailyStreakService | Menu (line 25) | Menu | **no** | Daily streak tracking |
| ITrophyService | Menu (line 26) | Menu | **no** | Trophy management |
| ICharacterService | Menu (line 31) | Menu | **no** | Character selection |
| ISkinsService | Menu (line 32) | Menu | **no** | Skin management |
| IGameModeService | Menu (line 33) | Menu | **no** | Game mode selection |
| ITeamManager | Menu (line 39) | Menu | **no** | Team management |
| IPlayerManager | Menu (line 40) | Menu | **no** | Player management |
| ILobbyManager | Menu (line 41) | Menu | **no** | Lobby operations |
| IMatchmakingService | Menu (line 42) | Menu | **no** | Matchmaking operations |
| INetworkingServices | Menu (line 48–50) | Menu | **no** | NetworkingServiceContainer |
| IGameStarter | Menu (line 52–53) | Menu | **no** | Factory delegate from NetworkingServiceContainer |
| IMatchContextReceiver | Menu (line 54–55) | Menu | **no** | Factory delegate from NetworkingServiceContainer |
| **Match services** |
| IScoreService | Match (line 23) | Match | **no** | Per-match scoring |
| IOrderService | Match (line 24) | Match | **no** | Per-match orders |
| IAbilityService | Match (line 25) | Match | **no** | Per-match abilities |
| IHazardService | Match (line 26) | Match | **no** | Per-match hazards |
| IMatchContext | Match (line 30–32) | Match | **no** | Match runtime registry |
| IMatchRuntimeRegistry | Match (line 30–32) | Match | **no** | Same as IMatchContext (dual interface) |
| RecipeCatalog | Match (line 14) | Match | **no** | Per-match recipe data |
| BotManager | Match (line 35) | Match | **no** | Per-match bot AI |
| BotClaimRegistry | Match (line 36) | Match | **no** | Per-match bot claims |
| BotTaskPlanner | Match (line 37) | Match | **no** | Per-match bot planning |
| AbilityFactory | Match (line 17) | Match | **no** | Per-match ability creation |
| AbilityEffectHandler | Match (line 20) | Match | **no** | Per-match ability effects |

### Anti-pattern scan results

| Location | Pattern | Likely action | Status |
|----------|---------|---------------|--------|
| `MatchRuntimeSceneBinder:238` | `FindObjectsByType` | Shared bootstrap — scene discovery only; binder registers to registry to avoid downstream FindObjectOfType | **Keep** |
| `SpawnManager:82` | `FindObjectsByType<SpawnPoint>` | Unknown — fix only if `IMatchRuntimeRegistry` already exposes spawn points | **Fix if registry available** |
| `ServingStation:112` | `FindObjectsByType<SinkStation>` | Prefer registry/context if available; else leave Unknown | **Fix if registry available** |
| `HomeScreen:185` | `SessionContainer` | Prefer injected services already on Menu scope | **Task 13: inject services** |
| `SkinsTabComponent:85` | `SessionContainer` | Prefer injected services already on Menu scope | **Task 13: inject services** |
| `PlayerController:433,1057` | `SessionContainer` | Prefer constructor/field injection of needed interfaces | **Task 12: inject services** |
| Comments forbidding Singleton | `MatchLifetimeScope:29`, `MatchRuntimeSceneBinder:15`, `MatchContext:13–14`, `CookingStation:388` | Keep comments; no code change | **Keep** |

### Violations for Tasks 12–14

**Task 12 (Root network scope):** ✅ No violations
- INetworkObjectPool, INetworkGameManager, IPlayerNetworkManager correctly registered in Root
- NetworkManager instance correctly registered in Root
- No match-scoped services registered in Root

**Task 13 (Menu scope):** 2 violations
1. `HomeScreen.cs:185` — accesses `SessionContainer` directly instead of injecting services
2. `SkinsTabComponent.cs:85` — accesses `SessionContainer` directly instead of injecting services

**Task 14 (Match scope):** 1 violation
1. `PlayerController.cs:433,1057` — accesses `SessionContainer` directly instead of injecting services

**FindObjectOfType violations:** 2 candidates
1. `SpawnManager:82` — `FindObjectsByType<SpawnPoint>` (fix if registry exposes spawn points)
2. `ServingStation:112` — `FindObjectsByType<SinkStation>` (fix if registry exposes sinks)

### Task 11 completion summary

- **Root scope services:** 17 core + 3 network primitives (20 total) ✅
- **Menu scope services:** 14 + 2 factory delegates (16 total) ✅
- **Match scope services:** 11 + 2 bridges (13 total) ✅
- **SessionContainer violations:** 3 files (2 UI, 1 player controller) → Tasks 12–14
- **FindObjectOfType candidates:** 2 files (SpawnManager, ServingStation) → follow-on if registry available
