# Specification: Fix - Splash Screen Animation Calls

## Goal
To resolve the compilation errors in `SplashScreen.cs` by updating the animation calls to use the direct sub-animator properties (`UI` and `Transform`) from the `AnimationService`, following the recent simplification of the `IAnimationService` interface.

## Scope
1.  **Code Correction:**
    -   Update `SplashScreen.cs` to use `_animationService.UI.ScaleIn(...)` instead of `AnimateScale(...)`.
    -   Update `SplashScreen.cs` to use `_animationService.UI.FadeIn(...)` instead of `AnimateOpacity(...)`.
    -   Ensure all calls use the modern `UniTask` pattern.

## Success Criteria
-   `SplashScreen.cs` compiles without errors.
-   Animation sequence logic remains identical to the intended design.
