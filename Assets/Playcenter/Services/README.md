# Playcenter.Services

In-repo portable **service contracts** for multi-title Brawl-class games.

## Purpose

- Own engine-free ports shared across titles: config, analytics, ads, IAP, auth, encryption, maintenance.
- Zero KitchenClash / Unity / EOS / NGO references (`noEngineReferences`).
- Game adapters implement these interfaces (Firebase, EOS, stubs, etc.).

## Layout

```
Runtime/
  Playcenter.Services.asmdef
  Config/       IConfigService, IConfigModel, ConfigHealthStatus
  Analytics/    IAnalyticsService
  Ads/          IAdsService, AdRewardResult
  IAP/          IIAPService, IAPResult
  Auth/         IAuthService, AuthResult
  Encryption/   IEncryptionService
  Maintenance/  IMaintenanceService
```

## Rules

1. No Unity types (AudioClip, MonoBehaviour, UniTask) — those stay game-side.
2. No game IP (cooking, economy tables, chefs, maps).
3. `Playcenter.GameFlow` and `Playcenter.Shell` do **not** reference Services (keep independent).
4. Domain/Application may reference Services; delete KitchenClash originals on cutover.

## Not here (stay game or Unity-bound leaves)

- Audio (`AudioClip` / `AudioSource`)
- UI stack (`IUIService` + UI Toolkit)
- Save/storage with game DTOs
- Platform / Async Unity helpers
- Remote-config orchestration that uses UniTask (game Application ports may compose Services types)
