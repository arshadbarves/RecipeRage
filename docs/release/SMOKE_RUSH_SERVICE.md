# Smoke: Rush Service slice

## Steps
1. Start 2v2 Rush (bots OK).
2. Prime, collect, deliver at least one dish each team.
3. Confirm score UI / tug bar moves.
4. KO a carrier; loot appears; respawn works.
5. Force win (deliver to target or debug); Results screen shows; coins grant.

## Code readiness audit (2026-07-24)
- `AutonomousCookingStation`: prime→cook→ready→collect state machine implemented (server-auth, NetworkVariable replicated).
- `PlayerCombatController`: melee, KO, 100% loot drop, 3s respawn implemented.
- `ServingStation`: delivery → `AddScoreServerRpc` → `ScoreManager` → `ScoreService` → `ScoreChangedEvent`.
- `MatchEndController`: round start (mode + timer) and match end (score limit / timer) → `GamePhaseSync` → results.
- **Fixed this task:** `MatchWinConditionCoordinator` was never spawned and `SetMode` never called — the tug-of-war win condition was dead code. Added `MatchRuntimeBootstrap` (server-only NetworkObject spawned by `MatchContext.TryWireWinCondition()` once per match) that creates the coordinator, injects deps via `InjectDeps`, and calls `SetMode(modeId)`.
- `Map_RushService.unity`: contains TeamA/B spawns, 2 `AutonomousCookingStation`, 2 `ServingStation`. In build settings.

## Manual scene work (Unity Editor required, pending)
- Tug-of-war bar UI binding in HUD to `TugOfWarWinCondition` bar state (needs editor verification; HUD shows team scores today).
- Verify map floor/colliders/zones play well (editor play test).

## Last run
- Date: _pending — requires Unity Editor run_
- Result: _PENDING_
