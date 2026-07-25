# Phase 0: Playcenter SDK Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the complete Playcenter SDK foundation — core services (EventBus, Logging, Time), full-logic services (Auth, Config, Storage, Analytics, Ads, IAP, Friends, Audio, Save, Wallet), ServiceLocator DI, and the dual composition root boot system.

**Architecture:** Manual DI with two composition roots. `PlaycenterCompositionRoot` (MonoBehaviour) constructs and initializes all SDK services, registers them in a static `ServiceLocator`, then fires `OnPlaycenterInitialized`. `GameplayCompositionRoot` listens for that event before constructing game services. All SDK services contain FULL logic; the game side only consumes interfaces.

**Tech Stack:** Unity 6000.3.0f1, C# (.NET Standard 2.1), Unity Audio Mixer, Addressables, Input System. External SDKs (Firebase, EOS, AdMob, Unity IAP, Unity Gaming Services Friends, Facebook/Google login) are integrated behind interfaces with stub/fake implementations first — real SDK wiring happens in Slice 2/5 where credentials exist.

## Global Constraints

- Namespace root: `Playcenter.*` for SDK, `RecipeRage.*` for game code
- Folder root: `Assets/Playcenter/` for SDK, `Assets/Game/` for game code
- No VContainer, no third-party DI — manual composition root only
- No `FindObjectOfType` / `NetworkManager.Singleton` in new code
- EventBus is custom lightweight pub/sub (mobile-optimized, no per-frame allocs)
- All services are interfaces + concrete implementations (FULL logic in SDK)
- 4-space indentation, CRLF line endings, explicit accessibility modifiers, no `this.` qualification
- Testing policy: no tests initially (project owner decision) — verification is by compiling and running the boot scene
- Coins are only earned and spent, never lost. Trophies: win +15, loss -8
- Auth providers: Facebook, Google, Guest (NO Epic account login)
- Commit format: `type(scope): description` with `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>` trailer

---

### Task 1: Project Scaffolding + Assembly Definitions

**Files:**
- Create: `Assets/Playcenter/Core/Playcenter.Core.asmdef`
- Create: `Assets/Playcenter/Services/Playcenter.Services.asmdef`
- Create: `Assets/Playcenter/UI/Playcenter.UI.asmdef`
- Create: `Assets/Playcenter/Net/Playcenter.Net.asmdef`
- Create: `Assets/Game/RecipeRage.Gameplay.asmdef`
- Create: `Assets/Game/RecipeRage.UI.asmdef`

**Interfaces:**
- Consumes: nothing
- Produces: assembly boundaries used by every later task

- [ ] **Step 1: Create folder structure**

```bash
mkdir -p Assets/Playcenter/{Core/{DI,Events,Logging,Time},Services/{Auth,Config,Storage,Analytics,Ads,IAP,Friends,Audio,Save,Wallet},UI/{IUIService,UIToolkit},Net/{INetService,EOS}}
mkdir -p Assets/Game/{DI,Gameplay/{Player,Ingredient,Station,Recipe,Cooking,Match,Tutorial,Indicators},Network,Bots/Evaluators,Progression/{Chef,Trophy},Monetization/Cosmetics,UI/{Screens,Components,Animations}}
mkdir -p Assets/Art/{Characters,Maps,UI,VFX}
mkdir -p Assets/Scenes
```

- [ ] **Step 2: Create Playcenter.Core.asmdef**

`Assets/Playcenter/Core/Playcenter.Core.asmdef`:
```json
{
    "name": "Playcenter.Core",
    "rootNamespace": "Playcenter",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "autoReferenced": true,
    "noEngineReferences": false
}
```

- [ ] **Step 3: Create Playcenter.Services.asmdef**

`Assets/Playcenter/Services/Playcenter.Services.asmdef`:
```json
{
    "name": "Playcenter.Services",
    "rootNamespace": "Playcenter.Services",
    "references": [
        "Playcenter.Core"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "autoReferenced": true,
    "noEngineReferences": false
}
```

- [ ] **Step 4: Create Playcenter.UI.asmdef**

`Assets/Playcenter/UI/Playcenter.UI.asmdef`:
```json
{
    "name": "Playcenter.UI",
    "rootNamespace": "Playcenter.UI",
    "references": [
        "Playcenter.Core",
        "Playcenter.Services"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "autoReferenced": true,
    "noEngineReferences": false
}
```

- [ ] **Step 5: Create Playcenter.Net.asmdef**

`Assets/Playcenter/Net/Playcenter.Net.asmdef`:
```json
{
    "name": "Playcenter.Net",
    "rootNamespace": "Playcenter.Net",
    "references": [
        "Playcenter.Core",
        "Playcenter.Services",
        "Unity.Netcode.Runtime"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "autoReferenced": true,
    "noEngineReferences": false
}
```

- [ ] **Step 6: Create RecipeRage.Gameplay.asmdef**

`Assets/Game/RecipeRage.Gameplay.asmdef`:
```json
{
    "name": "RecipeRage.Gameplay",
    "rootNamespace": "RecipeRage",
    "references": [
        "Playcenter.Core",
        "Playcenter.Services"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "autoReferenced": true,
    "noEngineReferences": false
}
```

- [ ] **Step 7: Create RecipeRage.UI.asmdef**

`Assets/Game/RecipeRage.UI.asmdef`:
```json
{
    "name": "RecipeRage.UI",
    "rootNamespace": "RecipeRage.UI",
    "references": [
        "Playcenter.Core",
        "Playcenter.Services",
        "Playcenter.UI",
        "RecipeRage.Gameplay"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "autoReferenced": true,
    "noEngineReferences": false
}
```

- [ ] **Step 8: Verify compilation**

Open the project in Unity (or let it recompile) and confirm zero compile errors. Expected: 6 new assemblies compile cleanly.

- [ ] **Step 9: Commit**

```bash
git add Assets/Playcenter Assets/Game Assets/Art Assets/Scenes
git commit -m "chore(scaffold): folder structure + assembly definitions for rebuild"
```

---

### Task 2: ServiceLocator + EventBus (Core Primitives)

**Files:**
- Create: `Assets/Playcenter/Core/DI/ServiceLocator.cs`
- Create: `Assets/Playcenter/Core/Events/IEventBus.cs`
- Create: `Assets/Playcenter/Core/Events/EventBus.cs`

**Interfaces:**
- Consumes: nothing
- Produces:
  - `ServiceLocator.Register<T>(T instance)`, `ServiceLocator.Get<T>()`, `ServiceLocator.TryGet<T>(out T)`, `ServiceLocator.Clear()`
  - `IEventBus.Publish<T>(T evt)`, `IEventBus.Subscribe<T>(Action<T>)`, `IEventBus.Unsubscribe<T>(Action<T>)`

- [ ] **Step 1: Write ServiceLocator**

`Assets/Playcenter/Core/DI/ServiceLocator.cs`:
```csharp
using System;
using System.Collections.Generic;

namespace Playcenter
{
    /// <summary>
    /// Static service registry. Registered by composition roots, consumed everywhere.
    /// Not a container — no construction, no lifetimes, just lookup.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new Dictionary<Type, object>(64);

        public static void Register<T>(T instance) where T : class
        {
            Services[typeof(T)] = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        public static T Get<T>() where T : class
        {
            if (Services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            throw new InvalidOperationException($"Service not registered: {typeof(T).Name}");
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            if (Services.TryGetValue(typeof(T), out var raw))
            {
                service = (T)raw;
                return true;
            }
            service = null;
            return false;
        }

        public static void Clear()
        {
            Services.Clear();
        }
    }
}
```

- [ ] **Step 2: Write IEventBus**

`Assets/Playcenter/Core/Events/IEventBus.cs`:
```csharp
using System;

namespace Playcenter
{
    /// <summary>
    /// Lightweight typed pub/sub. Gameplay publishes, systems subscribe. No per-frame allocs.
    /// </summary>
    public interface IEventBus
    {
        void Publish<T>(T eventData);
        void Subscribe<T>(Action<T> handler);
        void Unsubscribe<T>(Action<T> handler);
        void Clear();
    }
}
```

- [ ] **Step 3: Write EventBus**

`Assets/Playcenter/Core/Events/EventBus.cs`:
```csharp
using System;
using System.Collections.Generic;

namespace Playcenter
{
    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>(64);

        public void Publish<T>(T eventData)
        {
            if (_handlers.TryGetValue(typeof(T), out var handlers))
            {
                ((Action<T>)handlers)?.Invoke(eventData);
            }
        }

        public void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var existing))
            {
                _handlers[type] = Delegate.Combine(existing, handler);
            }
            else
            {
                _handlers[type] = handler;
            }
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var existing))
            {
                var updated = Delegate.Remove(existing, handler);
                if (updated == null)
                {
                    _handlers.Remove(type);
                }
                else
                {
                    _handlers[type] = updated;
                }
            }
        }

        public void Clear()
        {
            _handlers.Clear();
        }
    }
}
```

- [ ] **Step 4: Verify compilation**

Expected: `Playcenter.Core` assembly compiles with zero errors.

- [ ] **Step 5: Commit**

```bash
git add Assets/Playcenter/Core
git commit -m "feat(core): ServiceLocator + EventBus primitives"
```

---

### Task 3: Logging + Time Services

**Files:**
- Create: `Assets/Playcenter/Core/Logging/ILoggingService.cs`
- Create: `Assets/Playcenter/Core/Logging/UnityLoggingService.cs`
- Create: `Assets/Playcenter/Core/Time/ITimeService.cs`
- Create: `Assets/Playcenter/Core/Time/UnityTimeService.cs`

**Interfaces:**
- Consumes: nothing
- Produces:
  - `ILoggingService.Log(string)`, `.LogWarning(string)`, `.LogError(string)`
  - `ITimeService.Time`, `.DeltaTime`, `.UnscaledTime`, `.UnscaledDeltaTime`

- [ ] **Step 1: Write logging**

`Assets/Playcenter/Core/Logging/ILoggingService.cs`:
```csharp
namespace Playcenter
{
    public interface ILoggingService
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}
```

`Assets/Playcenter/Core/Logging/UnityLoggingService.cs`:
```csharp
using UnityEngine;

namespace Playcenter
{
    public sealed class UnityLoggingService : ILoggingService
    {
        public void Log(string message) => Debug.Log(message);
        public void LogWarning(string message) => Debug.LogWarning(message);
        public void LogError(string message) => Debug.LogError(message);
    }
}
```

- [ ] **Step 2: Write time service**

`Assets/Playcenter/Core/Time/ITimeService.cs`:
```csharp
namespace Playcenter
{
    public interface ITimeService
    {
        float Time { get; }
        float DeltaTime { get; }
        float UnscaledTime { get; }
        float UnscaledDeltaTime { get; }
    }
}
```

`Assets/Playcenter/Core/Time/UnityTimeService.cs`:
```csharp
namespace Playcenter
{
    public sealed class UnityTimeService : ITimeService
    {
        public float Time => UnityEngine.Time.time;
        public float DeltaTime => UnityEngine.Time.deltaTime;
        public float UnscaledTime => UnityEngine.Time.unscaledTime;
        public float UnscaledDeltaTime => UnityEngine.Time.unscaledDeltaTime;
    }
}
```

- [ ] **Step 3: Verify compilation + commit**

```bash
git add Assets/Playcenter/Core/Logging Assets/Playcenter/Core/Time
git commit -m "feat(core): logging + time services"
```

---

### Task 4: Save + Storage Services (EOS Cloud Storage Interface)

**Files:**
- Create: `Assets/Playcenter/Services/Storage/IStorageService.cs`
- Create: `Assets/Playcenter/Services/Storage/EOSCloudStorageService.cs`
- Create: `Assets/Playcenter/Services/Save/ISaveService.cs`
- Create: `Assets/Playcenter/Services/Save/EOSCloudSaveService.cs`

**Interfaces:**
- Consumes: `ILoggingService`
- Produces:
  - `IStorageService.Initialize()` → `IEnumerator`, `.WriteFile(string key, byte[] data)` → `Task<bool>`, `.ReadFile(string key)` → `Task<byte[]>`
  - `ISaveService.Save<T>(string key, T value)`, `.Load<T>(string key, T fallback)`, `.Delete(string key)`, `.Flush()` → `Task`

- [ ] **Step 1: Write IStorageService**

`Assets/Playcenter/Services/Storage/IStorageService.cs`:
```csharp
using System.Collections;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>
    /// Cloud file storage (EOS Player Data Storage in production).
    /// </summary>
    public interface IStorageService
    {
        bool IsReady { get; }
        IEnumerator Initialize();
        Task<bool> WriteFile(string key, byte[] data);
        Task<byte[]> ReadFile(string key);
        Task<bool> DeleteFile(string key);
    }
}
```

- [ ] **Step 2: Write EOSCloudStorageService (stub-first)**

The real EOS SDK wiring happens in Slice 2 when credentials/config exist. This implementation is the FULL local-persisting logic behind the same interface, so the game works end-to-end today and swaps to EOS transport without call-site changes.

`Assets/Playcenter/Services/Storage/EOSCloudStorageService.cs`:
```csharp
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Playcenter.Services
{
    /// <summary>
    /// EOS Player Data Storage provider. Until the EOS transport is wired (Slice 2),
    /// persists to Application.persistentDataPath under the same interface.
    /// </summary>
    public sealed class EOSCloudStorageService : IStorageService
    {
        private readonly ILoggingService _log;
        private string _rootPath;

        public bool IsReady { get; private set; }

        public EOSCloudStorageService(ILoggingService log)
        {
            _log = log;
        }

        public IEnumerator Initialize()
        {
            _rootPath = Path.Combine(Application.persistentDataPath, "cloud");
            Directory.CreateDirectory(_rootPath);
            IsReady = true;
            _log.Log("[Storage] Initialized (local-persist mode, EOS transport pending)");
            yield break;
        }

        public Task<bool> WriteFile(string key, byte[] data)
        {
            try
            {
                File.WriteAllBytes(GetPath(key), data);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _log.LogError($"[Storage] Write failed for {key}: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public Task<byte[]> ReadFile(string key)
        {
            var path = GetPath(key);
            if (!File.Exists(path))
            {
                return Task.FromResult<byte[]>(null);
            }

            try
            {
                return Task.FromResult(File.ReadAllBytes(path));
            }
            catch (Exception ex)
            {
                _log.LogError($"[Storage] Read failed for {key}: {ex.Message}");
                return Task.FromResult<byte[]>(null);
            }
        }

        public Task<bool> DeleteFile(string key)
        {
            var path = GetPath(key);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            return Task.FromResult(true);
        }

        private string GetPath(string key)
        {
            var safeKey = string.Concat(key.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
            return Path.Combine(_rootPath, safeKey + ".dat");
        }
    }
}
```

- [ ] **Step 3: Write ISaveService + EOSCloudSaveService**

`Assets/Playcenter/Services/Save/ISaveService.cs`:
```csharp
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>
    /// Typed key-value save layer on top of IStorageService. JSON-serialized.
    /// </summary>
    public interface ISaveService
    {
        void Save<T>(string key, T value);
        T Load<T>(string key, T fallback);
        bool Has(string key);
        void Delete(string key);
        Task Flush();
    }
}
```

`Assets/Playcenter/Services/Save/EOSCloudSaveService.cs`:
```csharp
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Playcenter.Services
{
    /// <summary>
    /// Save layer backed by IStorageService (EOS Cloud Storage in production).
    /// Writes are cached in memory immediately and flushed to storage on Flush()
    /// (called on pause / quit / match end), so gameplay never blocks on IO.
    /// </summary>
    public sealed class EOSCloudSaveService : ISaveService
    {
        private readonly IStorageService _storage;
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>(64);
        private bool _dirty;

        public EOSCloudSaveService(IStorageService storage)
        {
            _storage = storage;
        }

        public void Save<T>(string key, T value)
        {
            _cache[key] = JsonUtility.ToJson(value);
            _dirty = true;
        }

        public T Load<T>(string key, T fallback)
        {
            if (_cache.TryGetValue(key, out var json))
            {
                return JsonUtility.FromJson<T>(json);
            }

            // Synchronous first-load path: storage read is awaited by caller via Preload.
            return fallback;
        }

        public bool Has(string key) => _cache.ContainsKey(key);

        public void Delete(string key)
        {
            _cache.Remove(key);
            _dirty = true;
        }

        /// <summary>
        /// Preloads keys from storage into memory cache. Called once after auth.
        /// </summary>
        public async Task Preload(string[] keys)
        {
            foreach (var key in keys)
            {
                var bytes = await _storage.ReadFile(key);
                if (bytes != null)
                {
                    _cache[key] = Encoding.UTF8.GetString(bytes);
                }
            }
        }

        public async Task Flush()
        {
            if (!_dirty)
            {
                return;
            }

            foreach (var kvp in _cache)
            {
                await _storage.WriteFile(kvp.Key, Encoding.UTF8.GetBytes(kvp.Value));
            }
            _dirty = false;
        }
    }
}
```

- [ ] **Step 4: Verify compilation + commit**

```bash
git add Assets/Playcenter/Services/Storage Assets/Playcenter/Services/Save
git commit -m "feat(services): storage + save services (EOS interface, local-persist mode)"
```

---

### Task 5: Config + Analytics + Wallet Services

**Files:**
- Create: `Assets/Playcenter/Services/Config/IConfigService.cs`
- Create: `Assets/Playcenter/Services/Config/FirebaseConfigService.cs`
- Create: `Assets/Playcenter/Services/Analytics/IAnalyticsService.cs`
- Create: `Assets/Playcenter/Services/Analytics/FirebaseAnalyticsService.cs`
- Create: `Assets/Playcenter/Services/Wallet/IWalletService.cs`
- Create: `Assets/Playcenter/Services/Wallet/CoinWalletService.cs`

**Interfaces:**
- Consumes: `ISaveService`, `ILoggingService`
- Produces:
  - `IConfigService.Initialize()` → `IEnumerator`, `.Get<T>(string key, T fallback)`
  - `IAnalyticsService.Initialize()` → `IEnumerator`, `.TrackEvent(string name, Dictionary<string,object> props)`
  - `IWalletService.GetCoins()`, `.AddCoins(int)`, `.TrySpendCoins(int)` → `bool`, `event Action<int> OnCoinsChanged`

- [ ] **Step 1: Write config service**

`Assets/Playcenter/Services/Config/IConfigService.cs`:
```csharp
using System.Collections;

namespace Playcenter.Services
{
    /// <summary>
    /// Remote config with local fallback defaults. Firebase Remote Config in production.
    /// </summary>
    public interface IConfigService
    {
        bool IsReady { get; }
        IEnumerator Initialize();
        T Get<T>(string key, T fallback);
    }
}
```

`Assets/Playcenter/Services/Config/FirebaseConfigService.cs`:
```csharp
using System.Collections;
using System.Collections.Generic;

namespace Playcenter.Services
{
    /// <summary>
    /// Firebase Remote Config provider. Until Firebase is wired (Slice 2), serves
    /// the built-in defaults table — the same defaults every Get() call passes.
    /// </summary>
    public sealed class FirebaseConfigService : IConfigService
    {
        private readonly ILoggingService _log;
        private readonly Dictionary<string, object> _overrides = new Dictionary<string, object>();

        public bool IsReady { get; private set; }

        public FirebaseConfigService(ILoggingService log)
        {
            _log = log;
        }

        public IEnumerator Initialize()
        {
            // Firebase Remote Config fetch goes here in Slice 2.
            IsReady = true;
            _log.Log("[Config] Initialized (defaults mode, Firebase pending)");
            yield break;
        }

        public T Get<T>(string key, T fallback)
        {
            if (_overrides.TryGetValue(key, out var value) && value is T typed)
            {
                return typed;
            }
            return fallback;
        }

        /// <summary>Editor/debug hook for forcing values without Firebase.</summary>
        public void SetOverride(string key, object value)
        {
            _overrides[key] = value;
        }
    }
}
```

- [ ] **Step 2: Write analytics service**

`Assets/Playcenter/Services/Analytics/IAnalyticsService.cs`:
```csharp
using System.Collections;
using System.Collections.Generic;

namespace Playcenter.Services
{
    public interface IAnalyticsService
    {
        bool IsReady { get; }
        IEnumerator Initialize();
        void TrackEvent(string eventName, Dictionary<string, object> properties = null);
    }
}
```

`Assets/Playcenter/Services/Analytics/FirebaseAnalyticsService.cs`:
```csharp
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Playcenter.Services
{
    /// <summary>
    /// Firebase Analytics provider. Until Firebase is wired (Slice 2), logs events
    /// through ILoggingService so funnels can be verified in the editor console.
    /// </summary>
    public sealed class FirebaseAnalyticsService : IAnalyticsService
    {
        private readonly ILoggingService _log;

        public bool IsReady { get; private set; }

        public FirebaseAnalyticsService(ILoggingService log)
        {
            _log = log;
        }

        public IEnumerator Initialize()
        {
            IsReady = true;
            _log.Log("[Analytics] Initialized (log mode, Firebase pending)");
            yield break;
        }

        public void TrackEvent(string eventName, Dictionary<string, object> properties = null)
        {
            var sb = new StringBuilder($"[Analytics] {eventName}");
            if (properties != null)
            {
                foreach (var kvp in properties)
                {
                    sb.Append($" {kvp.Key}={kvp.Value}");
                }
            }
            _log.Log(sb.ToString());
        }
    }
}
```

- [ ] **Step 3: Write wallet service**

`Assets/Playcenter/Services/Wallet/IWalletService.cs`:
```csharp
using System;

namespace Playcenter.Services
{
    /// <summary>
    /// Coin wallet. Coins are only earned and spent — never lost per match.
    /// </summary>
    public interface IWalletService
    {
        event Action<int> OnCoinsChanged;
        int GetCoins();
        void AddCoins(int amount);
        bool TrySpendCoins(int amount);
    }
}
```

`Assets/Playcenter/Services/Wallet/CoinWalletService.cs`:
```csharp
using System;
using System.Collections.Generic;

namespace Playcenter.Services
{
    public sealed class CoinWalletService : IWalletService
    {
        private const string CoinsKey = "wallet_coins";

        private readonly ISaveService _save;
        private readonly IAnalyticsService _analytics;
        private CoinData _data;

        public event Action<int> OnCoinsChanged;

        public CoinWalletService(ISaveService save, IAnalyticsService analytics)
        {
            _save = save;
            _analytics = analytics;
            _data = _save.Load(CoinsKey, new CoinData { Coins = 0 });
        }

        public int GetCoins() => _data.Coins;

        public void AddCoins(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _data.Coins += amount;
            _save.Save(CoinsKey, _data);
            _analytics.TrackEvent("coins_earned", new Dictionary<string, object>
            {
                { "amount", amount },
                { "total", _data.Coins }
            });
            OnCoinsChanged?.Invoke(_data.Coins);
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount <= 0 || _data.Coins < amount)
            {
                return false;
            }

            _data.Coins -= amount;
            _save.Save(CoinsKey, _data);
            _analytics.TrackEvent("coins_spent", new Dictionary<string, object>
            {
                { "amount", amount },
                { "total", _data.Coins }
            });
            OnCoinsChanged?.Invoke(_data.Coins);
            return true;
        }

        [Serializable]
        private sealed class CoinData
        {
            public int Coins;
        }
    }
}
```

- [ ] **Step 4: Verify compilation + commit**

```bash
git add Assets/Playcenter/Services/Config Assets/Playcenter/Services/Analytics Assets/Playcenter/Services/Wallet
git commit -m "feat(services): config + analytics + wallet (Firebase interfaces, defaults/log mode)"
```

---

### Task 6: Auth Service (Facebook / Google / Guest)

**Files:**
- Create: `Assets/Playcenter/Services/Auth/IAuthService.cs`
- Create: `Assets/Playcenter/Services/Auth/AuthService.cs`

**Interfaces:**
- Consumes: `ISaveService`, `ILoggingService`, `IAnalyticsService`
- Produces:
  - `IAuthService.Initialize()` → `IEnumerator`, `.SignInWithFacebook()` → `Task<AuthResult>`, `.SignInWithGoogle()` → `Task<AuthResult>`, `.SignInAsGuest()` → `Task<AuthResult>`, `.SignOut()`, `.IsSignedIn`, `.UserId`, `.DisplayName`
  - `AuthResult` struct: `bool Success`, `string UserId`, `string DisplayName`, `string Error`

- [ ] **Step 1: Write IAuthService**

`Assets/Playcenter/Services/Auth/IAuthService.cs`:
```csharp
using System.Collections;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>
    /// Authentication. Providers: Facebook, Google, Guest. NO Epic account login.
    /// </summary>
    public interface IAuthService
    {
        bool IsReady { get; }
        bool IsSignedIn { get; }
        string UserId { get; }
        string DisplayName { get; }

        IEnumerator Initialize();
        Task<AuthResult> SignInWithFacebook();
        Task<AuthResult> SignInWithGoogle();
        Task<AuthResult> SignInAsGuest();
        void SignOut();
    }

    public readonly struct AuthResult
    {
        public bool Success { get; }
        public string UserId { get; }
        public string DisplayName { get; }
        public string Error { get; }

        public AuthResult(bool success, string userId, string displayName, string error = null)
        {
            Success = success;
            UserId = userId;
            DisplayName = displayName;
            Error = error;
        }
    }
}
```

- [ ] **Step 2: Write AuthService**

`Assets/Playcenter/Services/Auth/AuthService.cs`:
```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>
    /// Auth orchestrator. Facebook/Google SDK sign-in is wired in Slice 2; Guest
    /// sign-in is fully functional now (persistent anonymous ID). Until provider
    /// SDKs land, Facebook/Google fall back to guest with a logged warning so the
    /// whole game is playable end-to-end.
    /// </summary>
    public sealed class AuthService : IAuthService
    {
        private const string GuestIdKey = "auth_guest_id";
        private const string ProviderKey = "auth_provider";

        private readonly ISaveService _save;
        private readonly ILoggingService _log;
        private readonly IAnalyticsService _analytics;

        public bool IsReady { get; private set; }
        public bool IsSignedIn { get; private set; }
        public string UserId { get; private set; }
        public string DisplayName { get; private set; }

        public AuthService(ISaveService save, ILoggingService log, IAnalyticsService analytics)
        {
            _save = save;
            _log = log;
            _analytics = analytics;
        }

        public IEnumerator Initialize()
        {
            // Restore previous session
            var savedId = _save.Load(GuestIdKey, string.Empty);
            if (!string.IsNullOrEmpty(savedId))
            {
                UserId = savedId;
                DisplayName = "Guest";
                IsSignedIn = true;
                _log.Log($"[Auth] Restored guest session: {savedId}");
            }

            IsReady = true;
            yield break;
        }

        public async Task<AuthResult> SignInWithFacebook()
        {
            // Facebook SDK sign-in wired in Slice 2. Fallback: guest.
            _log.LogWarning("[Auth] Facebook SDK not wired yet — signing in as guest");
            return await SignInAsGuest();
        }

        public async Task<AuthResult> SignInWithGoogle()
        {
            // Google SDK sign-in wired in Slice 2. Fallback: guest.
            _log.LogWarning("[Auth] Google SDK not wired yet — signing in as guest");
            return await SignInAsGuest();
        }

        public Task<AuthResult> SignInAsGuest()
        {
            var guestId = _save.Load(GuestIdKey, string.Empty);
            if (string.IsNullOrEmpty(guestId))
            {
                guestId = "guest_" + Guid.NewGuid().ToString("N").Substring(0, 12);
                _save.Save(GuestIdKey, guestId);
            }

            UserId = guestId;
            DisplayName = "Guest";
            IsSignedIn = true;
            _save.Save(ProviderKey, "guest");

            _analytics.TrackEvent("auth_sign_in", new Dictionary<string, object> { { "provider", "guest" } });
            return Task.FromResult(new AuthResult(true, guestId, DisplayName));
        }

        public void SignOut()
        {
            IsSignedIn = false;
            UserId = null;
            DisplayName = null;
            _analytics.TrackEvent("auth_sign_out");
        }
    }
}
```

- [ ] **Step 3: Verify compilation + commit**

```bash
git add Assets/Playcenter/Services/Auth
git commit -m "feat(services): auth service (Facebook/Google/Guest, guest functional now)"
```

---

### Task 7: Audio Service + Event-Driven AudioSystem

**Files:**
- Create: `Assets/Playcenter/Services/Audio/IAudioService.cs`
- Create: `Assets/Playcenter/Services/Audio/UnityAudioService.cs`
- Create: `Assets/Playcenter/Services/Audio/AudioSystem.cs`
- Create: `Assets/Playcenter/Services/Audio/SfxId.cs`

**Interfaces:**
- Consumes: `IEventBus`, `ILoggingService`
- Produces:
  - `IAudioService.Play(string sfxId)`, `.PlayMusic(string musicId)`, `.StopMusic()`, `.SetMasterVolume(float)`, `.SetMusicVolume(float)`, `.SetSfxVolume(float)`
  - `AudioSystem.Initialize(IEventBus)` — subscribes to gameplay events (wired to real events in Slice 1)

- [ ] **Step 1: Write IAudioService + SfxId**

`Assets/Playcenter/Services/Audio/IAudioService.cs`:
```csharp
namespace Playcenter.Services
{
    public interface IAudioService
    {
        void Play(string sfxId);
        void PlayMusic(string musicId);
        void StopMusic();
        void SetMasterVolume(float volume01);
        void SetMusicVolume(float volume01);
        void SetSfxVolume(float volume01);
    }
}
```

`Assets/Playcenter/Services/Audio/SfxId.cs`:
```csharp
namespace Playcenter.Services
{
    /// <summary>Stable SFX identifiers. Clips are mapped in the AudioService inspector.</summary>
    public static class SfxId
    {
        public const string KnifeChop = "sfx_knife_chop";
        public const string CookingDone = "sfx_cooking_done";
        public const string Burning = "sfx_burning";
        public const string RecipeComplete = "sfx_recipe_complete";
        public const string Pickup = "sfx_pickup";
        public const string Drop = "sfx_drop";
        public const string PlateArrange = "sfx_plate_arrange";
        public const string ButtonClick = "sfx_button_click";
        public const string CoinCollect = "sfx_coin_collect";
        public const string Victory = "sfx_victory";
        public const string Defeat = "sfx_defeat";
        public const string Countdown = "sfx_countdown";
    }
}
```

- [ ] **Step 2: Write UnityAudioService**

`Assets/Playcenter/Services/Audio/UnityAudioService.cs`:
```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Playcenter.Services
{
    /// <summary>
    /// Audio via Unity AudioMixer (Master/Music/SFX groups) + pooled AudioSources.
    /// Clips are registered by id in the inspector-facing ClipMap asset created in Slice 1.
    /// </summary>
    public sealed class UnityAudioService : IAudioService
    {
        private const string MasterParam = "MasterVolume";
        private const string MusicParam = "MusicVolume";
        private const string SfxParam = "SFXVolume";

        private readonly AudioMixer _mixer;
        private readonly AudioSource _musicSource;
        private readonly List<AudioSource> _sfxPool = new List<AudioSource>(8);
        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>(32);

        public UnityAudioService(AudioMixer mixer, Transform poolParent)
        {
            _mixer = mixer;

            var musicGo = new GameObject("MusicSource");
            musicGo.transform.SetParent(poolParent, false);
            _musicSource = musicGo.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.outputAudioMixerGroup = _mixer.FindMatchingGroups("Music")[0];

            for (int i = 0; i < 8; i++)
            {
                var go = new GameObject($"SfxSource_{i}");
                go.transform.SetParent(poolParent, false);
                var source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.outputAudioMixerGroup = _mixer.FindMatchingGroups("SFX")[0];
                _sfxPool.Add(source);
            }
        }

        public void RegisterClip(string id, AudioClip clip)
        {
            if (clip != null)
            {
                _clips[id] = clip;
            }
        }

        public void Play(string sfxId)
        {
            if (!_clips.TryGetValue(sfxId, out var clip))
            {
                return;
            }

            var source = GetFreeSource();
            source.PlayOneShot(clip);
        }

        public void PlayMusic(string musicId)
        {
            if (!_clips.TryGetValue(musicId, out var clip))
            {
                return;
            }

            if (_musicSource.clip == clip && _musicSource.isPlaying)
            {
                return;
            }

            _musicSource.clip = clip;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            _musicSource.Stop();
        }

        public void SetMasterVolume(float volume01) => SetVolume(MasterParam, volume01);
        public void SetMusicVolume(float volume01) => SetVolume(MusicParam, volume01);
        public void SetSfxVolume(float volume01) => SetVolume(SfxParam, volume01);

        private void SetVolume(string param, float volume01)
        {
            var db = volume01 <= 0.0001f ? -80f : Mathf.Log10(Mathf.Clamp01(volume01)) * 20f;
            _mixer.SetFloat(param, db);
        }

        private AudioSource GetFreeSource()
        {
            foreach (var source in _sfxPool)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            return _sfxPool[0];
        }
    }
}
```

- [ ] **Step 3: Write AudioSystem (event-driven glue)**

`Assets/Playcenter/Services/Audio/AudioSystem.cs`:
```csharp
namespace Playcenter.Services
{
    /// <summary>
    /// Subscribes to gameplay events on the bus and maps them to SFX.
    /// Gameplay knows nothing about audio. Event subscriptions are added
    /// in Slice 1 when the gameplay event types exist.
    /// </summary>
    public sealed class AudioSystem
    {
        private readonly IAudioService _audio;

        public AudioSystem(IAudioService audio)
        {
            _audio = audio;
        }

        public void Initialize(IEventBus bus)
        {
            // Slice 1 wires gameplay events here, e.g.:
            // bus.Subscribe<IngredientChoppedEvent>(e => _audio.Play(SfxId.KnifeChop));
            // bus.Subscribe<RecipeServedEvent>(e => _audio.Play(SfxId.RecipeComplete));
        }
    }
}
```

- [ ] **Step 4: Create AudioMixer asset**

In Unity Editor:
1. `Assets/Art/Audio/` → right-click → Create → Audio Mixer → name `MainMixer`
2. Add groups: `Master` → children `Music`, `SFX`
3. Expose parameters: `MasterVolume`, `MusicVolume`, `SFXVolume`

- [ ] **Step 5: Verify compilation + commit**

```bash
git add Assets/Playcenter/Services/Audio Assets/Art/Audio
git commit -m "feat(services): audio service (mixer + pool) + event-driven AudioSystem"
```

---

### Task 8: Ads / IAP / Friends Services (Interfaces + Stub-First)

**Files:**
- Create: `Assets/Playcenter/Services/Ads/IAdsService.cs`
- Create: `Assets/Playcenter/Services/Ads/AdMobService.cs`
- Create: `Assets/Playcenter/Services/IAP/IIAPService.cs`
- Create: `Assets/Playcenter/Services/IAP/UnityIAPService.cs`
- Create: `Assets/Playcenter/Services/Friends/IFriendsService.cs`
- Create: `Assets/Playcenter/Services/Friends/UnityGamingServicesFriends.cs`

**Interfaces:**
- Consumes: `ILoggingService`, `IAnalyticsService`
- Produces:
  - `IAdsService.ShowRewardedAd(string placement, Action<bool> onComplete)`, `.ShowInterstitial()`, `.IsRewardedReady`
  - `IIAPService.Initialize()` → `IEnumerator`, `.Purchase(string productId)`, `.IsProductAvailable(string productId)`, `event Action<string> OnPurchaseCompleted`
  - `IFriendsService.GetFriends()` → `Task<List<FriendInfo>>`, `.InviteFriend(string friendId)`, `.AddFriendByCode(string code)` → `Task<bool>`, `.MyFriendCode`

- [ ] **Step 1: Write ads**

`Assets/Playcenter/Services/Ads/IAdsService.cs`:
```csharp
using System;
using System.Collections;

namespace Playcenter.Services
{
    public interface IAdsService
    {
        bool IsReady { get; }
        bool IsRewardedReady { get; }
        IEnumerator Initialize();
        void ShowRewardedAd(string placement, Action<bool> onComplete);
        void ShowInterstitial();
    }
}
```

`Assets/Playcenter/Services/Ads/AdMobService.cs`:
```csharp
using System;
using System.Collections;
using System.Collections.Generic;

namespace Playcenter.Services
{
    /// <summary>
    /// AdMob provider. Until the ad SDK is wired (Slice 5), rewarded ads
    /// immediately succeed so the reward flow is testable end-to-end.
    /// </summary>
    public sealed class AdMobService : IAdsService
    {
        private readonly ILoggingService _log;
        private readonly IAnalyticsService _analytics;

        public bool IsReady { get; private set; }
        public bool IsRewardedReady => true; // stub: always ready

        public AdMobService(ILoggingService log, IAnalyticsService analytics)
        {
            _log = log;
            _analytics = analytics;
        }

        public IEnumerator Initialize()
        {
            IsReady = true;
            _log.Log("[Ads] Initialized (stub mode, AdMob pending)");
            yield break;
        }

        public void ShowRewardedAd(string placement, Action<bool> onComplete)
        {
            _log.Log($"[Ads] Rewarded ad requested: {placement} (stub — auto-success)");
            _analytics.TrackEvent("ad_rewarded_shown", new Dictionary<string, object> { { "placement", placement } });
            onComplete?.Invoke(true);
        }

        public void ShowInterstitial()
        {
            _log.Log("[Ads] Interstitial requested (stub — no-op)");
            _analytics.TrackEvent("ad_interstitial_shown");
        }
    }
}
```

- [ ] **Step 2: Write IAP**

`Assets/Playcenter/Services/IAP/IIAPService.cs`:
```csharp
using System;
using System.Collections;

namespace Playcenter.Services
{
    public interface IIAPService
    {
        bool IsReady { get; }
        event Action<string> OnPurchaseCompleted;
        IEnumerator Initialize();
        void Purchase(string productId);
        bool IsProductAvailable(string productId);
        string GetLocalizedPrice(string productId);
    }
}
```

`Assets/Playcenter/Services/IAP/UnityIAPService.cs`:
```csharp
using System;
using System.Collections;
using System.Collections.Generic;

namespace Playcenter.Services
{
    /// <summary>
    /// Unity IAP provider. Until store billing is wired (Slice 5), purchases
    /// immediately complete so the purchase flow is testable end-to-end.
    /// Product catalog lives in Slice 5.
    /// </summary>
    public sealed class UnityIAPService : IIAPService
    {
        private readonly ILoggingService _log;
        private readonly IAnalyticsService _analytics;

        public bool IsReady { get; private set; }
        public event Action<string> OnPurchaseCompleted;

        public UnityIAPService(ILoggingService log, IAnalyticsService analytics)
        {
            _log = log;
            _analytics = analytics;
        }

        public IEnumerator Initialize()
        {
            IsReady = true;
            _log.Log("[IAP] Initialized (stub mode, Unity IAP pending)");
            yield break;
        }

        public void Purchase(string productId)
        {
            _log.Log($"[IAP] Purchase requested: {productId} (stub — auto-complete)");
            _analytics.TrackEvent("iap_purchase", new Dictionary<string, object> { { "productId", productId } });
            OnPurchaseCompleted?.Invoke(productId);
        }

        public bool IsProductAvailable(string productId) => true;

        public string GetLocalizedPrice(string productId) => "$0.99";
    }
}
```

- [ ] **Step 3: Write friends**

`Assets/Playcenter/Services/Friends/IFriendsService.cs`:
```csharp
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>
    /// Friends via Unity Gaming Services (NOT EOS — EOS requires Epic accounts).
    /// </summary>
    public interface IFriendsService
    {
        bool IsReady { get; }
        string MyFriendCode { get; }
        IEnumerator Initialize();
        Task<List<FriendInfo>> GetFriends();
        Task<bool> AddFriendByCode(string code);
        void InviteFriend(string friendId);
    }

    public sealed class FriendInfo
    {
        public string FriendId;
        public string DisplayName;
        public FriendPresence Presence;
    }

    public enum FriendPresence
    {
        Offline,
        InMainMenu,
        InLobby,
        InMatch
    }
}
```

`Assets/Playcenter/Services/Friends/UnityGamingServicesFriends.cs`:
```csharp
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>
    /// Unity Gaming Services Friends provider. Until UGS Friends is wired (Slice 5),
    /// returns an empty friend list and a locally-generated friend code.
    /// </summary>
    public sealed class UnityGamingServicesFriends : IFriendsService
    {
        private readonly ISaveService _save;
        private readonly ILoggingService _log;

        public bool IsReady { get; private set; }
        public string MyFriendCode { get; private set; }

        public UnityGamingServicesFriends(ISaveService save, ILoggingService log)
        {
            _save = save;
            _log = log;
        }

        public IEnumerator Initialize()
        {
            MyFriendCode = _save.Load("friend_code", string.Empty);
            if (string.IsNullOrEmpty(MyFriendCode))
            {
                var rng = new System.Random();
                const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
                var code = new char[6];
                for (int i = 0; i < code.Length; i++)
                {
                    code[i] = chars[rng.Next(chars.Length)];
                }
                MyFriendCode = new string(code);
                _save.Save("friend_code", MyFriendCode);
            }

            IsReady = true;
            _log.Log($"[Friends] Initialized (stub mode, UGS pending). Code: {MyFriendCode}");
            yield break;
        }

        public Task<List<FriendInfo>> GetFriends()
        {
            return Task.FromResult(new List<FriendInfo>());
        }

        public Task<bool> AddFriendByCode(string code)
        {
            _log.Log($"[Friends] Add by code requested: {code} (stub — no-op)");
            return Task.FromResult(false);
        }

        public void InviteFriend(string friendId)
        {
            _log.Log($"[Friends] Invite requested: {friendId} (stub — no-op)");
        }
    }
}
```

- [ ] **Step 4: Verify compilation + commit**

```bash
git add Assets/Playcenter/Services/Ads Assets/Playcenter/Services/IAP Assets/Playcenter/Services/Friends
git commit -m "feat(services): ads + IAP + friends (interfaces, stub-first implementations)"
```

---

### Task 9: PlaycenterCompositionRoot + Boot Scene

**Files:**
- Create: `Assets/Playcenter/Core/DI/PlaycenterCompositionRoot.cs`
- Create: `Assets/Scenes/Boot.unity` (created in editor)
- Create: `Assets/Playcenter/Services/Audio/AudioClipMap.cs`

**Interfaces:**
- Consumes: every SDK service from Tasks 2-8
- Produces:
  - `PlaycenterCompositionRoot.OnPlaycenterInitialized` (static `event Action`)
  - Boot scene that initializes the full SDK on app launch

- [ ] **Step 1: Write AudioClipMap (inspector clip registry)**

`Assets/Playcenter/Services/Audio/AudioClipMap.cs`:
```csharp
using System;
using UnityEngine;

namespace Playcenter.Services
{
    /// <summary>
    /// Inspector-facing clip registry. On boot, registers all clips with IAudioService.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioClipMap", menuName = "Playcenter/Audio Clip Map")]
    public sealed class AudioClipMap : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public string Id;
            public AudioClip Clip;
        }

        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();

        public void RegisterAll(IAudioService audioService)
        {
            if (audioService is UnityAudioService unityAudio)
            {
                foreach (var entry in _entries)
                {
                    unityAudio.RegisterClip(entry.Id, entry.Clip);
                }
            }
        }
    }
}
```

- [ ] **Step 2: Write PlaycenterCompositionRoot**

`Assets/Playcenter/Core/DI/PlaycenterCompositionRoot.cs`:
```csharp
using System;
using System.Collections;
using Playcenter.Services;
using UnityEngine;
using UnityEngine.Audio;

namespace Playcenter
{
    /// <summary>
    /// Boot composition root. Constructs + initializes every SDK service,
    /// registers them in ServiceLocator, then fires OnPlaycenterInitialized.
    /// GameplayCompositionRoot listens for that event before building game services.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class PlaycenterCompositionRoot : MonoBehaviour
    {
        public static event Action OnPlaycenterInitialized;

        [Header("Audio")]
        [SerializeField] private AudioMixer _mainMixer;
        [SerializeField] private AudioClipMap _clipMap;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            // Core primitives
            var eventBus = new EventBus();
            var loggingService = new UnityLoggingService();
            var timeService = new UnityTimeService();

            // SDK services (FULL logic)
            var storageService = new EOSCloudStorageService(loggingService);
            var saveService = new EOSCloudSaveService(storageService);
            var configService = new FirebaseConfigService(loggingService);
            var analyticsService = new FirebaseAnalyticsService(loggingService);
            var authService = new AuthService(saveService, loggingService, analyticsService);
            var adsService = new AdMobService(loggingService, analyticsService);
            var iapService = new UnityIAPService(loggingService, analyticsService);
            var friendsService = new UnityGamingServicesFriends(saveService, loggingService);
            var audioService = new UnityAudioService(_mainMixer, transform);
            var walletService = new CoinWalletService(saveService, analyticsService);

            if (_clipMap != null)
            {
                _clipMap.RegisterAll(audioService);
            }

            var audioSystem = new AudioSystem(audioService);
            audioSystem.Initialize(eventBus);

            // Register
            ServiceLocator.Register<IEventBus>(eventBus);
            ServiceLocator.Register<ILoggingService>(loggingService);
            ServiceLocator.Register<ITimeService>(timeService);
            ServiceLocator.Register<IStorageService>(storageService);
            ServiceLocator.Register<ISaveService>(saveService);
            ServiceLocator.Register<IConfigService>(configService);
            ServiceLocator.Register<IAnalyticsService>(analyticsService);
            ServiceLocator.Register<IAuthService>(authService);
            ServiceLocator.Register<IAdsService>(adsService);
            ServiceLocator.Register<IIAPService>(iapService);
            ServiceLocator.Register<IFriendsService>(friendsService);
            ServiceLocator.Register<IAudioService>(audioService);
            ServiceLocator.Register<IWalletService>(walletService);

            StartCoroutine(InitializeSDK());
        }

        private IEnumerator InitializeSDK()
        {
            yield return ServiceLocator.Get<IStorageService>().Initialize();
            yield return ServiceLocator.Get<IConfigService>().Initialize();
            yield return ServiceLocator.Get<IAuthService>().Initialize();
            yield return ServiceLocator.Get<IAnalyticsService>().Initialize();
            yield return ServiceLocator.Get<IAdsService>().Initialize();
            yield return ServiceLocator.Get<IIAPService>().Initialize();
            yield return ServiceLocator.Get<IFriendsService>().Initialize();

            ServiceLocator.Get<ILoggingService>().Log("[Playcenter] SDK initialized");
            OnPlaycenterInitialized?.Invoke();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && ServiceLocator.TryGet<ISaveService>(out var save))
            {
                _ = save.Flush();
            }
        }

        private void OnApplicationQuit()
        {
            if (ServiceLocator.TryGet<ISaveService>(out var save))
            {
                _ = save.Flush();
            }
        }
    }
}
```

- [ ] **Step 3: Create Boot scene in editor**

1. Create `Assets/Scenes/Boot.unity`
2. Add empty GameObject `PlaycenterCompositionRoot` → attach `PlaycenterCompositionRoot`
3. Assign `_mainMixer` (from Task 7 Step 4) and `_clipMap` (create via Create → Playcenter → Audio Clip Map)
4. Add scene to Build Settings (index 0)

- [ ] **Step 4: Verify — run Boot scene in editor**

Expected console output:
```
[Storage] Initialized (local-persist mode, EOS transport pending)
[Config] Initialized (defaults mode, Firebase pending)
[Analytics] Initialized (log mode, Firebase pending)
[Ads] Initialized (stub mode, AdMob pending)
[IAP] Initialized (stub mode, Unity IAP pending)
[Friends] Initialized (stub mode, UGS pending). Code: XXXXXX
[Playcenter] SDK initialized
```

- [ ] **Step 5: Commit**

```bash
git add Assets/Playcenter Assets/Scenes
git commit -m "feat(core): PlaycenterCompositionRoot + boot scene (full SDK init)"
```

---

### Task 10: Game State Machine + Scene Loader + Input Service (Game-Side Foundation)

**Files:**
- Create: `Assets/Game/DI/GameplayCompositionRoot.cs`
- Create: `Assets/Game/Gameplay/Match/IGameStateMachine.cs`
- Create: `Assets/Game/Gameplay/Match/GameStateMachine.cs`
- Create: `Assets/Game/Gameplay/Match/IGameState.cs`
- Create: `Assets/Game/Gameplay/Match/States/MainMenuState.cs` (in `Assets/Game/Gameplay/Match/States/`)
- Create: `Assets/Game/Gameplay/Player/ISceneLoader.cs` (place in `Assets/Game/Gameplay/` root instead: `Assets/Game/Gameplay/ISceneLoader.cs`)
- Create: `Assets/Game/Gameplay/AddressablesSceneLoader.cs`
- Create: `Assets/Game/Gameplay/Player/IInputService.cs`
- Create: `Assets/Game/Gameplay/Player/DualStickInputService.cs`

**Interfaces:**
- Consumes: `PlaycenterCompositionRoot.OnPlaycenterInitialized`, all SDK services
- Produces:
  - `IGameStateMachine.ChangeState(IGameState)`, `.Update(float)`; `IGameState.Enter()/Exit()/Update(float)`
  - `ISceneLoader.LoadScene(string key)` → `Task`, `.UnloadScene(string key)` → `Task`
  - `IInputService.MoveAxis` (Vector2), `.AimAxis` (Vector2), `.InteractPressed` (bool, frame), `.ChopPressed` (bool, frame)
  - `GameplayCompositionRoot` — the game-side entry point used by every later slice

- [ ] **Step 1: Write state machine**

`Assets/Game/Gameplay/Match/IGameState.cs`:
```csharp
namespace RecipeRage
{
    public interface IGameState
    {
        void Enter();
        void Exit();
        void Update(float deltaTime);
    }
}
```

`Assets/Game/Gameplay/Match/IGameStateMachine.cs`:
```csharp
namespace RecipeRage
{
    public interface IGameStateMachine
    {
        IGameState CurrentState { get; }
        void ChangeState(IGameState newState);
        void Update(float deltaTime);
    }
}
```

`Assets/Game/Gameplay/Match/GameStateMachine.cs`:
```csharp
namespace RecipeRage
{
    public sealed class GameStateMachine : IGameStateMachine
    {
        public IGameState CurrentState { get; private set; }

        public void ChangeState(IGameState newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
        }

        public void Update(float deltaTime)
        {
            CurrentState?.Update(deltaTime);
        }
    }
}
```

`Assets/Game/Gameplay/Match/States/MainMenuState.cs`:
```csharp
using Playcenter;

namespace RecipeRage
{
    /// <summary>
    /// Placeholder main menu state — real UI lands in Slice 5. Logs entry so the
    /// boot → gameplay handoff is verifiable in the console today.
    /// </summary>
    public sealed class MainMenuState : IGameState
    {
        public void Enter()
        {
            ServiceLocator.Get<ILoggingService>().Log("[Game] MainMenuState entered");
        }

        public void Exit() { }

        public void Update(float deltaTime) { }
    }
}
```

- [ ] **Step 2: Write scene loader**

`Assets/Game/Gameplay/ISceneLoader.cs`:
```csharp
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace RecipeRage
{
    public interface ISceneLoader
    {
        Task LoadSceneAdditive(string sceneName);
        Task UnloadScene(string sceneName);
    }
}
```

`Assets/Game/Gameplay/AddressablesSceneLoader.cs`:
```csharp
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace RecipeRage
{
    /// <summary>
    /// Scene loading. Uses SceneManager today; Addressables scene keys swap in
    /// when map assets are built (Polish phase) without changing call sites.
    /// </summary>
    public sealed class AddressablesSceneLoader : ISceneLoader
    {
        public async Task LoadSceneAdditive(string sceneName)
        {
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!op.isDone)
            {
                await Task.Yield();
            }
        }

        public async Task UnloadScene(string sceneName)
        {
            var op = SceneManager.UnloadSceneAsync(sceneName);
            while (op != null && !op.isDone)
            {
                await Task.Yield();
            }
        }
    }
}
```

- [ ] **Step 3: Write input service**

`Assets/Game/Gameplay/Player/IInputService.cs`:
```csharp
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Dual-stick input. Left stick = move, right stick = aim/interact direction.
    /// Button states are per-frame (true only on the frame pressed).
    /// </summary>
    public interface IInputService
    {
        Vector2 MoveAxis { get; }
        Vector2 AimAxis { get; }
        bool InteractPressed { get; }
        bool ChopPressed { get; }
        void Tick();
    }
}
```

`Assets/Game/Gameplay/Player/DualStickInputService.cs`:
```csharp
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Dual-stick touch input. Renders two virtual sticks (UI lands in Slice 5);
    /// until then reads editor keyboard (WASD + mouse buttons) so gameplay is
    /// testable immediately. Touch stick logic slots into Tick() without
    /// changing the interface.
    /// </summary>
    public sealed class DualStickInputService : IInputService
    {
        public Vector2 MoveAxis { get; private set; }
        public Vector2 AimAxis { get; private set; }
        public bool InteractPressed { get; private set; }
        public bool ChopPressed { get; private set; }

        public void Tick()
        {
            // Editor/dev fallback; touch sticks replace this body in Slice 5.
            MoveAxis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (MoveAxis.sqrMagnitude > 1f)
            {
                MoveAxis.Normalize();
            }

            AimAxis = Vector2.zero;
            InteractPressed = Input.GetMouseButtonDown(0);
            ChopPressed = Input.GetMouseButtonDown(1);
        }
    }
}
```

- [ ] **Step 4: Write GameplayCompositionRoot**

`Assets/Game/DI/GameplayCompositionRoot.cs`:
```csharp
using Playcenter;
using Playcenter.Services;
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Game-side composition root. Waits for the Playcenter SDK, then constructs
    /// game services (gameplay logic ONLY — core logic lives in the SDK).
    /// </summary>
    [DefaultExecutionOrder(-900)]
    public sealed class GameplayCompositionRoot : MonoBehaviour
    {
        private IGameStateMachine _stateMachine;
        private IInputService _input;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            PlaycenterCompositionRoot.OnPlaycenterInitialized += OnPlaycenterReady;
        }

        private void OnPlaycenterReady()
        {
            _input = new DualStickInputService();
            var sceneLoader = new AddressablesSceneLoader();
            _stateMachine = new GameStateMachine();

            ServiceLocator.Register(_input);
            ServiceLocator.Register<ISceneLoader>(sceneLoader);
            ServiceLocator.Register(_stateMachine);

            _stateMachine.ChangeState(new MainMenuState());
            ServiceLocator.Get<ILoggingService>().Log("[Game] Gameplay initialized");
        }

        private void Update()
        {
            if (_stateMachine == null)
            {
                return;
            }

            _input.Tick();
            _stateMachine.Update(ServiceLocator.Get<ITimeService>().DeltaTime);
        }

        private void OnDestroy()
        {
            PlaycenterCompositionRoot.OnPlaycenterInitialized -= OnPlaycenterReady;
        }
    }
}
```

- [ ] **Step 5: Wire into Boot scene**

In the editor, add a second GameObject `GameplayCompositionRoot` to `Boot.unity` with the `GameplayCompositionRoot` component.

- [ ] **Step 6: Verify — run Boot scene**

Expected console output (after SDK init lines):
```
[Game] MainMenuState entered
[Game] Gameplay initialized
```

- [ ] **Step 7: Commit**

```bash
git add Assets/Game Assets/Scenes
git commit -m "feat(game): GameplayCompositionRoot + state machine + scene loader + input service"
```

---

## Self-Review Notes

- **Spec coverage:** Composition roots ✅, EventBus ✅, Audio ✅, Save (EOS Cloud interface) ✅, Config ✅, Logging ✅, State machine ✅, Scene loading ✅, Input ✅, Auth (FB/Google/Guest) ✅, Ads/IAP/Friends stubs ✅, Wallet ✅. UI Toolkit screen framework and Net/EOS transport are intentionally deferred — they're only needed from Slice 2/5 onward and their interfaces would be guesswork before then.
- **Type consistency:** `IAuthService.Initialize()` returns `IEnumerator` consistently across services; `OnPlaycenterInitialized` is the single handoff event; `ISaveService.Flush()` returns `Task` everywhere referenced.
- **Deferred items (explicit, not placeholders):** Firebase/EOS/AdMob/UGS SDK wiring (Slice 2/5, requires credentials), UI Toolkit screens (Slice 5), touch stick UI (Slice 5), gameplay event subscriptions in `AudioSystem.Initialize` (Slice 1, needs gameplay event types).

## Next Plans in Series

1. `2026-07-25-reciperage-slice1-core-gameplay.md` — Player, stations, recipes, chop/cook/plate/serve, tutorial map, off-screen indicators
2. `2026-07-25-reciperage-slice2-multiplayer.md` — NGO + EOS, network sync, lobby/matchmaking, team compositions, countdown
3. `2026-07-25-reciperage-slice3-bots.md` — BotController, evaluators, adaptive difficulty
4. `2026-07-25-reciperage-slice4-progression.md` — Chef unlock/upgrade, trophies, EOS Cloud persistence
5. `2026-07-25-reciperage-slice5-monetization-polish.md` — IAP/ads/cosmetics, UI screens, premium animations, 3D showcase, themed maps
