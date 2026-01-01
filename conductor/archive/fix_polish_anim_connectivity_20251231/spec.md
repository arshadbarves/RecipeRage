# Specification: Fix & Polish - Animation & Connectivity

## Goal
To finalize the AnimationService architecture by removing redundant wrapper methods (strictly adhering to the Facade pattern by exposing sub-animators) and to repair the broken `ConnectivityService` implementation.

## Scope
1.  **AnimationService Refactor:**
    -   Remove `AnimateOpacity`, `AnimatePosition`, `AnimateScale`, `AnimateRotation` methods from `IAnimationService` and `AnimationService`.
    -   Remove `AnimateTransformPosition`, `AnimateTransformScale`, `AnimateTransformRotation` methods.
    -   Expose `IUIAnimator UI { get; }` and `ITransformAnimator Transform { get; }` as the primary API.
    -   Ensure `IUIAnimator` and `ITransformAnimator` are robust enough to handle the needs (e.g. they already return UniTask).

2.  **ConnectivityService Repair:**
    -   Implement `ForceCheckAsync` correctly (remove `NotImplementedException`).
    -   Ensure `StartMonitoring` loop is implemented and robust (checking `_isRunning`).
    -   Ensure correct usage of `UnityWebRequest` with `UniTask`.

## Success Criteria
-   `AnimationService` is a thin Facade (only properties, Initialize, KillAll).
-   `ConnectivityService` compiles and has functional logic for `ForceCheckAsync`.
