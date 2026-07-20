# Playcenter.Services

In-repo portable **service contracts** for multi-title Brawl-class games.

## Purpose

- Own engine-free ports shared across titles: config, analytics, ads, IAP, auth, encryption, maintenance, localization, storage, time, audio volume, remote-config, session (lobby/MM/team), social (friends).
- Zero KitchenClash / Unity / EOS / NGO references (`noEngineReferences`).
- Game adapters implement these interfaces (Firebase, EOS, stubs, etc.).

## Layout

```
Runtime/
  Playcenter.Services.asmdef
  Config/        IConfigService, IConfigModel, ConfigHealthStatus
  Analytics/     IAnalyticsService
  Ads/           IAdsService, AdRewardResult
  IAP/           IIAPService, IAPResult
  Auth/          IAuthService, AuthResult
  Encryption/    IEncryptionService
  Maintenance/   IMaintenanceService
  Localization/  ILocalizationManager
  Storage/       IStorageProvider, ICloudStorageProvider, StorageConfig, StorageStrategy
  Time/          INTPTimeService
  Audio/         IAudioVolumeController
  RemoteConfig/  IRemoteConfigService, IConfigProvider
  Session/       ILobbyManager, IMatchmakingService, ITeamManager, Lobby*, PlayerInfo, BotPlayer, TeamId
  Social/        IFriendsService, IFriendsServiceFactory, FriendInfo, FriendRequest
```

## Rules

1. No Unity types (AudioClip, MonoBehaviour, UniTask) — those stay game-side.
2. No game IP (cooking, economy tables, chefs, maps).
3. `Playcenter.GameFlow` and `Playcenter.Shell` do **not** reference Services (keep independent).
4. Domain/Application may reference Services; delete KitchenClash originals on cutover.

## Not here (stay game or Unity-bound leaves)

- Clip-based audio (`AudioClip` / `AudioSource` playback)
- UI stack contracts — live in `Playcenter.UI` (not this assembly); UI Toolkit adapter stays game-side
- Save DTOs (game-specific persistence models)
- Platform / Async Unity helpers
