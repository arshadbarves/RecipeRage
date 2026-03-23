# GDD Alignment Matrix

This matrix tracks where the current RecipeRage implementation matches, partially matches, or diverges from the phase-development GDD.

## Status Legend

- `Implemented`: current code matches the intended behavior closely enough for implementation work.
- `Partial`: current code contains some of the target behavior, but still has meaningful drift.
- `Planned`: called out in the aspirational GDD, but not implemented in the current repo.

## Current Source-Of-Truth Rule

For implementation decisions:

1. current code
2. `KitchenClash_GDD_v3.md`
3. `Documentation/KitchenClash_GDD_v3_aspirational.docx`

## Alignment Table

| Area | Status | Current Repo Reality | Owning Subsystem | Recommended Phase |
| --- | --- | --- | --- | --- |
| State-driven matchmaking | Implemented | `MatchmakingState` owns matchmaking startup, timeout, and gameplay transition | App State / Matchmaking | Keep current |
| Queue-driven 2v2 / 3v3 match format | Partial | queue catalog and game mode assets support 2v2 / 3v3, and generic 4-player fallbacks were removed, but broader runtime verification is still needed | Matchmaking / Lobby | Runtime verification |
| Game-over transition flow | Partial | `GameOverState` is now wired from timer expiry through `GameplayHudViewModel`, but non-timer end conditions still need verification | App State / HUD | Runtime verification |
| UI Toolkit + view models | Implemented | current UI uses UI Toolkit screens and view models | UI | Keep current |
| RouterService push/pop architecture | Planned | repo still uses `UIService` + `UIScreenStackManager` | UI Architecture | Major migration |
| Root/Menu/Match scope split | Planned | repo uses `GameLifetimeScope` + `SessionLifetimeScope` | DI / Bootstrap | Major migration |
| Full Google / Facebook / Apple EOS Connect flows | Planned | repo remains guest/EOS-first; provider text existed ahead of implementation | Auth | Feature phase |
| Zero static singleton goal | Planned | repo still relies on `EOSManager.Instance`, `NetworkManager.Singleton`, and similar globals | Core Networking / Auth | Architecture phase |

## Singleton-Heavy Backlog

These scripts remain off-GDD relative to the zero-singleton target and should be treated as roadmap work, not cleanup-only work:

- `Assets/Scripts/Core/Auth/AuthenticationService.cs`
- `Assets/Scripts/Gameplay/App/Networking/GameStarter.cs`
- `Assets/Scripts/Gameplay/App/Networking/NetworkingServiceContainer.cs`
- `Assets/Scripts/Core/Networking/Services/LobbyService.cs`
- `Assets/Scripts/Core/Networking/Services/MatchmakingService.cs`
- `Assets/Scripts/Core/Networking/Services/PlayerManager.cs`
- `Assets/Scripts/Core/Networking/Services/TeamManager.cs`
- `Assets/Scripts/Core/Networking/Services/NetworkGameManager.cs`
- `Assets/Scripts/Core/Networking/Services/NetworkObjectPool.cs`
- `Assets/Scripts/Gameplay/Spawning/SpawnManager.cs`
- `Assets/Scripts/Gameplay/Cooking/PlateItem.cs`
- `Assets/Scripts/Gameplay/Cooking/IngredientNetworkSpawner.cs`
- `Assets/Scripts/Gameplay/Characters/PlayerTeamVisuals.cs`
- `Assets/Scripts/Core/Persistence/Providers/EOSCloudStorageProvider.cs`

## Low-Risk Next Pass Targets

- verify queue-selected team size drives lobby and matchmaking behavior end to end
- keep auth/provider UI text honest about what is implemented now
- verify non-timer end conditions also converge on `GameOverState`
