# Playcenter Foundation Extract — Design

**Date:** 2026-07-15  
**Branch:** `architecture-cleanup`  
**Status:** Approved for implementation (user override of prior YAGNI deferral)  
**Related:** Shell @ shipped, Services @ shipped, GameFlow @ shipped; candidates plan `docs/superpowers/plans/2026-07-14-playcenter-module-extract-candidates.md`

---

## 1. Problem

After GameFlow / Shell / Services, multi-title **product foundation** ports still live in KitchenClash Application/Domain:

| Area | Today | Why extract |
|------|-------|-------------|
| UI stack contract | `IUIService`, `NotificationType`, `UIScreenCategory` | Every Brawl shell needs type-based screen stack + toast |
| Localization | `ILocalizationManager` | String tables + language switch is multi-title |
| Storage ports | `IStorageProvider`, `ICloudStorageProvider`, `StorageStrategy`, `StorageConfig` | Key/value + cloud lifecycle is multi-title; DTOs stay game |
| Time sync | `INTPTimeService` | Server-skew clock is multi-title |
| Audio volume | `IAudioVolumeController` | Master/music/SFX/mute floats — no clips |
| Remote config orchestration ports | `IRemoteConfigService`, `IConfigProvider` | Compose `Playcenter.Services` config types; UniTask is game-only today |

User directive: extract whatever is common to all games; hard cutover; no legacy dual APIs.

---

## 2. Locked decisions

### 2.1 Two modules (not one mega-Foundation)

| Module | Owns | Why separate |
|--------|------|--------------|
| **`Playcenter.UI`** | Screen stack contract + notification enums + screen category | Large consumer surface (Presentation + Flow); independent of Services |
| **`Playcenter.Services` (expand)** | Localization, Storage, Time, Audio volume, RemoteConfig ports | Same product-service family as auth/config/analytics |

Do **not** create `Playcenter.Audio` / `Playcenter.Platform` / `Playcenter.Async` assemblies for Unity-bound helpers.

### 2.2 Engine-free rules (same bar as Shell/Services)

- `noEngineReferences: true` on both modules.
- **No** Unity types (`AudioClip`, `AudioSource`, `Vector3`, `MonoBehaviour`).
- **No** UniTask / VContainer / DOTween in Playcenter contracts.
- Async surface uses **`System.Threading.Tasks.Task`** (same as `IAuthService`, `IConfigService`).
- `IUIService.SetCurrentScope` today takes `VContainer.IObjectResolver` → change to **`object scope`** (game casts to `IObjectResolver` in `UIService`). No VContainer reference in Playcenter.UI.
- Toast/notification methods return **`Task`** not `UniTask`. Game `UIService` implements with `async Task` (or `AsTask()`).

### 2.3 Hard cutover

- Delete KitchenClash originals after consumers switch.
- No type aliases, obsolete stubs, dual namespaces, or dual UniTask/Task APIs.
- GameFlow and Shell **do not** reference UI or Services.
- UI **does not** reference Services; Services **does not** reference UI.
- Domain/Application/Presentation/Infrastructure reference modules as needed.

### 2.4 Stay game-side forever (this program)

| Item | Why |
|------|-----|
| `IAudioService`, `IMusicPlayer`, `ISFXPlayer` | `AudioClip` / `AudioSource` / `Vector3` |
| `SFXType`, `MusicTrack`, `SFXEvent`, `MusicEvent` | Title sound tables (chop, sizzle, rush…) |
| `ISaveService`, `GameSettingsData`, economy/player DTOs | Title persistence shape |
| `PlatformUtils`, `CoroutineRunner`, `TaskExtensions` | Unity leaf helpers — keep `KitchenClash.Infrastructure.Platform` / `.Async` |
| `ISessionLifecycle`, `ISessionContext`, matchmaking, flow handlers | Game session / NGO / product flow adapters |
| Cooking, chefs, bots, maps, EOS/Firebase/NGO concrete | Game IP / backend choice |
| Presentation `UIScreenPriority` (numeric layering) | Presentation-only; **not** the dead Application enum in `IUIService.cs` |

### 2.5 Application `UIScreenPriority` enum

`IUIService.cs` defines a **second** `UIScreenPriority` (Splash/Loading/Modal…) that is **not** used by Presentation (Presentation has its own numeric enum). **Do not extract** Application’s enum — **delete** it with the file move (only `NotificationType` + interface methods move). Presentation keeps `KitchenClash.Presentation.Common.UIScreenPriority`.

### 2.6 Namespace

- `Playcenter.UI` for UI types.
- `Playcenter.Services` for expanded service ports (same root as existing Services).

---

## 3. Target layout

```
Assets/Playcenter/
  GameFlow/          (unchanged — zero UI/Services refs)
  Shell/             (unchanged — zero UI/Services refs)
  Services/          (EXPAND)
    Runtime/
      …existing Config/Analytics/Ads/IAP/Auth/Encryption/Maintenance…
      Localization/  ILocalizationManager
      Storage/       IStorageProvider, ICloudStorageProvider, StorageStrategy, StorageConfig
      Time/          INTPTimeService
      Audio/         IAudioVolumeController
      RemoteConfig/  IRemoteConfigService, IConfigProvider
    README.md        (update)
  UI/                (NEW)
    Runtime/
      Playcenter.UI.asmdef   # noEngineReferences, references: []
      IUIService.cs
      NotificationType.cs
      UIScreenCategory.cs
    README.md

Assets/_KitchenClash/
  Domain/            DELETE moved enums/models (UIScreenCategory, Storage*)
  Application/       DELETE moved interfaces
  Presentation/      using Playcenter.UI; UIService implements Task + object scope
  Infrastructure/*   using Playcenter.Services / Playcenter.UI; UniTask adapters call Task APIs
  Composition/       usings only
```

---

## 4. Contract shapes (post-extract)

### 4.1 Playcenter.UI

```csharp
namespace Playcenter.UI
{
    public enum NotificationType { Info, Success, Warning, Error }

    public enum UIScreenCategory
    {
        System = 0, Overlay = 1, Modal = 2, Popup = 3,
        Screen = 4, HUD = 5, Toast = 6
    }

    public interface IUIService
    {
        bool IsInitialized { get; }
        // … same navigation surface as today …
        Task ShowToast(string message, NotificationType type = NotificationType.Info, float duration = 3f);
        Task ShowToast(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f);
        Task ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 3f);
        Task ShowNotification(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f);
        // …
        void SetCurrentScope(object scope);  // was VContainer.IObjectResolver
        void Update(float deltaTime);
    }
}
```

### 4.2 Playcenter.Services additions

```csharp
// Localization — same members as today
// Storage — IStorageProvider with Task ReadAsync/WriteAsync (not UniTask)
// ICloudStorageProvider : IStorageProvider + login hooks
// StorageStrategy, StorageConfig — same
// INTPTimeService — Task<bool> SyncTime()
// IAudioVolumeController — same (already engine-free)
// IRemoteConfigService / IConfigProvider — Task instead of UniTask; still use IConfigModel, ConfigHealthStatus
```

### 4.3 Adapter note

KitchenClash implementers may keep **internal** UniTask usage; public interface methods must match Playcenter signatures (`Task`). Prefer `async Task` methods that `await` UniTask operations.

---

## 5. Asmdef graph

```
Playcenter.GameFlow     → []
Playcenter.Shell        → []
Playcenter.Services     → []          (still no UI)
Playcenter.UI           → []          (no Services)

KitchenClash.Domain     → Shell, Services, UI   (UI only if Domain still needs UIScreenCategory — after move, Domain may drop UI ref if category only used from Presentation)
KitchenClash.Application→ Domain, Shell, Services, UI, UniTask, VContainer
KitchenClash.Presentation → … + Playcenter.UI
Infrastructure leaves   → … + Services and/or UI as needed
```

**Gate:** `rg "Playcenter\.(Services|UI)" Assets/Playcenter/GameFlow Assets/Playcenter/Shell` → empty.  
**Gate:** no `KitchenClash.Application.Services.IUIService` / old storage types remain.

---

## 6. Migration order

1. Expand **Services** contracts (Localization, Storage, Time, Audio volume, RemoteConfig) + hard cutover.  
2. Add **Playcenter.UI** + hard cutover (largest blast radius ~55 files for IUIService).  
3. Docs/wiki + candidates plan supersession.  
4. CLI build gates; EditMode compile; no dual APIs.

Order rationale: Services expansion is smaller and unblocks Domain cleanliness; UI is noisier (Presentation + Flow + tests) and benefits from stable Services first. Either order is valid if done in one program — **prefer Services first, UI second**.

---

## 7. Out of scope

- Extracting Unity Audio playback / Platform / Async leaves into Playcenter  
- Splitting Network mega-asmdef  
- Re-extracting cooking Domain  
- Changing logging wire order (already fixed @ `76f14e35`)  
- Unrelated WIP (maps, fonts, combat, packages-lock)

---

## 8. Success criteria

- [ ] `Assets/Playcenter/UI` exists with README + asmdef `noEngineReferences`  
- [ ] Services Runtime folders for Localization/Storage/Time/Audio/RemoteConfig  
- [ ] All listed originals deleted from Domain/Application  
- [ ] Consumers use `Playcenter.UI` / `Playcenter.Services` only  
- [ ] Builds: Services → UI → Domain → Application → Infrastructure → Presentation → Composition → Editor → EditMode  
- [ ] Grep gates: no dual aliases; GameFlow/Shell independence  
- [ ] Wiki `Technical.md` Playcenter table updated; candidates plan marked superseded for these items  
