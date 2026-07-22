# Playcenter Shared Services — Design

**Date:** 2026-07-22  
**Status:** Approved design (not implemented)  
**Branch:** `architecture-cleanup`  
**Scope:** Move game-side Ads / Analytics / IAP / RemoteConfig implementations into the Playcenter SDK as shared, per-title-pluggable services. Delete the stub/"stop code" placeholders.

**Related:**
- `docs/superpowers/specs/2026-07-20-playcenter-studio-sdk-design.md` (implemented Studio SDK — boot, shell, module host, vendor firewall)
- `wiki/Technical.md` § Playcenter Studio SDK, `wiki/LLM-Rules.md` § Playcenter Studio SDK

---

## 1. Problem

The SDK already defines the shared service **ports** (`IAdsService`, `IAnalyticsService`, `IIAPService`, `IRemoteConfigService`) under `Assets/Playcenter/Services/Runtime/**`, but every game still carries its own **implementations** under `Assets/_KitchenClash/**`:

| Service | Game-side impl today | Problem |
|---------|----------------------|---------|
| Ads | `Infrastructure/Ads/StubAdsService.cs` | Gating logic (frequency, min-gap, disable) is common but lives in the game; ad render is a `Debug.Log` stub ("stop code"). |
| Analytics | `Infrastructure/Analytics/FirebaseAnalyticsService.cs` (+ duplicate under `Infrastructure/Firebase/`) | Dispatch/sanitizing is common but lives in the game; duplicated file. |
| IAP | `Infrastructure/IAP/StubIAPService.cs` | Purchase→deliver→log flow is common but lives in the game; `#if UNITY_EDITOR` simulate / `#else` fail stub ("stop code"). |
| RemoteConfig | `Infrastructure/Services/CompositeRemoteConfigService.cs`, `FallbackRemoteConfigService.cs` | Cache/fallback/health is common but lives in the game. |

Ads, analytics, IAP, and remote config have **the same common logic for every game**. Keeping it on the game side duplicates it per title and leaves placeholder stubs in production code.

**Goal:** the common logic lives once in the Playcenter SDK; each game plugs in only its own **seams** (ad network, analytics backend, store, currency grant, config provider) via small adapters. The stubs and duplicated wrappers are deleted.

---

## 2. Decisions (locked with stakeholder)

| Seam | Choice |
|------|--------|
| Structure | **Ports-and-adapters in the SDK.** Real common services in `Playcenter.Services`; games inject small adapters. AAA-standard, testable, no vendor lock-in at the SDK layer. |
| Ad network | **AppLovin MAX** behind a new `IAdNetwork` port. |
| Analytics backend | **Firebase Analytics** behind a new `IAnalyticsSink` port (current behaviour preserved). |
| IAP store | **Unity IAP** (`com.unity.purchasing`) behind a new `IStoreBackend` port. |
| RemoteConfig provider | Existing **`IConfigProvider`** port (already in `Playcenter.Services`); RecipeRage keeps its Firebase provider. |
| Config-change notification | **Plain C# events** on the SDK service (`event Action<IConfigModel> OnConfigUpdated`, `event Action<ConfigHealthStatus> OnHealthChanged`). RecipeRage subscribes and re-publishes to its own `IEventBus`. SDK stays engine-free and `Playcenter.Shell`-free. |

**Cutover policy:** AAA complete replacement, consistent with the Studio SDK cutover. No dual-path service resolution, no obsolete shims, no feature flags keeping the old game-side services alive.

---

## 3. Architecture

```
Playcenter.Services  (pure C#; NO VContainer, NO UnityEngine, NO vendor SDK, NO Playcenter.Shell)
  Ads/           IAdsService (port, unchanged) + AdsService (common) + IAdNetwork (new port) + NullAdNetwork
  Analytics/     IAnalyticsService (port, unchanged) + AnalyticsService (common) + IAnalyticsSink (new port) + DebugAnalyticsSink
  IAP/           IIAPService / IAPResult (port, unchanged) + IAPService (common) + IStoreBackend (new port) + IIapRewardGrantor (new port)
  RemoteConfig/  IRemoteConfigService + IConfigProvider (ports, unchanged) + RemoteConfigService (common) + FallbackConfigProvider
  Config/        IConfigService, IConfigModel (unchanged)

Playcenter.Services.Unity  (NEW Unity pack — the ONLY assembly that references vendor SDKs)
  Playcenter.Services.Unity.asmdef   (references Playcenter.Services, UnityEngine)
  Ads/MaxAdNetwork.cs                (#if APPLOVIN_MAX — AppLovin MAX)
  Analytics/FirebaseAnalyticsSink.cs (#if FIREBASE_ANALYTICS — Firebase)
  IAP/UnityIapStoreBackend.cs        (#if UNITY_IAP — com.unity.purchasing)
  IAP/EditorFakeStoreBackend.cs      (editor simulate; default when UNITY_IAP undefined)

KitchenClash (game — Composition/seams only; no common logic, no stubs)
  Composition/RootLifetimeScope.cs   (rewire to SDK services + inject RecipeRage adapters)
  Application/Config/AnalyticsEvents.cs   (UNCHANGED — event-name constants stay game-side)
  Application/Services/IAPCatalog.cs      (UNCHANGED — game catalog)
  Infrastructure/Services/RecipeRageIapRewardGrantor.cs  (NEW: IAPCatalog → IEconomyService.AddGems)
  Infrastructure/RemoteConfig/RemoteConfigEventBridge.cs (NEW: SDK C# events → game IEventBus)
  Infrastructure/Firebase/FirebaseConfigProvider.cs      (UNCHANGED — Firebase RC provider)
```

### Vendor/firewall laws (carried from Studio SDK)
- `Assets/Playcenter/Services/**` — no VContainer, no UnityEngine, no vendor SDK, no `Playcenter.Shell`.
- `Assets/Playcenter/Services.Unity/**` — the only Playcenter assembly allowed to reference AppLovin/Firebase/Unity-IAP. Guarded by `#if` so the SDK compiles when a vendor SDK is absent.
- Game Presentation/Application never reference vendor SDKs directly (unchanged law).

---

## 4. Per-service seam split

### 4.1 Ads
**Common (`AdsService`)** owns: `ShouldShowInterstitial(matchCount)` frequency/min-gap/disable gating, `_lastInterstitialUtc`, config reads via `IConfigService` (`ad_interstitial_enabled`, `ad_interstitial_frequency`, `ad_interstitial_min_gap_sec`), `DisableInterstitials()`. `IsInterstitialReady`/`IsRewardedReady`/`ShowInterstitialAsync`/`ShowRewardedAsync` delegate to the injected `IAdNetwork`, and `ShowInterstitialAsync` records `_lastInterstitialUtc` on a shown ad.

**New port `IAdNetwork`:**
```csharp
bool IsInterstitialReady { get; }
bool IsRewardedReady { get; }
Task<bool> ShowInterstitialAsync();
Task<AdRewardResult> ShowRewardedAsync(string placement);
```
`MaxAdNetwork` (Unity pack, `#if APPLOVIN_MAX`) implements it with AppLovin MAX. `NullAdNetwork` (SDK, log-only) is the default when no network adapter is injected. The `Debug.Log`-instead-of-showing stub is deleted.

### 4.2 Analytics
**Common (`AnalyticsService`)** owns: `LogEvent`/`SetUserProperty` surface (unchanged signatures), null/empty-param sanitizing, forward to the injected sink. Takes plain strings/dictionaries so any title's event constants work.

**New port `IAnalyticsSink`:**
```csharp
void LogEvent(string eventName, Dictionary<string, object> parameters);
void SetUserProperty(string name, string value);
```
`FirebaseAnalyticsSink` (Unity pack, `#if FIREBASE_ANALYTICS`) adapts Firebase — the exact param-mapping logic from today's game file moves here. `DebugAnalyticsSink` (SDK) is the editor/default. The duplicated game `FirebaseAnalyticsService` files are deleted.

### 4.3 IAP
**Common (`IAPService`)** owns the PurchaseAsync flow: store init check → `IStoreBackend.PurchaseAsync(productId)` → on success `IIapRewardGrantor.GrantAsync(productId)` → analytics success/fail events → return `IAPResult`. Store initialization is lazy/idempotent.

**New ports:**
```csharp
public interface IStoreBackend
{
    bool IsInitialized { get; }
    Task InitializeAsync();
    Task<StorePurchaseResult> PurchaseAsync(string productId); // Success + ProductId + Error
}
public interface IIapRewardGrantor
{
    Task GrantAsync(string productId); // game maps productId → currency grant
}
```
`UnityIapStoreBackend` (Unity pack, `#if UNITY_IAP`) wraps `com.unity.purchasing`. `EditorFakeStoreBackend` (Unity pack) succeeds in-editor so editor purchases still work for testing — replacing the `#if UNITY_EDITOR simulate / #else fail` stub. RecipeRage supplies `RecipeRageIapRewardGrantor` (maps `IAPCatalog.GetById(productId).Gems` → `IEconomyService.AddGems`).

**Analytics from IAP:** `IAPService` depends on the existing `IAnalyticsService` port and emits **generic, title-agnostic** event names — `"iap_purchase_success"` and `"iap_purchase_fail"` with params `product_id`, `success`, `reason`. The SDK never references the game's `AnalyticsEvents` constants class (that stays game-side for game-specific events). This keeps `Playcenter.Services` free of any KitchenClash dependency while preserving purchase telemetry for every title.

### 4.4 RemoteConfig
**Common (`RemoteConfigService`)** owns: typed-config cache, fallback-on-failure, `HealthStatus`, `LastUpdateTime`, `Initialize`/`RefreshConfig`/`RefreshConfig<T>`, and the `IConfigService.Get<T>(key, fallback)` raw-key surface (so the game’s `IConfigService` consumers keep working). Implements both `IRemoteConfigService` and `IConfigService`.

**Seam:** `IConfigProvider` (unchanged port). `FallbackConfigProvider` (SDK) returns defaults when no cloud provider is injected — replacing `FallbackRemoteConfigService`. RecipeRage keeps `FirebaseConfigProvider`.

**Change notification:** `event Action<IConfigModel> OnConfigUpdated` and `event Action<ConfigHealthStatus> OnHealthChanged`. RecipeRage's `RemoteConfigEventBridge` subscribes and re-publishes `ConfigUpdatedEvent` / `ConfigHealthStatusChangedEvent` on the game `IEventBus`. The game's `Composite`/`Fallback` wrappers are deleted.

---

## 5. Wiring (RootLifetimeScope)

Replace the four game-side registrations with SDK services + RecipeRage adapters. Vendor adapters are resolved conditionally on the same `#if` symbols used today (`FIREBASE_REMOTE_CONFIG`, `FIREBASE_ANALYTICS`, plus new `APPLOVIN_MAX`, `UNITY_IAP`), falling back to the SDK's Null/Debug/Fallback/EditorFake defaults so the game builds with or without each vendor SDK:

```csharp
// RemoteConfig
builder.Register<FirebaseConfigProvider>(...).As<IConfigProvider>();           // #if FIREBASE_REMOTE_CONFIG, else FallbackConfigProvider
builder.Register<RemoteConfigService>(...).As<IConfigService>().As<IRemoteConfigService>();
builder.Register<RemoteConfigEventBridge>(...).AsSelf();                        // SDK events -> game IEventBus

// Analytics
builder.Register<FirebaseAnalyticsSink>(...).As<IAnalyticsSink>();              // #if FIREBASE_ANALYTICS, else DebugAnalyticsSink
builder.Register<AnalyticsService>(...).As<IAnalyticsService>();

// Ads
builder.Register<MaxAdNetwork>(...).As<IAdNetwork>();                           // #if APPLOVIN_MAX, else NullAdNetwork
builder.Register<AdsService>(...).As<IAdsService>();

// IAP
builder.Register<UnityIapStoreBackend>(...).As<IStoreBackend>();                // #if UNITY_IAP, else EditorFakeStoreBackend
builder.Register<RecipeRageIapRewardGrantor>(...).As<IIapRewardGrantor>();
builder.Register<IAPService>(...).As<IIAPService>();
```
SDK services are plain classes with constructor injection — no VContainer inside `Playcenter.Services`. VContainer only wires them at the game's Composition layer (unchanged law). Because SDK services are shared instances, the same objects are also registered into the SDK `ServiceRegistry` by `PlaycenterSdkBootstrap` where modules need them (e.g. `IAnalyticsService`, `IRemoteConfigService` already are).

---

## 6. Delete list ("stop code" + duplicates)

- `Assets/_KitchenClash/Infrastructure/Ads/StubAdsService.cs`
- `Assets/_KitchenClash/Infrastructure/IAP/StubIAPService.cs`
- `Assets/_KitchenClash/Infrastructure/Analytics/FirebaseAnalyticsService.cs`
- `Assets/_KitchenClash/Infrastructure/Firebase/FirebaseAnalyticsService.cs` (duplicate)
- `Assets/_KitchenClash/Infrastructure/Services/CompositeRemoteConfigService.cs`
- `Assets/_KitchenClash/Infrastructure/Services/FallbackRemoteConfigService.cs`

Delete gates: `rg -n "StubAdsService|StubIAPService|FirebaseAnalyticsService|CompositeRemoteConfigService|FallbackRemoteConfigService" Assets --glob '*.cs'` → 0 code hits; `rg -n "using VContainer" Assets/Playcenter --glob '*.cs'` → 0; all assemblies build green.

---

## 7. Error handling

- **Ads:** network not ready / show failure → `ShowInterstitialAsync` returns `false`, `ShowRewardedAsync` returns `AdRewardResult(false, placement)`; never throws into gameplay.
- **Analytics:** sink throws → caught and logged; never crashes gameplay.
- **IAP:** store init failure or purchase failure → `IAPResult(false, productId, reason)`; no currency granted; analytics fail event emitted.
- **RemoteConfig:** provider init/fetch failure → keep serving cache/fallback, `HealthStatus = Degraded`, `OnHealthChanged` raised; `Initialize` still returns `true` so boot is not blocked.

## 8. Testing

- SDK services (`AdsService`, `AnalyticsService`, `IAPService`, `RemoteConfigService`) are pure C# → NUnit EditMode tests with fake `IAdNetwork`/`IAnalyticsSink`/`IStoreBackend`/`IIapRewardGrantor`/`IConfigProvider`, no Unity runtime.
- Cover: ads gating (frequency/min-gap/disable/last-shown), analytics dispatch + sanitizing, IAP success/fail/grant flow, RC cache/fallback/health + C# events.
- Reuse existing `SpyAnalytics`/`DictionaryConfigService` doubles where applicable. Target >80% on new SDK service code.

## 9. Out of scope (YAGNI)

- Real AppLovin MAX / Unity IAP SDK *integration code* beyond the adapter class behind its `#if` (the actual SDK package import + callbacks is a follow-up once packages are added).
- Mediation waterfall / ad-revenue analytics.
- Moving other ports (Audio, Social, Wallet, etc.) — only the four named services.
- Changing any port's public method signatures (consumers keep working unchanged).
