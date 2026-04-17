# RecipeRage Phase Roadmap

This file translates the aspirational GDD into an implementation roadmap grounded in the current codebase.

Use it with:

1. `Documentation/Architecture/PROJECT_MEMORY.md`
2. `Documentation/Architecture/GDD_ALIGNMENT_MATRIX.md`
3. `KitchenClash_GDD_v3.md`
4. `Documentation/KitchenClash_GDD_v3_aspirational.docx`

Implementation work follows current code and the current-state markdown GDD. This roadmap explains which aspirational items belong to the current phase versus later phases.

## Current Phase

### Phase 2: Runtime Verification and Stabilization

Status: `Current`

The project is no longer in early foundation setup, but it is not yet in the large architecture-migration phase.

This phase is about proving that the current code path works end to end in Unity and closing the remaining `Partial` items from the GDD alignment matrix.

Primary targets:

- verify queue-selected `quick_2v2` and `quick_3v3` sizing flows through matchmaking, lobby, and gameplay
- verify timer-expiry and score-limit endings both converge on `GameOverState`
- verify `MatchResultSync` drives the correct winner/draw result in Game Over UI
- confirm host disconnect still remains a direct technical return-to-lobby path

Exit criteria:

- `Queue-driven 2v2 / 3v3 match format` can move from `Partial` to `Implemented`
- `Game-over transition flow` can move from `Partial` to `Implemented`
- manual Unity verification confirms the current gameplay loop is stable

## Completed Foundation

### Phase 1: Current-State Foundation

Status: `Complete enough to build on`

This phase covers the major implementation foundations that already exist in current code:

- state-driven app flow: `BootstrapState -> LoginState/MainMenuState -> MatchmakingState -> GameplayState -> GameOverState`
- UI Toolkit + view-model-based UI architecture
- queue-aware game mode selection and matchmaking startup
- gameplay-owned match end via `MatchEndController`
- synchronized final-result snapshot via `MatchResultSync`

These items are not all identical to the aspirational GDD, but they are the current implementation base and should be treated as established unless a later phase intentionally replaces them.

## Next Phase

### Phase 3: Architecture Cleanup

Status: `Next`

This is the next engineering phase after runtime verification is complete.

Primary targets:

- reduce singleton-heavy ownership in networking and auth flows
- move more runtime ownership behind DI boundaries instead of static/global access
- clean up high-friction architecture drift that blocks later roadmap work

Main backlog area:

- `AuthenticationService`
- `LobbyService`
- `MatchmakingService`
- `NetworkGameManager`
- `NetworkObjectPool`
- related singleton-heavy gameplay/network bridges tracked in `GDD_ALIGNMENT_MATRIX.md`

This phase is still grounded in the current code structure. It does not force the larger aspirational migrations yet.

## Later Phases

### Phase 4: Major Architecture Migration

Status: `Later`

These items remain planned, but they are too large for cleanup-only work:

- `RouterService` push/pop navigation architecture
- `Root/Menu/Match` lifetime-scope split

These should be treated as explicit migration projects, not opportunistic cleanup.

### Phase 5: External Auth Feature Phase

Status: `Later`

These items remain product and platform work rather than cleanup:

- direct `Google / Facebook / Apple` EOS Connect login flows
- account linking and upgrade flow from guest identity
- provider-specific platform integration work

## Not Current Requirements

The aspirational GDD contains some target-state rules that should not be treated as current implementation requirements yet.

These remain roadmap items:

- zero static singletons
- `RouterService` as the active UI navigation model
- `Root/Menu/Match` lifetime scopes as the active DI split
- full direct provider-to-EOS auth flow

If those ideas conflict with current code during normal implementation work, follow current code plus `KitchenClash_GDD_v3.md`, then schedule the larger change into the appropriate later phase instead of forcing it into an unrelated task.
