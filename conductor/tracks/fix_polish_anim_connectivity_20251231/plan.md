# Plan: Fix & Polish - Animation & Connectivity

## Phase 1: AnimationService Simplification
Remove redundant wrappers.

- [x] Task: Update IAnimationService
    -   Remove `Animate*` methods.
    -   Keep `Kill*` methods (useful global helpers).
    -   Keep `UI` and `Transform` properties.
- [x] Task: Update AnimationService
    -   Remove implementation of `Animate*` methods.
- [x] Task: Fix Usages (If any)
    -   Scan codebase for `AnimateOpacity`, etc., and replace with `UI.FadeIn` or similar. (Note: Previous checks showed `AnimationService` was mostly used via `UI` property in `UIService` anyway, but I should verify `LoadingScreen` etc.)

## Phase 2: ConnectivityService Repair
Fix the broken logic.

- [x] Task: Implement ConnectivityService
    -   Implement `ForceCheckAsync` with `UnityWebRequest`.
    -   Implement `StartMonitoring` with `while` loop.
    -   Ensure `Dispose` cancels the loop.
