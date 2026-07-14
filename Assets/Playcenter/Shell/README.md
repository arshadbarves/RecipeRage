# Playcenter.Shell

In-repo portable shell contracts for Brawl-class multiplayer games.

## Purpose

- Own **cross-title shell primitives**: logging facade, event bus, connectivity contracts.
- Keep **zero game-specific references** (no KitchenClash, EOS, NGO, Unity).
- Game adapters implement ports (`UnityLoggingService`, `NetworkConnectivityService`).

## Layout

```
Runtime/
  Playcenter.Shell.asmdef   # noEngineReferences, no game refs
  Logging/                  # ILoggingService, LogLevel, LogEntry, GameLogger
  Events/                   # IEventBus, EventBus
  Connectivity/             # IConnectivityService, ConnectivityState
```

## Rules

1. `GameLogger.SetService` must run from game `LoggingBootstrap` before any log call.
2. Missing service throws `InvalidOperationException` (no Console fallback).
3. Event **types** stay in the game Domain; only the bus lives here.
4. Connectivity **policy timers** stay in the game adapter; only the contract lives here.

## DI (game root)

```csharp
builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
builder.Register<UnityLoggingService>(Lifetime.Singleton).As<ILoggingService>();
builder.RegisterEntryPoint<LoggingBootstrap>();
builder.Register<NetworkConnectivityService>(Lifetime.Singleton).As<IConnectivityService>().As<ITickable>();
```
