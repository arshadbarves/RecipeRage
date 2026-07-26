# Playcenter.EOS

**Tier:** Unity-thin (EOS SDK + UGS + UniTask; no VContainer, no KitchenClash)

Shared Epic Online Services slice for multi-title reuse: Connect auth, optional UGS bridge, and Player Data Storage cloud provider.

## DAG position

```
Playcenter.EOS
  → Playcenter.Shell      (GameLogger)
  → Playcenter.Services   (IAuthService, AuthResult, ICloudStorageProvider)
  → UniTask
  → PlayEveryWare EOS / Epic Online Services
  → Unity.Services.Core / Authentication
```

No KitchenClash, UI, GameFlow, Animation, or VContainer references.

## Key types

| Type | Purpose |
|------|---------|
| `AuthenticationService` | EOS Device ID guest login + optional UGS OpenID bridge |
| `EOSCloudStorageProvider` | `ICloudStorageProvider` over EOS Player Data Storage |
| `IEOSConfig` | Title-supplied UGS project/profile flags |
| `IAuthLifecycleHooks` | Title side effects (events, settings) after login/logout |
| `EosResultMapper` | Generic `Result` → success/error-code helpers |

## Game-side wiring (KitchenClash)

```csharp
// Adapters
UgsEosConfigAdapter              : IEOSConfig from UGSConfig SO
KitchenClashAuthLifecycleHooks   : publishes LoginSuccessEvent / LogoutEvent, updates settings

// RootLifetimeScope
builder.RegisterInstance(_ugsConfig);
builder.Register<UgsEosConfigAdapter>(Lifetime.Singleton).As<IEOSConfig>();
builder.Register<KitchenClashAuthLifecycleHooks>(Lifetime.Singleton).As<IAuthLifecycleHooks>();
builder.Register<Playcenter.EOS.AuthenticationService>(Lifetime.Singleton).As<IAuthService>();
builder.Register<Playcenter.EOS.EOSCloudStorageProvider>(Lifetime.Singleton).As<ICloudStorageProvider>();
```

## Stays game-side

Lobby, matchmaking, friends, team manager, player manager, transport, player data service.
