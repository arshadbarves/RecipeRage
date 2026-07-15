# Playcenter Foundation Extract Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Extract multi-title foundation contracts into `Playcenter.Services` (expand) and new `Playcenter.UI`, then hard-cutover RecipeRage with zero legacy dual APIs.

**Architecture:** Same bar as Shell/Services: engine-free assemblies under `Assets/Playcenter/*`, `noEngineReferences: true`, zero KitchenClash refs. Async uses `System.Threading.Tasks.Task` (not UniTask). Adapters stay in KitchenClash Infrastructure/Presentation. GameFlow and Shell remain independent (no UI/Services refs). UI and Services do not reference each other.

**Tech Stack:** Unity 6, VContainer, UniTask (game adapters only), NUnit EditMode, existing `Playcenter.*.csproj` CLI pattern, VContainer DI unchanged in shape.

## Global Constraints

- Full cutover: no type aliases, obsolete stubs, dual namespaces, dual UniTask/Task public APIs.
- `Playcenter.GameFlow` and `Playcenter.Shell` must not reference `Playcenter.UI` or `Playcenter.Services`.
- `Playcenter.UI` must not reference Services; Services must not reference UI.
- No Unity types in Playcenter (`AudioClip`, `AudioSource`, `Vector3`, `MonoBehaviour`).
- Do **not** extract: `IAudioService` / clip players, `SFXType`/`MusicTrack`/events, `ISaveService`/`GameSettingsData`, Platform/Async leaves, session/matchmaking/flow handlers, cooking IP, EOS/Firebase concrete.
- Delete Application’s unused `UIScreenPriority` enum inside `IUIService.cs` (Presentation keeps its own numeric enum).
- Do not commit unrelated WIP (maps, fonts, combat, packages-lock, etc.).
- Commit trailer: `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>`
- Spec: `docs/superpowers/specs/2026-07-15-playcenter-foundation-extract-design.md`

---

## File map

| Path | Role |
|------|------|
| `Assets/Playcenter/Services/Runtime/Localization/ILocalizationManager.cs` | NEW port |
| `Assets/Playcenter/Services/Runtime/Storage/*` | NEW storage ports + strategy/config |
| `Assets/Playcenter/Services/Runtime/Time/INTPTimeService.cs` | NEW time port |
| `Assets/Playcenter/Services/Runtime/Audio/IAudioVolumeController.cs` | NEW volume port |
| `Assets/Playcenter/Services/Runtime/RemoteConfig/*` | NEW remote-config ports |
| `Assets/Playcenter/Services/README.md` | Update layout |
| `Assets/Playcenter/UI/Runtime/Playcenter.UI.asmdef` | NEW engine-free wall |
| `Assets/Playcenter/UI/Runtime/IUIService.cs` | NEW UI stack contract |
| `Assets/Playcenter/UI/Runtime/NotificationType.cs` | NEW |
| `Assets/Playcenter/UI/Runtime/UIScreenCategory.cs` | NEW (from Domain) |
| `Assets/Playcenter/UI/README.md` | NEW |
| `Playcenter.Services.csproj` | Add Compile includes for new files |
| `Playcenter.UI.csproj` | NEW CLI project (mirror Services) |
| Delete Application/Domain originals listed per task | Hard purge |
| Asmdefs + consumers | `using Playcenter.Services` / `using Playcenter.UI` |

---

### Task 1: Expand Playcenter.Services with portable ports (module only)

**Files:**
- Create: `Assets/Playcenter/Services/Runtime/Localization/ILocalizationManager.cs`
- Create: `Assets/Playcenter/Services/Runtime/Storage/StorageStrategy.cs`
- Create: `Assets/Playcenter/Services/Runtime/Storage/StorageConfig.cs`
- Create: `Assets/Playcenter/Services/Runtime/Storage/IStorageProvider.cs`
- Create: `Assets/Playcenter/Services/Runtime/Storage/ICloudStorageProvider.cs`
- Create: `Assets/Playcenter/Services/Runtime/Time/INTPTimeService.cs`
- Create: `Assets/Playcenter/Services/Runtime/Audio/IAudioVolumeController.cs`
- Create: `Assets/Playcenter/Services/Runtime/RemoteConfig/IRemoteConfigService.cs`
- Create: `Assets/Playcenter/Services/Runtime/RemoteConfig/IConfigProvider.cs`
- Modify: `Playcenter.Services.csproj` — add `<Compile Include=...>` for each new file
- Modify: `Assets/Playcenter/Services/README.md` — document new folders

**Interfaces:**
- Consumes: existing `IConfigModel`, `ConfigHealthStatus` in `Playcenter.Services`
- Produces: all new types in namespace `Playcenter.Services`

- [ ] **Step 1: Add Localization**

```csharp
// Assets/Playcenter/Services/Runtime/Localization/ILocalizationManager.cs
using System;
using System.Collections.Generic;

namespace Playcenter.Services
{
    public interface ILocalizationManager
    {
        string CurrentLanguage { get; }
        IReadOnlyCollection<string> AvailableLanguages { get; }
        void Initialize();
        void SetLanguage(string languageCode);
        string GetText(string key);
        string GetText(string key, params object[] args);
        bool HasKey(string key);
        void Reload();
        void RegisterBinding(object owner, string key, Action<string> onUpdate);
        void UnregisterAll(object owner);
    }
}
```

- [ ] **Step 2: Add Storage**

```csharp
// StorageStrategy.cs
namespace Playcenter.Services
{
    public enum StorageStrategy
    {
        LocalOnly,
        CloudOnly,
        CloudWithCache
    }
}

// StorageConfig.cs
namespace Playcenter.Services
{
    public sealed class StorageConfig
    {
        public string Key { get; }
        public StorageStrategy Strategy { get; }
        public bool EncryptData { get; }

        public StorageConfig(string key, StorageStrategy strategy, bool encryptData = false)
        {
            Key = key;
            Strategy = strategy;
            EncryptData = encryptData;
        }
    }
}

// IStorageProvider.cs
using System.Threading.Tasks;

namespace Playcenter.Services
{
    public interface IStorageProvider
    {
        bool IsAvailable { get; }
        string Read(string key);
        void Write(string key, string content);
        Task<string> ReadAsync(string key);
        Task WriteAsync(string key, string content);
        bool Exists(string key);
        void Delete(string key);
    }
}

// ICloudStorageProvider.cs
namespace Playcenter.Services
{
    public interface ICloudStorageProvider : IStorageProvider
    {
        void OnUserLoggedIn();
        void OnUserLoggedOut();
    }
}
```

- [ ] **Step 3: Add Time + Audio volume**

```csharp
// INTPTimeService.cs
using System;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    public interface INTPTimeService
    {
        Task<bool> SyncTime();
        DateTime GetServerTime();
        TimeSpan GetTimeOffset();
        bool IsSynced { get; }
        DateTime LastSyncTime { get; }
    }
}

// IAudioVolumeController.cs
namespace Playcenter.Services
{
    public interface IAudioVolumeController
    {
        void SetMasterVolume(float volume);
        void SetMusicVolume(float volume);
        void SetSFXVolume(float volume);
        void SetMute(bool mute);
        float GetMasterVolume();
        float GetMusicVolume();
        float GetSFXVolume();
    }
}
```

- [ ] **Step 4: Add RemoteConfig ports**

```csharp
// IRemoteConfigService.cs
using System;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    public interface IRemoteConfigService
    {
        Task<bool> Initialize();
        T GetConfig<T>() where T : class, IConfigModel;
        bool TryGetConfig<T>(out T config) where T : class, IConfigModel;
        Task<bool> RefreshConfig();
        Task<bool> RefreshConfig<T>() where T : class, IConfigModel;
        ConfigHealthStatus HealthStatus { get; }
        DateTime LastUpdateTime { get; }
    }
}

// IConfigProvider.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    public interface IConfigProvider
    {
        string ProviderName { get; }
        bool IsAvailable();
        Task<bool> Initialize();
        Task<T> FetchConfig<T>(string key) where T : IConfigModel;
        Task<Dictionary<string, IConfigModel>> FetchAllConfigs();
    }
}
```

- [ ] **Step 5: Update Services README layout section**

Replace “Not here” bullets that claim Localization/Storage/Time/Audio volume/RemoteConfig stay game-side. New layout:

```
Runtime/
  …existing…
  Localization/  ILocalizationManager
  Storage/       IStorageProvider, ICloudStorageProvider, StorageStrategy, StorageConfig
  Time/          INTPTimeService
  Audio/         IAudioVolumeController
  RemoteConfig/  IRemoteConfigService, IConfigProvider
```

Still not here: clip-based audio, UI stack (`Playcenter.UI`), save DTOs, Platform/Async Unity helpers, cooking IP.

- [ ] **Step 6: Add Compile includes to `Playcenter.Services.csproj`**

Add one `<Compile Include="Assets/Playcenter/Services/Runtime/..."/>` per new `.cs` file next to existing Services Compile items.

- [ ] **Step 7: Build Services**

Run: `dotnet build Playcenter.Services.csproj -nologo -v q`  
Expected: 0 errors

- [ ] **Step 8: Commit**

```bash
git add Assets/Playcenter/Services Playcenter.Services.csproj
git commit -m "$(cat <<'EOF'
feat(services): add localization, storage, time, audio volume, remote-config ports

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 2: Hard-cutover Services expansion consumers

**Files:**
- Delete: `Assets/_KitchenClash/Application/Services/ILocalizationManager.cs`
- Delete: `Assets/_KitchenClash/Application/Services/INTPTimeService.cs`
- Delete: `Assets/_KitchenClash/Application/Services/IRemoteConfigService.cs`
- Delete: `Assets/_KitchenClash/Application/Services/IConfigProvider.cs`
- Delete: `Assets/_KitchenClash/Application/Interfaces/IStorageProvider.cs`
- Delete: `Assets/_KitchenClash/Application/Interfaces/ICloudStorageProvider.cs`
- Delete: `Assets/_KitchenClash/Application/Interfaces/IAudioVolumeController.cs`
- Delete: `Assets/_KitchenClash/Domain/Enums/StorageStrategy.cs`
- Delete: `Assets/_KitchenClash/Domain/Models/StorageConfig.cs`
- Modify: `Assets/_KitchenClash/Application/Services/NTPTime.cs` — `using Playcenter.Services;` (keep class in `KitchenClash.Application.Services`)
- Modify: `Assets/_KitchenClash/Application/Interfaces/ISaveService.cs` — `using Playcenter.Services;` for `StorageStrategy`
- Modify implementers (public API `Task`, not `UniTask`):
  - `Assets/_KitchenClash/Infrastructure/Localization/LocalizationManager.cs`
  - `Assets/_KitchenClash/Infrastructure/Persistence/LocalStorageProvider.cs`
  - `Assets/_KitchenClash/Infrastructure/EOS/EOSCloudStorageProvider.cs`
  - `Assets/_KitchenClash/Infrastructure/Network/NTPTimeService.cs`
  - `Assets/_KitchenClash/Infrastructure/Audio/AudioVolumeController.cs`
  - `Assets/_KitchenClash/Infrastructure/Services/CompositeRemoteConfigService.cs`
  - `Assets/_KitchenClash/Infrastructure/Services/FallbackRemoteConfigService.cs`
  - `Assets/_KitchenClash/Infrastructure/Firebase/FirebaseConfigProvider.cs`
- Modify consumers that import old namespaces (Application services, Flow handlers, Composition, tests, Persistence SaveService, etc.)
- Modify: any file with `using KitchenClash.Domain` only for `StorageStrategy`/`StorageConfig` → `using Playcenter.Services`

**Interfaces:**
- Consumes: Task 1 ports
- Produces: game adapters implementing `Playcenter.Services` contracts

- [ ] **Step 1: Convert storage adapters to Task**

In `LocalStorageProvider` and `EOSCloudStorageProvider`, change:

```csharp
// before
public async UniTask<string> ReadAsync(string key)
public async UniTask WriteAsync(string key, string content)

// after
public async Task<string> ReadAsync(string key)
public async Task WriteAsync(string key, string content)
```

Keep internal UniTask/EOS callbacks if needed; `await` them from `async Task` methods.  
Replace fire-and-forget:

```csharp
// before
WriteAsync(key, content).Forget();

// after
_ = WriteAsync(key, content);
```

Add `using System.Threading.Tasks;` and `using Playcenter.Services;`. Remove Application interface usings for deleted types.

- [ ] **Step 2: Convert NTP + RemoteConfig adapters to Task**

```csharp
// NTPTimeService
public async Task<bool> SyncTime() { /* body unchanged; internal await UniTask OK */ }

// FallbackRemoteConfigService
public Task<bool> Initialize() => Task.FromResult(true);
public Task<bool> RefreshConfig() => Task.FromResult(true);
public Task<bool> RefreshConfig<T>() where T : class, IConfigModel => Task.FromResult(true);

// CompositeRemoteConfigService + FirebaseConfigProvider: async Task instead of async UniTask
// Existing: public Task FetchAsync() => RefreshConfig().AsTask();
// After:    public Task FetchAsync() => RefreshConfig();
```

- [ ] **Step 3: Fix BootSequence cancellation on Task**

`Assets/_KitchenClash/Infrastructure/Flow/Handlers/BootSequence.cs` currently:

```csharp
await _ntpTimeService.SyncTime().AttachExternalCancellation(ntpCts.Token).SuppressCancellationThrow();
```

Replace with:

```csharp
try
{
    await _ntpTimeService.SyncTime().ConfigureAwait(false);
}
catch (OperationCanceledException)
{
    // boot continues with local clock if NTP cancelled/timed out
}
// If ntpCts is still required, race Task against delay:
// var sync = _ntpTimeService.SyncTime();
// var completed = await Task.WhenAny(sync, Task.Delay(timeout, ntpCts.Token));
// if (completed == sync) await sync;
```

Prefer preserving existing timeout behavior with `Task.WhenAny` + existing timeout ms constant if present in file.

- [ ] **Step 4: Update all usings + delete originals**

```bash
# Find remaining old type homes (must be empty after delete)
rg -n "KitchenClash\.Application\.Services\.(ILocalizationManager|INTPTimeService|IRemoteConfigService|IConfigProvider)" --glob '*.cs' || true
rg -n "namespace KitchenClash\.(Application|Domain)" Assets/_KitchenClash/Application/Interfaces/IStorageProvider.cs 2>/dev/null || true
```

For every consumer file, ensure `using Playcenter.Services;` and remove obsolete Application/Domain imports for moved types only.

`NTPTime` static helper **stays** in Application:

```csharp
using System;
using Playcenter.Services;

namespace KitchenClash.Application.Services
{
    public static class NTPTime
    {
        private static INTPTimeService _instance;
        // … unchanged body …
    }
}
```

- [ ] **Step 5: Build chain**

```bash
dotnet build Playcenter.Services.csproj -nologo -v q
dotnet build KitchenClash.Domain.csproj -nologo -v q
dotnet build KitchenClash.Application.csproj -nologo -v q
dotnet build KitchenClash.Infrastructure.Localization.csproj -nologo -v q
dotnet build KitchenClash.Infrastructure.Persistence.csproj -nologo -v q
dotnet build KitchenClash.Infrastructure.Audio.csproj -nologo -v q
dotnet build KitchenClash.Infrastructure.csproj -nologo -v q
dotnet build KitchenClash.Infrastructure.EOS.csproj -nologo -v q
dotnet build KitchenClash.Infrastructure.Flow.csproj -nologo -v q
dotnet build KitchenClash.Composition.csproj -nologo -v q
dotnet build RecipeRage.Tests.EditMode.csproj -nologo -v q
```

Expected: 0 errors. If Unity-generated csproj missing Compile entries for new Services files, add them the same way as Task 1 (or ensure asmdef auto-includes under `Assets/Playcenter/Services/Runtime/**`).

- [ ] **Step 6: Grep gates**

```bash
# No dual aliases
rg -n "using ILocalizationManager|global using|Obsolete.*IStorageProvider" --glob '*.cs' || true
# Originals gone
test ! -f Assets/_KitchenClash/Application/Services/ILocalizationManager.cs
test ! -f Assets/_KitchenClash/Domain/Enums/StorageStrategy.cs
# GameFlow/Shell still independent of Services (already true; re-check)
rg -n "Playcenter\.Services" Assets/Playcenter/GameFlow Assets/Playcenter/Shell && exit 1 || true
```

- [ ] **Step 7: Commit**

```bash
git add -A Assets/_KitchenClash Assets/Scripts/Tests Assets/Playcenter/Services
# do not stage unrelated WIP
git commit -m "$(cat <<'EOF'
feat(services): hard-cutover localization, storage, time, volume, remote-config

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 3: Create Playcenter.UI module

**Files:**
- Create: `Assets/Playcenter/UI/Runtime/Playcenter.UI.asmdef`
- Create: `Assets/Playcenter/UI/Runtime/NotificationType.cs`
- Create: `Assets/Playcenter/UI/Runtime/UIScreenCategory.cs`
- Create: `Assets/Playcenter/UI/Runtime/IUIService.cs`
- Create: `Assets/Playcenter/UI/README.md`
- Create: `Playcenter.UI.csproj` (copy structure from `Playcenter.Services.csproj`; change name/guid/Compile list)

**Interfaces:**
- Produces: `Playcenter.UI.IUIService`, `NotificationType`, `UIScreenCategory`

- [ ] **Step 1: Create asmdef**

```json
{
    "name": "Playcenter.UI",
    "rootNamespace": "Playcenter.UI",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": true
}
```

- [ ] **Step 2: Add enums**

```csharp
// NotificationType.cs
namespace Playcenter.UI
{
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }
}

// UIScreenCategory.cs
namespace Playcenter.UI
{
    public enum UIScreenCategory
    {
        System = 0,
        Overlay = 1,
        Modal = 2,
        Popup = 3,
        Screen = 4,
        HUD = 5,
        Toast = 6
    }
}
```

- [ ] **Step 3: Add IUIService (Task-based, object scope)**

Copy method surface from `Assets/_KitchenClash/Application/Services/IUIService.cs` with these **required** changes:

```csharp
using System;
using System.Threading.Tasks;

namespace Playcenter.UI
{
    public interface IUIService
    {
        bool IsInitialized { get; }

        void SetRootScreen<T>(bool animate = true) where T : class;
        void SetRootScreen(Type screenType, bool animate = true);
        void PushScreen<T>(bool animate = true) where T : class;
        void PushScreen(Type screenType, bool animate = true);
        void ShowSystem<T>(bool animate = true) where T : class;
        void ShowSystem(Type screenType, bool animate = true);
        void HideSystem<T>(bool animate = true) where T : class;
        void HideSystem(Type screenType, bool animate = true);
        void ShowOverlay<T>(bool animate = true) where T : class;
        void ShowOverlay(Type screenType, bool animate = true);
        void HideOverlay<T>(bool animate = true) where T : class;
        void HideOverlay(Type screenType, bool animate = true);
        void PushModal<T>(bool animate = true) where T : class;
        void PushModal(Type screenType, bool animate = true);
        void PushPopup<T>(bool animate = true) where T : class;
        void PushPopup(Type screenType, bool animate = true);
        void ShowHud<T>(bool animate = true) where T : class;
        void ShowHud(Type screenType, bool animate = true);
        void HideHud<T>(bool animate = true) where T : class;
        void HideHud(Type screenType, bool animate = true);
        bool Back(bool animate = true);

        Task ShowToast(string message, NotificationType type = NotificationType.Info, float duration = 3f);
        Task ShowToast(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f);

        void Show<T>(bool animate = true, bool addToHistory = true) where T : class;
        void Show(Type screenType, bool animate = true, bool addToHistory = true);
        void Hide<T>(bool animate = true) where T : class;
        void Hide(Type screenType, bool animate = true);

        void HideAllPopups(bool animate = true);
        void HideAllModals(bool animate = true);
        void HideAllGameScreens(bool animate = true);
        void HideAllScreens(bool animate = false);

        T GetScreen<T>() where T : class;

        bool IsScreenVisible<T>() where T : class;
        bool IsScreenVisible(Type screenType);

        bool GoBack(bool animate = true);
        void ClearHistory();

        Task ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 3f);
        Task ShowNotification(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f);

        event Action<Type> OnScreenShown;
        event Action<Type> OnScreenHidden;
        event Action OnAllScreensHidden;

        /// <summary>Game host passes its DI resolver (e.g. VContainer IObjectResolver). Playcenter stays engine-free.</summary>
        void SetCurrentScope(object scope);
        void Update(float deltaTime);
    }
}
```

Do **not** include Application’s `UIScreenPriority` enum in Playcenter.UI.

- [ ] **Step 4: README**

```markdown
# Playcenter.UI

Engine-free **UI stack contracts** for multi-title Brawl shells.

## Purpose

- Type-based screen navigation (root/push/system/overlay/modal/popup/HUD)
- Toast/notification surface
- Screen category enum for layering

## Rules

1. `noEngineReferences` — no UI Toolkit / Unity / VContainer / UniTask types.
2. Async methods return `Task`.
3. `SetCurrentScope(object)` — game casts to its DI resolver.
4. GameFlow/Shell/Services do not reference UI.
5. Adapters (`UIService`, screens) stay in KitchenClash.Presentation.

## Layout

```
Runtime/
  Playcenter.UI.asmdef
  IUIService.cs
  NotificationType.cs
  UIScreenCategory.cs
```
```

- [ ] **Step 5: CLI csproj**

Clone `Playcenter.Services.csproj` → `Playcenter.UI.csproj`: change `RootNamespace`, `AssemblyName`, `ProjectGuid` (new GUID), `OutputPath`, and Compile includes to the three Runtime `.cs` files only.

- [ ] **Step 6: Build UI**

Run: `dotnet build Playcenter.UI.csproj -nologo -v q`  
Expected: 0 errors

- [ ] **Step 7: Commit**

```bash
git add Assets/Playcenter/UI Playcenter.UI.csproj
git commit -m "$(cat <<'EOF'
feat(ui): add Playcenter.UI module (screen stack contracts)

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 4: Hard-cutover Playcenter.UI consumers

**Files:**
- Delete: `Assets/_KitchenClash/Application/Services/IUIService.cs` (includes dead Application `UIScreenPriority` + old `NotificationType`)
- Delete: `Assets/_KitchenClash/Domain/Enums/UIScreenCategory.cs`
- Modify asmdefs — add `"Playcenter.UI"` reference:
  - `Assets/_KitchenClash/Application/KitchenClash.Application.asmdef`
  - `Assets/_KitchenClash/Presentation/KitchenClash.Presentation.asmdef`
  - `Assets/_KitchenClash/Composition/…` if present and needed
  - Infrastructure Flow / Network / any leaf that uses `IUIService`
  - `Assets/Scripts/Tests/RecipeRage.Tests.EditMode.asmdef`
  - Domain **only if** something in Domain still needs `UIScreenCategory` (after delete, Domain should **not** need UI — prefer no Domain→UI ref)
- Modify Presentation:
  - `UIService.cs` — `SetCurrentScope(object scope)` cast to `IObjectResolver`
  - `UIService.Navigation.cs` — `async Task` toast/notification
  - All `using KitchenClash.Domain` for category → `using Playcenter.UI`
  - All `IUIService` / `NotificationType` imports → `Playcenter.UI`
- Modify Flow handlers, GameStarter, SessionManager, tests (`FakeUIService`), ViewModels, screens/overlays (~55 files for `IUIService` family)

**Interfaces:**
- Consumes: Task 3 contracts
- Produces: working game UI host on Playcenter.UI

- [ ] **Step 1: Wire asmdefs**

Add `"Playcenter.UI"` to Application, Presentation, Infrastructure (main + Flow + Network as needed), Composition, EditMode tests. Do **not** add to GameFlow/Shell/Services.

- [ ] **Step 2: Update UIService implementation**

```csharp
// UIService.cs
public void SetCurrentScope(object scope)
{
    _currentScope = scope as VContainer.IObjectResolver;
    // existing null-safe behavior when scope is null (session teardown)
}

// UIService.Navigation.cs
public async Task ShowToast(string message, NotificationType type = NotificationType.Info, float duration = 3f)
{
    // existing body; UniTask internals OK if awaited
}

public async Task ShowToast(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f) { … }

public async Task ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 3f)
{
    await ShowToast(message, type, duration);
}

public async Task ShowNotification(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f)
{
    await ShowToast(title, message, type, duration);
}
```

Usings at top of Presentation Common files:

```csharp
using Playcenter.UI;
// remove KitchenClash.Domain if only used for UIScreenCategory
// remove KitchenClash.Application.Services if only used for IUIService/NotificationType
```

- [ ] **Step 3: Update FakeUIService in tests**

`Assets/Scripts/Tests/EditMode/Gameplay/MatchmakingFlowTests.cs`:

```csharp
using Playcenter.UI;
// FakeUIService : IUIService
public Task ShowToast(...) => Task.CompletedTask;
public Task ShowNotification(...) => Task.CompletedTask;
public void SetCurrentScope(object scope) { }
```

- [ ] **Step 4: Bulk consumer using fix**

For each file from:

```bash
rg -l "IUIService|NotificationType|UIScreenCategory" --glob '*.cs' -g '!Assets/Playcenter/**'
```

Ensure types resolve from `Playcenter.UI`. Presentation stack types (`IUIScreenStackManager`, attributes) keep using `UIScreenCategory` from Playcenter.UI.

- [ ] **Step 5: Delete originals**

Delete Application `IUIService.cs` and Domain `UIScreenCategory.cs` (+ metas).

- [ ] **Step 6: Build chain**

```bash
dotnet build Playcenter.UI.csproj -nologo -v q
dotnet build KitchenClash.Application.csproj -nologo -v q
dotnet build KitchenClash.Presentation.csproj -nologo -v q
dotnet build KitchenClash.Infrastructure.Flow.csproj -nologo -v q
dotnet build KitchenClash.Infrastructure.csproj -nologo -v q
dotnet build KitchenClash.Composition.csproj -nologo -v q
dotnet build RecipeRage.Tests.EditMode.csproj -nologo -v q
dotnet build RecipeRage.Editor.csproj -nologo -v q
```

Expected: 0 errors. Fix any missing asmdef ProjectReference / Compile include for Playcenter.UI in Unity-generated csprojs if CLI fails to resolve the assembly (add ProjectReference to `Playcenter.UI.csproj` mirroring Services).

- [ ] **Step 7: Grep gates**

```bash
test ! -f Assets/_KitchenClash/Application/Services/IUIService.cs
test ! -f Assets/_KitchenClash/Domain/Enums/UIScreenCategory.cs
rg -n "KitchenClash\.Application\.Services\.IUIService|namespace KitchenClash\.Application\.Services\s*\{[^}]*IUIService" --glob '*.cs' && exit 1 || true
rg -n "Playcenter\.UI" Assets/Playcenter/GameFlow Assets/Playcenter/Shell Assets/Playcenter/Services && exit 1 || true
rg -n "UniTask ShowToast|UniTask ShowNotification|SetCurrentScope\(VContainer" --glob '*.cs' && exit 1 || true
```

- [ ] **Step 8: Commit**

```bash
git add Assets/Playcenter/UI Assets/_KitchenClash Assets/Scripts/Tests Playcenter.UI.csproj
git commit -m "$(cat <<'EOF'
feat(ui): hard-cutover IUIService and screen categories to Playcenter.UI

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 5: Docs, wiki, candidates supersession

**Files:**
- Modify: `wiki/Technical.md` — Playcenter table rows for UI + Services expansion; remove “Audio/UI deferred” for extracted contracts; keep clip-audio/Platform/Async deferred
- Modify: `wiki/log.md` — append entry
- Modify: `docs/superpowers/plans/2026-07-14-playcenter-module-extract-candidates.md` — supersession banner for foundation extract
- Modify: `Assets/Playcenter/Services/README.md` if any leftover “not here” lies
- Optional: short note in `wiki/GameFlow-SDK.md` only if it lists module inventory

- [ ] **Step 1: Update Technical.md Playcenter table**

```markdown
| `Playcenter.Services` | **Shipped** | … + Localization, Storage, Time, Audio volume, RemoteConfig ports |
| `Playcenter.UI` | **Shipped** | Engine-free screen stack + NotificationType + UIScreenCategory; UIService adapter in Presentation |
| Clip audio / Platform / Async leaves | **Stay KitchenClash** | Unity-bound helpers; not Playcenter |
```

Hard cutover rules: include UI. Logging wire order paragraph unchanged.

- [ ] **Step 2: Candidates plan banner**

At top of candidates plan, add:

```markdown
> **Superseded for foundation ports (2026-07-15):** Localization, Storage, Time, Audio volume, RemoteConfig → Services expansion; UI stack → `Playcenter.UI`. See `docs/superpowers/specs/2026-07-15-playcenter-foundation-extract-design.md` and `docs/superpowers/plans/2026-07-15-playcenter-foundation-extract.md`. Still deferred: clip-based audio, Platform/Async Unity leaves, save DTOs, cooking IP.
```

- [ ] **Step 3: wiki/log.md entry**

One line: foundation extract Services expand + Playcenter.UI hard cutover; Task-based ports; no dual APIs.

- [ ] **Step 4: Final gates + commit**

```bash
dotnet build Playcenter.Services.csproj -nologo -v q
dotnet build Playcenter.UI.csproj -nologo -v q
rg -n "Playcenter\.(Services|UI)" Assets/Playcenter/GameFlow Assets/Playcenter/Shell && exit 1 || true
```

```bash
git add wiki docs/superpowers Assets/Playcenter
git commit -m "$(cat <<'EOF'
docs(playcenter): document foundation extract (Services expand + UI)

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

## Self-review (plan author)

| Spec requirement | Task |
|------------------|------|
| Expand Services: Localization, Storage, Time, Audio volume, RemoteConfig | Task 1–2 |
| New Playcenter.UI: IUIService, NotificationType, UIScreenCategory | Task 3–4 |
| Task not UniTask; object scope not VContainer | Task 1 contracts + Task 3–4 |
| Delete Application UIScreenPriority with IUIService | Task 4 delete |
| No clip audio / SFX enums / SaveService DTOs / Platform / Async | Global constraints + Task 5 docs |
| Hard cutover, independence gates | Tasks 2, 4, 5 |
| Wiki + candidates supersession | Task 5 |

**Placeholder scan:** none intentional.  
**Type consistency:** `Playcenter.Services` / `Playcenter.UI` namespaces; `Task`/`Task<T>` throughout ports; `SetCurrentScope(object)`.

---

## Execution handoff

Plan complete and saved to `docs/superpowers/plans/2026-07-15-playcenter-foundation-extract.md`.

**Two execution options:**

1. **Subagent-Driven (recommended)** — fresh subagent per task, review between tasks  
2. **Inline Execution** — execute in this session with executing-plans checkpoints  

**Which approach?**
