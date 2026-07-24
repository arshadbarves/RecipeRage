# Monetization decision — soft launch

**Decision:** Ship **without** Unity IAP and **without** AppLovin MAX.
**Analytics/RC:** Keep Debug/Fallback sinks unless Firebase packages are already resolvable; do not block soft launch on Firebase.

## Rationale
- Shared SDK facades are production-shaped (`Playcenter.Services` + `Playcenter.Services.Unity`).
- Vendor packages absent from `Packages/manifest.json` (no `com.unity.purchasing`, no AppLovin MAX, no full Firebase modules).
- Soft-launch scope prioritizes the multiplayer loop.

## Soft-launch surface audit (2026-07-24)
- No Presentation ViewModel/Screen consumes `IIAPService` — the shop (`ShopViewModel`) transacts in soft currency (coins/gems) via `IEconomyService` only. There are no real-money purchase buttons to hide.
- Rewarded-ad CTA (`ResultsViewModel.WatchAdForGemsCommand`) self-hides: `CanShowRewardedAd` is false because `NullAdNetwork.IsRewardedReady == false` when `APPLOVIN_MAX` is off. No hard-lock path.
- `EditorFakeStoreBackend` is only wired when `UNITY_IAP` is undefined AND in editor (`#if UNITY_EDITOR` guard in RootLifetimeScope) — production builds get no fake grants.

## Follow-up (post soft launch)
1. Add `com.unity.purchasing`, implement `UnityIapStoreBackend` `#if UNITY_IAP` purchase callbacks.
2. Add AppLovin MAX Unity plugin; set `APPLOVIN_MAX`; complete `MaxAdNetwork` callbacks.
3. Add Firebase Analytics + Remote Config packages; enable `FIREBASE_ANALYTICS` + `FIREBASE_REMOTE_CONFIG` on Android/iOS.
4. Re-enable Results rewarded CTA and shop SKUs.
