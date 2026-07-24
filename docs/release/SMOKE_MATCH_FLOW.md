# Smoke: Match flow (EOS Dev)

## Steps
1. Two builds/Editor instances, both guest-logged on Dev **or** one client with bot fill enabled.
2. Both select Rush Service 2v2 queue (mode id `rush_service`).
3. Expect lobby form → match start ≤ 60s (or bot fill policy).
4. Both load gameplay scene; NGO `IsConnectedClient` true; player objects spawn.
5. Host migration / disconnect: leaving client returns to menu without freeze.

## Log markers
- Lobby create/join success
- Netcode start host/client
- `MatchLifetimeScope` build
- No repeated transport hard-fail loops

## Build settings fix (2026-07-24)
`Assets/Scenes/Map_RushService.unity` (guid `0459e120c2a934a498df1ee516524321`) added to `ProjectSettings/EditorBuildSettings.asset` — previously missing, so mode asset's `_mapSceneName: Map_RushService` could not load in builds.

## Last run
- Date: _pending — requires Unity Editor run_
- Result: _PENDING_
