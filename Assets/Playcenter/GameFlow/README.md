# Playcenter.GameFlow

In-repo product flow module for Brawl-class multiplayer shells.

## Purpose

- Own **legal product transitions** (Splash → Boot → Home → Matchmaking → Intro → Countdown → Match → Results).
- Expose **`IAppFlow`** as the only public navigation API for UI/features.
- Keep **zero game-specific references** (no KitchenClash, EOS, NGO types).

## Layout

```
Runtime/
  Playcenter.GameFlow.asmdef   # noEngineReferences, no game refs
  Core/                        # IAppFlow, AppFlowController, context, DTOs
  Ports/                       # ISplashPort, IHomePort, ...
  Policies/                    # AlwaysResolve, SoftPopup, RememberedQueue
```

Game adapters live in the game project, e.g. `Assets/_KitchenClash/Infrastructure/Flow/`.

## Rules

1. UI calls `IAppFlow` intents only (`RequestPlay`, `ReturnHome`, …).
2. Ports do scene/UI/net work; GameFlow never loads scenes itself.
3. Illegal transitions fail-closed to **Home**.
4. Matchmaking always resolves (bot fill is a game-port policy using `AlwaysResolveMatchPolicy`).

## DI

Register `AppFlowController` as `IAppFlow` (singleton) at root scope, with port adapters from the game assembly.

## Future extract (deferred)

When a second game needs this module:

1. Move `Assets/Playcenter/GameFlow` → repo `playcenter-gameflow`
2. Add as git submodule or UPM package
3. Pin a tag; do not extract before the loop is proven in RecipeRage

## Production target

See **`wiki/GameFlow-SDK.md`**: GameFlow is the public navigator; game states are phase workers/adapters only.

### Policies (shipped)

| Type | Use |
|------|-----|
| `AlwaysResolveMatchPolicy` | Bot-fill after timeout (game port calls `ShouldFillWithBots`) |
| `RememberedQueuePolicy` | Empty PLAY → last mode/team/chef |
| `SoftPopupPolicy` | Soft offers only after first completed play; query via `IAppFlow.CanShowSoftPopup()` |
