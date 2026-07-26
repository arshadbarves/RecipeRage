# Playcenter Shared Services Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move the common Ads/Analytics/IAP/RemoteConfig service logic out of the game and into the Playcenter SDK behind ports-and-adapters, deleting the game-side stub/"stop code" services.

**Architecture:** Pure C# common services in `Playcenter.Services` (no VContainer, no UnityEngine, no vendor SDK). Vendor adapters in a new `Playcenter.Services.Unity` pack (the only assembly that references AppLovin MAX / Firebase / Unity IAP, all `#if`-guarded). RecipeRage wires SDK services in `RootLifetimeScope` and injects only its own seams (gem grantor, Firebase RC provider, event bridge).

**Tech Stack:** Unity 6000.3, C# (netstandard2.1), NUnit EditMode tests, AppLovin MAX (`APPLOVIN_MAX`), Firebase Analytics/RemoteConfig (`FIREBASE_ANALYTICS`/`FIREBASE_REMOTE_CONFIG`), Unity IAP (`UNITY_IAP`).

**Spec:** `docs/superpowers/specs/2026-07-22-playcenter-shared-services-design.md`

## Global Constraints

- `Assets/Playcenter/Services/**` — **no** `using VContainer`, **no** `UnityEngine`, **no** vendor SDK (`Firebase.*`, AppLovin, `UnityEngine.Purchasing`), **no** `Playcenter.Shell`. Pure C# (`noEngineReferences: true`).
- `Assets/Playcenter/Services.Unity/**` — the **only** Playcenter assembly allowed to reference vendor SDKs; every vendor touch is wrapped in its `#if` define so the assembly compiles with the vendor SDK absent.
- Game Presentation/Application never reference vendor SDKs directly (unchanged firewall law).
- Do **not** change any existing port's public signatures (`IAdsService`, `IAnalyticsService`, `IIAPService`, `IRemoteConfigService`, `IConfigService`, `IConfigProvider`, `IConfigModel`).
- Game event-name constants (`AnalyticsEvents`) and `IAPCatalog` stay game-side; the SDK never references them.
- Verify each task with `dotnet build <proj>.csproj -nologo` (EditMode does not run via `dotnet test` in CLI; build is the green signal). Unity will regenerate `.csproj`/`.meta`.
- Commit trailer on every commit: `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>`

---

### Task 1: AnalyticsService + IAnalyticsSink in SDK

**Files:**
- Create: `Assets/Playcenter/Services/Runtime/Analytics/IAnalyticsSink.cs`
- Create: `Assets/Playcenter/Services/Runtime/Analytics/AnalyticsService.cs`
- Create: `Assets/Playcenter/Services/Runtime/Analytics/DebugAnalyticsSink.cs`
- Test: `Assets/Scripts/Tests/EditMode/StudioSdk/AnalyticsServiceTests.cs`

**Interfaces:**
- Consumes: existing `Playcenter.Services.IAnalyticsService` (`void LogEvent(string, Dictionary<string,object> = null)`, `void SetUserProperty(string,string)`).
- Produces: `IAnalyticsSink { void LogEvent(string eventName, Dictionary<string,object> parameters); void SetUserProperty(string name, string value); }`; `AnalyticsService(IAnalyticsSink sink)` implementing `IAnalyticsService`; `DebugAnalyticsSink` (default sink).

- [ ] **Step 1: Write the failing test**

```csharp
using System.Collections.Generic;
using NUnit.Framework;
using Playcenter.Services;

namespace RecipeRage.Tests.EditMode.StudioSdk
{
    public sealed class AnalyticsServiceTests
    {
        private sealed class SpySink : IAnalyticsSink
        {
            public List<(string name, Dictionary<string, object> ps)> Events = new();
            public List<(string n, string v)> Props = new();
            public void LogEvent(string e, Dictionary<string, object> p) => Events.Add((e, p));
            public void SetUserProperty(string n, string v) => Props.Add((n, v));
        }

        [Test]
        public void LogEvent_ForwardsToSink_WithSameParameters()
        {
            var sink = new SpySink();
            var svc = new AnalyticsService(sink);
            var ps = new Dictionary<string, object> { { "k", 1 } };
            svc.LogEvent("test_event", ps);
            Assert.AreEqual(1, sink.Events.Count);
            Assert.AreEqual("test_event", sink.Events[0].name);
            Assert.AreSame(ps, sink.Events[0].ps);
        }

        [Test]
        public void LogEvent_NullParameters_ForwardsEmptyDictionaryNotNull()
        {
            var sink = new SpySink();
            var svc = new AnalyticsService(sink);
            svc.LogEvent("e", null);
            Assert.IsNotNull(sink.Events[0].ps);
            Assert.AreEqual(0, sink.Events[0].ps.Count);
        }

        [Test]
        public void SetUserProperty_ForwardsToSink()
        {
            var sink = new SpySink();
            var svc = new AnalyticsService(sink);
            svc.SetUserProperty("level", "3");
            Assert.AreEqual(("level", "3"), sink.Props[0]);
        }

        [Test]
        public void LogEvent_NullSink_DoesNotThrow()
        {
            var svc = new AnalyticsService(null);
            Assert.DoesNotThrow(() => svc.LogEvent("e"));
            Assert.DoesNotThrow(() => svc.SetUserProperty("a", "b"));
        }
    }
}
```

- [ ] **Step 2: Run build to verify it fails**

Run: `dotnet build RecipeRage.Tests.EditMode.csproj -nologo`
Expected: FAIL — `AnalyticsService`/`IAnalyticsSink`/`DebugAnalyticsSink` do not exist (CS0246).

- [ ] **Step 3: Implement the port + service + default sink**

`IAnalyticsSink.cs`:
```csharp
using System.Collections.Generic;

namespace Playcenter.Services
{
    /// <summary>Backend sink for analytics. Games/adapters implement (e.g. Firebase).</summary>
    public interface IAnalyticsSink
    {
        void LogEvent(string eventName, Dictionary<string, object> parameters);
        void SetUserProperty(string name, string value);
    }
}
```

`AnalyticsService.cs`:
```csharp
using System.Collections.Generic;

namespace Playcenter.Services
{
    /// <summary>Shared analytics: sanitizes params and forwards to an <see cref="IAnalyticsSink"/>.</summary>
    public sealed class AnalyticsService : IAnalyticsService
    {
        private static readonly Dictionary<string, object> Empty = new();
        private readonly IAnalyticsSink _sink;

        public AnalyticsService(IAnalyticsSink sink)
        {
            _sink = sink;
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }
            _sink?.LogEvent(eventName, parameters ?? Empty);
        }

        public void SetUserProperty(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }
            _sink?.SetUserProperty(name, value);
        }
    }
}
```

`DebugAnalyticsSink.cs`:
```csharp
using System.Collections.Generic;

namespace Playcenter.Services
{
    /// <summary>Default sink that formats events to the debug log. Engine-free; games may swap.</summary>
    public sealed class DebugAnalyticsSink : IAnalyticsSink
    {
        public void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
            // Engine-free: no UnityEngine.Debug. Games wire a real sink; this is a safe no-op default.
            System.Diagnostics.Debug.WriteLine(Format(eventName, parameters));
        }

        public void SetUserProperty(string name, string value)
        {
            System.Diagnostics.Debug.WriteLine($"[Analytics] prop {name}={value}");
        }

        private static string Format(string eventName, Dictionary<string, object> ps)
        {
            if (ps == null || ps.Count == 0)
            {
                return $"[Analytics] {eventName}";
            }
            var sb = new System.Text.StringBuilder($"[Analytics] {eventName} {{ ");
            foreach (KeyValuePair<string, object> kvp in ps)
            {
                sb.Append($"{kvp.Key}={kvp.Value}, ");
            }
            sb.Append('}');
            return sb.ToString();
        }
    }
}
```

- [ ] **Step 4: Run build to verify it passes**

Run: `dotnet build RecipeRage.Tests.EditMode.csproj -nologo`
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add Assets/Playcenter/Services/Runtime/Analytics/ Assets/Scripts/Tests/EditMode/StudioSdk/AnalyticsServiceTests.cs
git commit -m "feat(sdk): add shared AnalyticsService with IAnalyticsSink port

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 2: AdsService + IAdNetwork in SDK

**Files:**
- Create: `Assets/Playcenter/Services/Runtime/Ads/IAdNetwork.cs`
- Create: `Assets/Playcenter/Services/Runtime/Ads/AdsService.cs`
- Create: `Assets/Playcenter/Services/Runtime/Ads/NullAdNetwork.cs`
- Test: `Assets/Scripts/Tests/EditMode/StudioSdk/AdsServiceTests.cs`

**Interfaces:**
- Consumes: existing `IAdsService`, `AdRewardResult` (in `Playcenter.Services`), `IConfigService` (`T Get<T>(string key, T fallback)`).
- Produces: `IAdNetwork { bool IsInterstitialReady { get; } bool IsRewardedReady { get; } Task<bool> ShowInterstitialAsync(); Task<AdRewardResult> ShowRewardedAsync(string placement); }`; `AdsService(IAdNetwork network, IConfigService cfg)` implementing `IAdsService`; `NullAdNetwork`.

- [ ] **Step 1: Write the failing test**

```csharp
using System.Threading.Tasks;
using NUnit.Framework;
using Playcenter.Services;

namespace RecipeRage.Tests.EditMode.StudioSdk
{
    public sealed class AdsServiceTests
    {
        private sealed class FakeNetwork : IAdNetwork
        {
            public bool InterstitialShown;
            public bool IsInterstitialReady => true;
            public bool IsRewardedReady => true;
            public Task<bool> ShowInterstitialAsync() { InterstitialShown = true; return Task.FromResult(true); }
            public Task<AdRewardResult> ShowRewardedAsync(string p) => Task.FromResult(new AdRewardResult(true, p));
        }

        private sealed class StubConfig : IConfigService
        {
            public System.Func<string, int> IntValue = key => 0;
            public System.Func<string, bool> BoolValue = key => true;
            public T Get<T>(string key, T fallback)
            {
                if (typeof(T) == typeof(int)) return (T)(object)IntValue(key);
                if (typeof(T) == typeof(bool)) return (T)(object)BoolValue(key);
                return fallback;
            }
            public Task FetchAsync() => Task.FromResult(true);
        }

        [Test]
        public void ShouldShowInterstitial_WhenDisabled_ReturnsFalse()
        {
            var svc = new AdsService(new FakeNetwork(), new StubConfig());
            svc.DisableInterstitials();
            Assert.IsFalse(svc.ShouldShowInterstitial(3));
        }

        [Test]
        public void ShouldShowInterstitial_WhenConfigDisabled_ReturnsFalse()
        {
            var cfg = new StubConfig { BoolValue = key => false };
            var svc = new AdsService(new FakeNetwork(), cfg);
            Assert.IsFalse(svc.ShouldShowInterstitial(3));
        }

        [Test]
        public void ShouldShowInterstitial_NotOnFrequencyBoundary_ReturnsFalse()
        {
            var cfg = new StubConfig { IntValue = key => key == "ad_interstitial_frequency" ? 3 : 0 };
            var svc = new AdsService(new FakeNetwork(), cfg);
            Assert.IsFalse(svc.ShouldShowInterstitial(4));
        }

        [Test]
        public void ShouldShowInterstitial_OnBoundaryAndGapElapsed_ReturnsTrue()
        {
            var cfg = new StubConfig { IntValue = key => key == "ad_interstitial_frequency" ? 3 : 0 };
            var svc = new AdsService(new FakeNetwork(), cfg);
            Assert.IsTrue(svc.ShouldShowInterstitial(3));
        }

        [Test]
        public async Task ShowInterstitial_RecordsShownTime_SoImmediateNextIsGated()
        {
            var cfg = new StubConfig { IntValue = key => key == "ad_interstitial_frequency" ? 1 : key == "ad_interstitial_min_gap_sec" ? 180 : 0 };
            var svc = new AdsService(new FakeNetwork(), cfg);
            Assert.IsTrue(svc.ShouldShowInterstitial(1));
            await svc.ShowInterstitialAsync();
            Assert.IsFalse(svc.ShouldShowInterstitial(2), "min-gap should block an immediate second interstitial");
        }
    }
}
```

- [ ] **Step 2: Run build to verify it fails**

Run: `dotnet build RecipeRage.Tests.EditMode.csproj -nologo`
Expected: FAIL — `IAdNetwork`/`AdsService`/`NullAdNetwork` do not exist (CS0246).

- [ ] **Step 3: Implement the port + service + null adapter**

`IAdNetwork.cs`:
```csharp
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>Ad-network adapter (e.g. AppLovin MAX). SDK keeps the gating logic.</summary>
    public interface IAdNetwork
    {
        bool IsInterstitialReady { get; }
        bool IsRewardedReady { get; }
        Task<bool> ShowInterstitialAsync();
        Task<AdRewardResult> ShowRewardedAsync(string placement);
    }
}
```

`AdsService.cs`:
```csharp
using System;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>Shared ads: owns interstitial frequency/min-gap/disable gating; renders via <see cref="IAdNetwork"/>.</summary>
    public sealed class AdsService : IAdsService
    {
        private readonly IAdNetwork _network;
        private readonly IConfigService _cfg;
        private bool _interstitialsDisabled;
        private DateTime _lastInterstitialUtc = DateTime.MinValue;

        public AdsService(IAdNetwork network, IConfigService cfg)
        {
            _network = network;
            _cfg = cfg;
        }

        public bool IsInterstitialReady => _network != null && _network.IsInterstitialReady;
        public bool IsRewardedReady => _network != null && _network.IsRewardedReady;

        public async Task<bool> ShowInterstitialAsync()
        {
            if (_network == null)
            {
                return false;
            }
            bool shown = await _network.ShowInterstitialAsync();
            if (shown)
            {
                _lastInterstitialUtc = DateTime.UtcNow;
            }
            return shown;
        }

        public Task<AdRewardResult> ShowRewardedAsync(string placement)
        {
            if (_network == null)
            {
                return Task.FromResult(new AdRewardResult(false, placement));
            }
            return _network.ShowRewardedAsync(placement);
        }

        public bool ShouldShowInterstitial(int matchCount)
        {
            if (_interstitialsDisabled)
            {
                return false;
            }
            if (_cfg != null && !_cfg.Get("ad_interstitial_enabled", true))
            {
                return false;
            }
            int frequency = _cfg != null ? _cfg.Get("ad_interstitial_frequency", 3) : 3;
            if (frequency <= 0)
            {
                frequency = 1;
            }
            if (matchCount % frequency != 0)
            {
                return false;
            }
            int minGapSec = _cfg != null ? _cfg.Get("ad_interstitial_min_gap_sec", 180) : 180;
            if ((DateTime.UtcNow - _lastInterstitialUtc).TotalSeconds < minGapSec)
            {
                return false;
            }
            return true;
        }

        public void DisableInterstitials()
        {
            _interstitialsDisabled = true;
        }
    }
}
```

`NullAdNetwork.cs`:
```csharp
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>Default ad network when no vendor adapter is wired: nothing is ever ready/shown.</summary>
    public sealed class NullAdNetwork : IAdNetwork
    {
        public bool IsInterstitialReady => false;
        public bool IsRewardedReady => false;
        public Task<bool> ShowInterstitialAsync() => Task.FromResult(false);
        public Task<AdRewardResult> ShowRewardedAsync(string placement) => Task.FromResult(new AdRewardResult(false, placement));
    }
}
```

- [ ] **Step 4: Run build to verify it passes**

Run: `dotnet build RecipeRage.Tests.EditMode.csproj -nologo`
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add Assets/Playcenter/Services/Runtime/Ads/ Assets/Scripts/Tests/EditMode/StudioSdk/AdsServiceTests.cs
git commit -m "feat(sdk): add shared AdsService with IAdNetwork port

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 3: IAPService + IStoreBackend + IIapRewardGrantor in SDK

**Files:**
- Create: `Assets/Playcenter/Services/Runtime/IAP/IStoreBackend.cs`
- Create: `Assets/Playcenter/Services/Runtime/IAP/IIapRewardGrantor.cs`
- Create: `Assets/Playcenter/Services/Runtime/IAP/IAPService.cs`
- Test: `Assets/Scripts/Tests/EditMode/StudioSdk/IAPServiceTests.cs`

**Interfaces:**
- Consumes: existing `IIAPService`, `IAPResult` (`Success`, `ProductId`, `Error`), `IAnalyticsService`.
- Produces:
  - `IStoreBackend { bool IsInitialized { get; } Task InitializeAsync(); Task<StorePurchaseResult> PurchaseAsync(string productId); }`
  - `StorePurchaseResult { bool Success { get; } string ProductId { get; } string Error { get; } }` (ctor `(bool, string, string error = null)`)
  - `IIapRewardGrantor { Task GrantAsync(string productId); }`
  - `IAPService(IStoreBackend store, IIapRewardGrantor grantor, IAnalyticsService analytics = null)` implementing `IIAPService`. Emits generic events `"iap_purchase_success"` / `"iap_purchase_fail"` with params `product_id`, `success`, `reason`.

- [ ] **Step 1: Write the failing test**

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Playcenter.Services;

namespace RecipeRage.Tests.EditMode.StudioSdk
{
    public sealed class IAPServiceTests
    {
        private sealed class FakeStore : IStoreBackend
        {
            public bool InitResult = true;
            public StorePurchaseResult Next = new StorePurchaseResult(true, "x");
            public bool IsInitialized { get; private set; }
            public Task InitializeAsync() { IsInitialized = InitResult; return Task.CompletedTask; }
            public Task<StorePurchaseResult> PurchaseAsync(string id) => Task.FromResult(Next);
        }

        private sealed class SpyGrantor : IIapRewardGrantor
        {
            public List<string> Granted = new();
            public Task GrantAsync(string productId) { Granted.Add(productId); return Task.CompletedTask; }
        }

        private sealed class SpyAnalytics : IAnalyticsService
        {
            public List<string> Events = new();
            public void LogEvent(string e, Dictionary<string, object> p = null) => Events.Add(e);
            public void SetUserProperty(string n, string v) { }
        }

        [Test]
        public async Task Purchase_Success_GrantsReward_AndLogsSuccess()
        {
            var store = new FakeStore { Next = new StorePurchaseResult(true, "gem_pack_s") };
            var grantor = new SpyGrantor();
            var analytics = new SpyAnalytics();
            var svc = new IAPService(store, grantor, analytics);
            var result = await svc.PurchaseAsync("gem_pack_s");
            Assert.IsTrue(result.Success);
            Assert.AreEqual(new[] { "gem_pack_s" }, grantor.Granted.ToArray());
            Assert.Contains("iap_purchase_success", analytics.Events);
        }

        [Test]
        public async Task Purchase_StoreFails_DoesNotGrant_AndLogsFail()
        {
            var store = new FakeStore { Next = new StorePurchaseResult(false, "x", "declined") };
            var grantor = new SpyGrantor();
            var analytics = new SpyAnalytics();
            var svc = new IAPService(store, grantor, analytics);
            var result = await svc.PurchaseAsync("x");
            Assert.IsFalse(result.Success);
            Assert.AreEqual("declined", result.Error);
            Assert.IsEmpty(grantor.Granted);
            Assert.Contains("iap_purchase_fail", analytics.Events);
        }

        [Test]
        public async Task Purchase_InitializesStoreOnce()
        {
            var store = new FakeStore();
            var svc = new IAPService(store, new SpyGrantor());
            await svc.PurchaseAsync("a");
            Assert.IsTrue(store.IsInitialized);
            Assert.IsTrue(svc.IsInitialized);
        }
    }
}
```

- [ ] **Step 2: Run build to verify it fails**

Run: `dotnet build RecipeRage.Tests.EditMode.csproj -nologo`
Expected: FAIL — `IStoreBackend`/`StorePurchaseResult`/`IIapRewardGrantor`/`IAPService` do not exist (CS0246).

- [ ] **Step 3: Implement ports + service**

`IStoreBackend.cs`:
```csharp
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>Store adapter (e.g. Unity IAP). SDK owns the purchase flow.</summary>
    public interface IStoreBackend
    {
        bool IsInitialized { get; }
        Task InitializeAsync();
        Task<StorePurchaseResult> PurchaseAsync(string productId);
    }

    public sealed class StorePurchaseResult
    {
        public bool Success { get; }
        public string ProductId { get; }
        public string Error { get; }

        public StorePurchaseResult(bool success, string productId, string error = null)
        {
            Success = success;
            ProductId = productId;
            Error = error;
        }
    }
}
```

`IIapRewardGrantor.cs`:
```csharp
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>Maps a purchased productId to the game's currency grant. Game-supplied.</summary>
    public interface IIapRewardGrantor
    {
        Task GrantAsync(string productId);
    }
}
```

`IAPService.cs`:
```csharp
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>Shared IAP flow: init store → purchase → grant reward → log outcome. Never throws into gameplay.</summary>
    public sealed class IAPService : IIAPService
    {
        private readonly IStoreBackend _store;
        private readonly IIapRewardGrantor _grantor;
        private readonly IAnalyticsService _analytics;
        private bool _initAttempted;

        public bool IsInitialized => _store != null && _store.IsInitialized;

        public IAPService(IStoreBackend store, IIapRewardGrantor grantor, IAnalyticsService analytics = null)
        {
            _store = store;
            _grantor = grantor;
            _analytics = analytics;
        }

        public async Task<IAPResult> PurchaseAsync(string productId)
        {
            if (_store == null)
            {
                return new IAPResult(false, productId, "no store backend");
            }

            if (!_initAttempted)
            {
                _initAttempted = true;
                await _store.InitializeAsync();
            }

            if (!_store.IsInitialized)
            {
                Log("iap_purchase_fail", productId, false, "store not initialized");
                return new IAPResult(false, productId, "store not initialized");
            }

            StorePurchaseResult storeResult = await _store.PurchaseAsync(productId);
            if (!storeResult.Success)
            {
                Log("iap_purchase_fail", productId, false, storeResult.Error);
                return new IAPResult(false, productId, storeResult.Error);
            }

            if (_grantor != null)
            {
                await _grantor.GrantAsync(productId);
            }

            Log("iap_purchase_success", productId, true, null);
            return new IAPResult(true, productId);
        }

        private void Log(string eventName, string productId, bool success, string reason)
        {
            _analytics?.LogEvent(eventName, new Dictionary<string, object>
            {
                { "product_id", productId ?? string.Empty },
                { "success", success },
                { "reason", reason ?? string.Empty }
            });
        }
    }
}
```

- [ ] **Step 4: Run build to verify it passes**

Run: `dotnet build RecipeRage.Tests.EditMode.csproj -nologo`
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add Assets/Playcenter/Services/Runtime/IAP/ Assets/Scripts/Tests/EditMode/StudioSdk/IAPServiceTests.cs
git commit -m "feat(sdk): add shared IAPService with IStoreBackend and IIapRewardGrantor ports

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 4: RemoteConfigService + FallbackConfigProvider in SDK

**Files:**
- Create: `Assets/Playcenter/Services/Runtime/RemoteConfig/RemoteConfigService.cs`
- Create: `Assets/Playcenter/Services/Runtime/RemoteConfig/FallbackConfigProvider.cs`
- Test: `Assets/Scripts/Tests/EditMode/StudioSdk/RemoteConfigServiceTests.cs`

**Interfaces:**
- Consumes: `IRemoteConfigService`, `IConfigService`, `IConfigProvider`, `IConfigModel` (`IsValid()`, `Validate()`), `ConfigHealthStatus` (`Healthy`/`Degraded`/`Failed`).
- Produces: `RemoteConfigService(IConfigProvider provider = null)` implementing `IRemoteConfigService` + `IConfigService`, with `event System.Action<IConfigModel> OnConfigUpdated` and `event System.Action<ConfigHealthStatus> OnHealthChanged`; `FallbackConfigProvider : IConfigProvider` (defaults, `ProviderName => "Fallback"`).

- [ ] **Step 1: Write the failing test**

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Playcenter.Services;

namespace RecipeRage.Tests.EditMode.StudioSdk
{
    public sealed class RemoteConfigServiceTests
    {
        private sealed class FakeModel : IConfigModel
        {
            public int Value = 1;
            public bool IsValid() => true;
        }

        private sealed class FakeProvider : IConfigProvider
        {
            public bool Available = true;
            public bool InitResult = true;
            public Dictionary<string, IConfigModel> All = new();
            public string ProviderName => "Fake";
            public bool IsAvailable() => Available;
            public Task<bool> Initialize() => Task.FromResult(InitResult);
            public Task<T> FetchConfig<T>(string key) where T : IConfigModel =>
                Task.FromResult(All.TryGetValue(key, out var m) ? (T)m : default);
            public Task<Dictionary<string, IConfigModel>> FetchAllConfigs() => Task.FromResult(All);
        }

        [Test]
        public async Task Initialize_ProviderHealthy_StatusHealthy()
        {
            var svc = new RemoteConfigService(new FakeProvider());
            await svc.Initialize();
            Assert.AreEqual(ConfigHealthStatus.Healthy, svc.HealthStatus);
        }

        [Test]
        public async Task Initialize_ProviderUnavailable_StatusDegraded()
        {
            var svc = new RemoteConfigService(new FakeProvider { InitResult = false, Available = false });
            await svc.Initialize();
            Assert.AreEqual(ConfigHealthStatus.Degraded, svc.HealthStatus);
        }

        [Test]
        public async Task Refresh_CachesModel_AndRaisesOnConfigUpdated()
        {
            var provider = new FakeProvider();
            provider.All["FakeModel"] = new FakeModel { Value = 42 };
            var svc = new RemoteConfigService(provider);
            await svc.Initialize();
            IConfigModel raised = null;
            svc.OnConfigUpdated += m => raised = m;
            await svc.RefreshConfig();
            Assert.IsTrue(svc.TryGetConfig<FakeModel>(out var cfg));
            Assert.AreEqual(42, cfg.Value);
            Assert.IsNotNull(raised);
        }

        [Test]
        public void HealthChange_RaisesOnHealthChanged()
        {
            var svc = new RemoteConfigService(new FakeProvider { InitResult = false, Available = false });
            ConfigHealthStatus? raised = null;
            svc.OnHealthChanged += s => raised = s;
            svc.Initialize().Wait();
            Assert.AreEqual(ConfigHealthStatus.Degraded, raised);
        }

        [Test]
        public void Get_UnknownRawKey_ReturnsFallback()
        {
            var svc = new RemoteConfigService(new FallbackConfigProvider());
            svc.Initialize().Wait();
            Assert.AreEqual(7, svc.Get("missing", 7));
        }
    }
}
```

- [ ] **Step 2: Run build to verify it fails**

Run: `dotnet build RecipeRage.Tests.EditMode.csproj -nologo`
Expected: FAIL — `RemoteConfigService`/`FallbackConfigProvider` do not exist (CS0246).

- [ ] **Step 3: Implement service + fallback provider**

`RemoteConfigService.cs`:
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>
    /// Shared remote config: typed-model cache + fallback + health, engine-free.
    /// Change notification via plain C# events; games bridge to their own event bus.
    /// Implements both <see cref="IRemoteConfigService"/> and <see cref="IConfigService"/>.
    /// </summary>
    public sealed class RemoteConfigService : IRemoteConfigService, IConfigService
    {
        private readonly IConfigProvider _provider;
        private readonly Dictionary<Type, IConfigModel> _cache = new();
        private readonly Dictionary<string, object> _rawCache = new();
        private ConfigHealthStatus _healthStatus = ConfigHealthStatus.Failed;
        private bool _isInitialized;

        public event Action<IConfigModel> OnConfigUpdated;
        public event Action<ConfigHealthStatus> OnHealthChanged;

        public ConfigHealthStatus HealthStatus => _healthStatus;
        public DateTime LastUpdateTime { get; private set; } = DateTime.MinValue;

        public RemoteConfigService(IConfigProvider provider = null)
        {
            _provider = provider ?? new FallbackConfigProvider();
        }

        public async Task<bool> Initialize()
        {
            if (_isInitialized)
            {
                return true;
            }

            bool ok = false;
            try
            {
                ok = await _provider.Initialize() && _provider.IsAvailable();
            }
            catch (Exception)
            {
                ok = false;
            }

            _isInitialized = true;
            SetHealth(ok ? ConfigHealthStatus.Healthy : ConfigHealthStatus.Degraded);
            return true; // never block boot on config
        }

        public T GetConfig<T>() where T : class, IConfigModel
        {
            return _cache.TryGetValue(typeof(T), out IConfigModel cached) ? cached as T : default;
        }

        public bool TryGetConfig<T>(out T config) where T : class, IConfigModel
        {
            config = GetConfig<T>();
            return config != null;
        }

        public async Task<bool> RefreshConfig()
        {
            if (!_isInitialized)
            {
                return false;
            }

            try
            {
                Dictionary<string, IConfigModel> configs = await _provider.FetchAllConfigs();
                if (configs != null && configs.Count > 0)
                {
                    foreach (KeyValuePair<string, IConfigModel> kvp in configs)
                    {
                        if (kvp.Value != null && kvp.Value.Validate())
                        {
                            _cache[kvp.Value.GetType()] = kvp.Value;
                            OnConfigUpdated?.Invoke(kvp.Value);
                        }
                    }
                    LastUpdateTime = DateTime.UtcNow;
                    SetHealth(ConfigHealthStatus.Healthy);
                    return true;
                }
            }
            catch (Exception)
            {
                // fall through to degraded
            }

            SetHealth(ConfigHealthStatus.Degraded);
            return false;
        }

        public async Task<bool> RefreshConfig<T>() where T : class, IConfigModel
        {
            if (!_isInitialized)
            {
                return false;
            }

            try
            {
                T config = await _provider.FetchConfig<T>(typeof(T).Name);
                if (config != null && config.Validate())
                {
                    _cache[typeof(T)] = config;
                    OnConfigUpdated?.Invoke(config);
                    LastUpdateTime = DateTime.UtcNow;
                    return true;
                }
            }
            catch (Exception)
            {
                SetHealth(ConfigHealthStatus.Degraded);
            }
            return false;
        }

        public T Get<T>(string key, T fallback)
        {
            if (_rawCache.TryGetValue(key, out object cached))
            {
                try { return (T)Convert.ChangeType(cached, typeof(T)); }
                catch { /* fall through */ }
            }
            return fallback;
        }

        public Task FetchAsync() => RefreshConfig();

        private void SetHealth(ConfigHealthStatus status)
        {
            if (_healthStatus != status)
            {
                _healthStatus = status;
                OnHealthChanged?.Invoke(status);
            }
        }
    }
}
```

`FallbackConfigProvider.cs`:
```csharp
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>Default provider when no cloud backend is wired: reports available, returns defaults.</summary>
    public sealed class FallbackConfigProvider : IConfigProvider
    {
        public string ProviderName => "Fallback";
        public bool IsAvailable() => true;
        public Task<bool> Initialize() => Task.FromResult(true);
        public Task<T> FetchConfig<T>(string key) where T : IConfigModel => Task.FromResult(default(T));
        public Task<Dictionary<string, IConfigModel>> FetchAllConfigs() =>
            Task.FromResult(new Dictionary<string, IConfigModel>());
    }
}
```

- [ ] **Step 4: Run build to verify it passes**

Run: `dotnet build RecipeRage.Tests.EditMode.csproj -nologo`
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add Assets/Playcenter/Services/Runtime/RemoteConfig/ Assets/Scripts/Tests/EditMode/StudioSdk/RemoteConfigServiceTests.cs
git commit -m "feat(sdk): add shared RemoteConfigService with FallbackConfigProvider

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 5: Playcenter.Services.Unity pack + vendor adapters

**Files:**
- Create: `Assets/Playcenter/Services.Unity/Runtime/Playcenter.Services.Unity.asmdef`
- Create: `Assets/Playcenter/Services.Unity/Runtime/Analytics/FirebaseAnalyticsSink.cs`
- Create: `Assets/Playcenter/Services.Unity/Runtime/Ads/MaxAdNetwork.cs`
- Create: `Assets/Playcenter/Services.Unity/Runtime/IAP/UnityIapStoreBackend.cs`
- Create: `Assets/Playcenter/Services.Unity/Runtime/IAP/EditorFakeStoreBackend.cs`

**Interfaces:**
- Consumes: `IAnalyticsSink`, `IAdNetwork`, `IStoreBackend`, `StorePurchaseResult`, `AdRewardResult` (all from Tasks 1–3, namespace `Playcenter.Services`).
- Produces: `Playcenter.Services.Unity.FirebaseAnalyticsSink : IAnalyticsSink`; `Playcenter.Services.Unity.MaxAdNetwork : IAdNetwork` (`MaxAdNetwork()`); `Playcenter.Services.Unity.UnityIapStoreBackend : IStoreBackend` (`UnityIapStoreBackend()`); `Playcenter.Services.Unity.EditorFakeStoreBackend : IStoreBackend` (`EditorFakeStoreBackend()`).

- [ ] **Step 1: Create the asmdef**

`Assets/Playcenter/Services.Unity/Runtime/Playcenter.Services.Unity.asmdef`:
```json
{
    "name": "Playcenter.Services.Unity",
    "rootNamespace": "Playcenter.Services.Unity",
    "references": [
        "Playcenter.Services"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 2: FirebaseAnalyticsSink** (`#if FIREBASE_ANALYTICS`; safe no-op otherwise)

```csharp
using System.Collections.Generic;
using Playcenter.Services;
using UnityEngine;

namespace Playcenter.Services.Unity
{
    /// <summary>Firebase Analytics sink. Compiles to a debug-log sink when Firebase is absent.</summary>
    public sealed class FirebaseAnalyticsSink : IAnalyticsSink
    {
        public void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
#if FIREBASE_ANALYTICS
            var firebaseParams = new Firebase.Analytics.Parameter[parameters?.Count ?? 0];
            if (parameters != null)
            {
                int i = 0;
                foreach (KeyValuePair<string, object> kvp in parameters)
                {
                    firebaseParams[i++] = kvp.Value switch
                    {
                        int v => new Firebase.Analytics.Parameter(kvp.Key, v),
                        long v => new Firebase.Analytics.Parameter(kvp.Key, v),
                        float v => new Firebase.Analytics.Parameter(kvp.Key, v),
                        double v => new Firebase.Analytics.Parameter(kvp.Key, v),
                        _ => new Firebase.Analytics.Parameter(kvp.Key, kvp.Value?.ToString() ?? string.Empty)
                    };
                }
            }
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, firebaseParams);
#else
            Debug.Log(Format(eventName, parameters));
#endif
        }

        public void SetUserProperty(string name, string value)
        {
#if FIREBASE_ANALYTICS
            Firebase.Analytics.FirebaseAnalytics.SetUserProperty(name, value);
#else
            Debug.Log($"[Analytics] prop {name}={value}");
#endif
        }

#if !FIREBASE_ANALYTICS
        private static string Format(string eventName, Dictionary<string, object> ps)
        {
            if (ps == null || ps.Count == 0)
            {
                return $"[Analytics] {eventName}";
            }
            var sb = new System.Text.StringBuilder($"[Analytics] {eventName} {{ ");
            foreach (KeyValuePair<string, object> kvp in ps)
            {
                sb.Append($"{kvp.Key}={kvp.Value}, ");
            }
            sb.Append('}');
            return sb.ToString();
        }
#endif
    }
}
```

- [ ] **Step 3: MaxAdNetwork** (`#if APPLOVIN_MAX`; ready=false/log otherwise)

```csharp
using System.Threading.Tasks;
using Playcenter.Services;
using UnityEngine;

namespace Playcenter.Services.Unity
{
    /// <summary>AppLovin MAX ad network. No MAX SDK → nothing is ready and shows log only.</summary>
    public sealed class MaxAdNetwork : IAdNetwork
    {
        public bool IsInterstitialReady
        {
            get
            {
#if APPLOVIN_MAX
                return MaxSdk.IsInterstitialReady(AdUnitIds.Interstitial);
#else
                return false;
#endif
            }
        }

        public bool IsRewardedReady
        {
            get
            {
#if APPLOVIN_MAX
                return MaxSdk.IsRewardedAdReady(AdUnitIds.Rewarded);
#else
                return false;
#endif
            }
        }

        public Task<bool> ShowInterstitialAsync()
        {
#if APPLOVIN_MAX
            // TODO(wire): subscribe MaxSdkCallbacks.Interstitial.OnAdHiddenEvent to complete the task.
            MaxSdk.ShowInterstitial(AdUnitIds.Interstitial);
            return Task.FromResult(true);
#else
            Debug.Log("[MaxAdNetwork] ShowInterstitial — AppLovin MAX not integrated");
            return Task.FromResult(false);
#endif
        }

        public Task<AdRewardResult> ShowRewardedAsync(string placement)
        {
#if APPLOVIN_MAX
            // TODO(wire): subscribe MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent.
            MaxSdk.ShowRewardedAd(AdUnitIds.Rewarded);
            return Task.FromResult(new AdRewardResult(true, placement));
#else
            Debug.Log($"[MaxAdNetwork] ShowRewarded placement={placement} — AppLovin MAX not integrated");
            return Task.FromResult(new AdRewardResult(false, placement));
#endif
        }
    }
}
```

- [ ] **Step 4: UnityIapStoreBackend** (`#if UNITY_IAP`) **+ EditorFakeStoreBackend** (default)

`UnityIapStoreBackend.cs`:
```csharp
using System.Threading.Tasks;
using Playcenter.Services;
using UnityEngine;

namespace Playcenter.Services.Unity
{
    /// <summary>Unity IAP store backend. Without UNITY_IAP, reports uninitialized (game falls back to editor fake).</summary>
    public sealed class UnityIapStoreBackend : IStoreBackend
    {
        public bool IsInitialized { get; private set; }

        public Task InitializeAsync()
        {
#if UNITY_IAP
            // TODO(wire): UnityEngine.Purchasing.StandardPurchasingModule + ConfigurationBuilder.Initialize.
            IsInitialized = true;
#else
            Debug.Log("[UnityIapStoreBackend] Unity IAP not integrated");
            IsInitialized = false;
#endif
            return Task.CompletedTask;
        }

        public Task<StorePurchaseResult> PurchaseAsync(string productId)
        {
#if UNITY_IAP
            // TODO(wire): IStoreController.InitiatePurchase; complete on ProcessPurchase.
            return Task.FromResult(new StorePurchaseResult(true, productId));
#else
            return Task.FromResult(new StorePurchaseResult(false, productId, "Unity IAP not integrated"));
#endif
        }
    }
}
```

`EditorFakeStoreBackend.cs`:
```csharp
using System.Threading.Tasks;
using Playcenter.Services;
using UnityEngine;

namespace Playcenter.Services.Unity
{
    /// <summary>Editor/dev store: always succeeds so purchases are testable without a store SDK.</summary>
    public sealed class EditorFakeStoreBackend : IStoreBackend
    {
        public bool IsInitialized { get; private set; }

        public Task InitializeAsync()
        {
            IsInitialized = true;
            return Task.CompletedTask;
        }

        public Task<StorePurchaseResult> PurchaseAsync(string productId)
        {
            Debug.Log($"[EditorFakeStoreBackend] Simulating purchase: {productId}");
            return Task.FromResult(new StorePurchaseResult(true, productId));
        }
    }
}
```

- [ ] **Step 5: Build the pack**

Run: `dotnet build Playcenter.Services.Unity.csproj -nologo`
Expected: `Build succeeded.` (If the `.csproj` does not exist yet, run the Unity Editor once to regenerate project files, or build `RecipeRage.Tests.EditMode.csproj` after Task 7 wiring instead.)

- [ ] **Step 6: Commit**

```bash
git add Assets/Playcenter/Services.Unity/
git commit -m "feat(sdk): add Playcenter.Services.Unity vendor adapters (MAX/Firebase/UnityIAP)

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 6: RecipeRage seams — gem grantor + RC event bridge

**Files:**
- Create: `Assets/_KitchenClash/Infrastructure/Services/RecipeRageIapRewardGrantor.cs`
- Create: `Assets/_KitchenClash/Infrastructure/RemoteConfig/RemoteConfigEventBridge.cs`

**Interfaces:**
- Consumes: `IIapRewardGrantor` (Task 3); `IAPCatalog.GetById(string) → IAPItem` (`Gems`), `IEconomyService.AddGems(int)`; `RemoteConfigService.OnConfigUpdated`/`OnHealthChanged` (Task 4); game `IEventBus` (`Publish<T>(T)`), `ConfigUpdatedEvent(IConfigModel)`, `ConfigHealthStatusChangedEvent { Status }`.
- Produces: `RecipeRageIapRewardGrantor(IEconomyService economy) : IIapRewardGrantor`; `RemoteConfigEventBridge(RemoteConfigService service, IEventBus eventBus)` with `void Attach()` / `void Detach()`.

- [ ] **Step 1: RecipeRageIapRewardGrantor**

```csharp
using System.Threading.Tasks;
using KitchenClash.Application.Services;
using KitchenClash.Application.Models;
using Playcenter.Services;
using Playcenter.Shell;

namespace KitchenClash.Infrastructure.Services
{
    /// <summary>Maps a purchased productId to RecipeRage gems via IAPCatalog → IEconomyService.</summary>
    public sealed class RecipeRageIapRewardGrantor : IIapRewardGrantor
    {
        private readonly IEconomyService _economy;

        public RecipeRageIapRewardGrantor(IEconomyService economy)
        {
            _economy = economy;
        }

        public Task GrantAsync(string productId)
        {
            IAPItem item = IAPCatalog.GetById(productId);
            if (item == null)
            {
                GameLogger.LogWarning($"[RecipeRageIapRewardGrantor] Unknown productId: {productId}");
                return Task.CompletedTask;
            }

            if (item.Gems > 0 && _economy != null)
            {
                _economy.AddGems(item.Gems);
                GameLogger.Log($"[RecipeRageIapRewardGrantor] Granted {item.Gems} gems for {productId}");
            }
            return Task.CompletedTask;
        }
    }
}
```

- [ ] **Step 2: RemoteConfigEventBridge**

```csharp
using KitchenClash.Domain;
using KitchenClash.Domain.Events;
using Playcenter.Services;

namespace KitchenClash.Infrastructure.RemoteConfig
{
    /// <summary>Bridges the SDK RemoteConfigService C# events onto the game IEventBus.</summary>
    public sealed class RemoteConfigEventBridge
    {
        private readonly RemoteConfigService _service;
        private readonly IEventBus _eventBus;

        public RemoteConfigEventBridge(RemoteConfigService service, IEventBus eventBus)
        {
            _service = service;
            _eventBus = eventBus;
        }

        public void Attach()
        {
            if (_service == null || _eventBus == null)
            {
                return;
            }
            _service.OnConfigUpdated += HandleConfigUpdated;
            _service.OnHealthChanged += HandleHealthChanged;
        }

        public void Detach()
        {
            if (_service == null)
            {
                return;
            }
            _service.OnConfigUpdated -= HandleConfigUpdated;
            _service.OnHealthChanged -= HandleHealthChanged;
        }

        private void HandleConfigUpdated(IConfigModel config)
        {
            _eventBus?.Publish(new ConfigUpdatedEvent(config));
        }

        private void HandleHealthChanged(ConfigHealthStatus status)
        {
            _eventBus?.Publish(new ConfigHealthStatusChangedEvent { Status = status });
        }
    }
}
```

- [ ] **Step 3: Build game Infrastructure**

Run: `dotnet build KitchenClash.Infrastructure.csproj -nologo`
Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add Assets/_KitchenClash/Infrastructure/Services/RecipeRageIapRewardGrantor.cs Assets/_KitchenClash/Infrastructure/RemoteConfig/RemoteConfigEventBridge.cs
git commit -m "feat(sdk): add RecipeRage IAP gem grantor and remote-config event bridge

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 7: Rewire RootLifetimeScope + delete stub services

**Files:**
- Modify: `Assets/_KitchenClash/Composition/RootLifetimeScope.cs` (~lines 168–185 — the RC/analytics/ads/IAP registrations)
- Delete: `Assets/_KitchenClash/Infrastructure/Ads/StubAdsService.cs` (+ `.meta`)
- Delete: `Assets/_KitchenClash/Infrastructure/IAP/StubIAPService.cs` (+ `.meta`)
- Delete: `Assets/_KitchenClash/Infrastructure/Analytics/FirebaseAnalyticsService.cs` (+ `.meta`)
- Delete: `Assets/_KitchenClash/Infrastructure/Firebase/FirebaseAnalyticsService.cs` (+ `.meta`)
- Delete: `Assets/_KitchenClash/Infrastructure/Services/CompositeRemoteConfigService.cs` (+ `.meta`)
- Delete: `Assets/_KitchenClash/Infrastructure/Services/FallbackRemoteConfigService.cs` (+ `.meta`)

**Interfaces:**
- Consumes: everything from Tasks 1–6.
- Produces: a container where `IAnalyticsService`, `IAdsService`, `IIAPService`, `IRemoteConfigService`, `IConfigService` resolve to the new SDK services.

- [ ] **Step 1: Replace the registrations**

Replace the `#if FIREBASE_REMOTE_CONFIG … #endif` RC block and the four stub registrations with:

```csharp
#if FIREBASE_REMOTE_CONFIG
        builder.Register<KitchenClash.Infrastructure.Firebase.FirebaseConfigProvider>(Lifetime.Singleton).As<IConfigProvider>();
#else
        builder.Register<FallbackConfigProvider>(Lifetime.Singleton).As<IConfigProvider>();
#endif
        builder.Register<RemoteConfigService>(Lifetime.Singleton).AsSelf().As<IConfigService>().As<IRemoteConfigService>();
        builder.Register(c =>
        {
            var bridge = new KitchenClash.Infrastructure.RemoteConfig.RemoteConfigEventBridge(
                c.Resolve<RemoteConfigService>(), c.Resolve<IEventBus>());
            bridge.Attach();
            return bridge;
        }, Lifetime.Singleton).AsSelf();

        builder.Register<MaintenanceService>(Lifetime.Singleton).As<IMaintenanceService>();

#if FIREBASE_ANALYTICS
        builder.Register<Playcenter.Services.Unity.FirebaseAnalyticsSink>(Lifetime.Singleton).As<IAnalyticsSink>();
#else
        builder.Register<DebugAnalyticsSink>(Lifetime.Singleton).As<IAnalyticsSink>();
#endif
        builder.Register<AnalyticsService>(Lifetime.Singleton).As<IAnalyticsService>();

#if APPLOVIN_MAX
        builder.Register<Playcenter.Services.Unity.MaxAdNetwork>(Lifetime.Singleton).As<IAdNetwork>();
#else
        builder.Register<NullAdNetwork>(Lifetime.Singleton).As<IAdNetwork>();
#endif
        builder.Register<AdsService>(Lifetime.Singleton).As<IAdsService>();

#if UNITY_IAP
        builder.Register<Playcenter.Services.Unity.UnityIapStoreBackend>(Lifetime.Singleton).As<IStoreBackend>();
#else
        builder.Register<Playcenter.Services.Unity.EditorFakeStoreBackend>(Lifetime.Singleton).As<IStoreBackend>();
#endif
        builder.Register<KitchenClash.Infrastructure.Services.RecipeRageIapRewardGrantor>(Lifetime.Singleton).As<IIapRewardGrantor>();
        builder.Register<IAPService>(Lifetime.Singleton).As<IIAPService>();
```

Add the needed `using Playcenter.Services;` if not already present at the top of the file. Ensure `KitchenClash.Composition.asmdef` references `Playcenter.Services.Unity` (add it if missing) so the vendor adapter types resolve.

- [ ] **Step 2: Delete the stub/duplicate services**

```bash
git rm Assets/_KitchenClash/Infrastructure/Ads/StubAdsService.cs \
       Assets/_KitchenClash/Infrastructure/IAP/StubIAPService.cs \
       Assets/_KitchenClash/Infrastructure/Analytics/FirebaseAnalyticsService.cs \
       Assets/_KitchenClash/Infrastructure/Firebase/FirebaseAnalyticsService.cs \
       Assets/_KitchenClash/Infrastructure/Services/CompositeRemoteConfigService.cs \
       Assets/_KitchenClash/Infrastructure/Services/FallbackRemoteConfigService.cs
```
(Delete the matching `.meta` files too if `git rm` does not take them.)

- [ ] **Step 3: Delete gates**

Run:
```bash
rg -n "StubAdsService|StubIAPService|FirebaseAnalyticsService|CompositeRemoteConfigService|FallbackRemoteConfigService" Assets --glob '*.cs'
rg -n "using VContainer" Assets/Playcenter --glob '*.cs'
```
Expected: first → 0 code hits (comments OK); second → 0 hits.

- [ ] **Step 4: Build the game assemblies**

Run:
```bash
dotnet build KitchenClash.Composition.csproj -nologo
dotnet build KitchenClash.Infrastructure.csproj -nologo
dotnet build RecipeRage.Tests.EditMode.csproj -nologo
```
Expected: all `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add Assets/_KitchenClash/Composition/RootLifetimeScope.cs Assets/_KitchenClash/Composition/KitchenClash.Composition.asmdef
git commit -m "refactor(sdk): rewire RootLifetimeScope to SDK services, delete stub services

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 8: Final gates + wiki/skill update

**Files:**
- Modify: `wiki/Technical.md` (add Shared Services section)
- Modify: `wiki/LLM-Rules.md` (REQUIRED/FORBIDDEN for shared services)
- Modify: `wiki/log.md` (2026-07-22 entry)
- Modify: `.github/skills/playcenter-sdk/SKILL.md` and `.claude/skills/playcenter-sdk/SKILL.md` (add Shared Services to FORBIDDEN/glossary — keep both files identical)

- [ ] **Step 1: Run all delete/consistency gates**

```bash
rg -n "StubAdsService|StubIAPService|FirebaseAnalyticsService|CompositeRemoteConfigService|FallbackRemoteConfigService" Assets --glob '*.cs'
rg -n "using VContainer" Assets/Playcenter --glob '*.cs'
rg -n "using Epic\.|Epic\.OnlineServices|UnityEngine\.Purchasing|Firebase\." Assets/_KitchenClash/Presentation Assets/_KitchenClash/Application --glob '*.cs'
```
Expected: 0 code hits in all three (Firebase/Purchasing only under `Playcenter/Services.Unity` or `Infrastructure/Firebase`, never Presentation/Application).

- [ ] **Step 2: Build everything green**

```bash
dotnet build Playcenter.Services.csproj -nologo
dotnet build Playcenter.Services.Unity.csproj -nologo
dotnet build KitchenClash.Composition.csproj -nologo
dotnet build RecipeRage.Tests.EditMode.csproj -nologo
```
Expected: all `Build succeeded.`

- [ ] **Step 3: Update wiki + skill**

Add to `wiki/Technical.md` a "Playcenter Shared Services" section: common Ads/Analytics/IAP/RemoteConfig live in `Playcenter.Services`; vendor adapters in `Playcenter.Services.Unity`; seams = `IAdNetwork`/`IAnalyticsSink`/`IStoreBackend`/`IIapRewardGrantor`/`IConfigProvider`; games wire in Composition only. Add matching REQUIRED/FORBIDDEN bullets to `wiki/LLM-Rules.md` (e.g. FORBIDDEN: game-side ads/analytics/IAP/RC service implementations; vendor SDK refs outside `Playcenter.Services.Unity`). Append a `2026-07-22` entry to `wiki/log.md`. Mirror the FORBIDDEN/glossary additions into both SKILL.md files identically.

- [ ] **Step 4: Commit**

```bash
git add wiki/ .github/skills/playcenter-sdk/SKILL.md .claude/skills/playcenter-sdk/SKILL.md
git commit -m "docs(sdk): wiki + skill for shared services

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```
