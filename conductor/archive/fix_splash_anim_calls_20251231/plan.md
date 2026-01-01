# Plan: Fix - Splash Screen Animation Calls

## Phase 1: Implementation
- [x] Task: Update SplashScreen Animation Calls
    -   Replace `AnimateScale` with `UI.ScaleIn`.
    -   Replace `AnimateOpacity` with `UI.FadeIn`.
    -   Verify all `UniTask` calls are awaited correctly.

## Phase 2: Verification
- [x] Task: Verify Compilation
    -   Ensure the project compiles.
