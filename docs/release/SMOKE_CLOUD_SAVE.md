# Smoke: Cloud save

## Preconditions
- Guest login works (Task 4 smoke)
- Online at least once for cloud path

## Steps
1. Guest login.
2. Set display name in UI (or UsernameViewModel path).
3. Return home (progress/stats persist on write).
4. Kill app, relaunch offline: name present from local cache.
5. Relaunch online: name present; EOS Player Data Storage receives `player_progress.json` + `player_stats.json` on save (CloudWithCache).

## Implementation notes (2026-07-24)
- `PlayerDataService` now loads `player_progress.json` / `player_stats.json` in `Initialize()` and persists on every mutation via `SaveData`.
- `SaveService` registers both keys with `StorageStrategy.CloudWithCache` (local write + background cloud sync when available).
- `KitchenClashAuthLifecycleHooks.OnLoginSucceeded` now calls `ISaveService.OnUserLoggedIn()` (previously never called — cloud provider stayed locked); `OnLogout` calls `OnUserLoggedOut()`.

## Last run
- Date: _pending — requires Unity Editor run_
- Result: _PENDING_
