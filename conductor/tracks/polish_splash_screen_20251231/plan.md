# Plan: Polish - Splash Screen Animation

## Phase 1: Setup & Cleanup
- [x] Task: Clean SplashScreen
    -   Remove commented-out binding code.
    -   Cache specific UI elements (`play-container`, `center-container`, labels).
    -   Inject `IAnimationService` directly.

## Phase 2: Animation Implementation
- [x] Task: Implement Intro Animation
    -   Override `AnimateShow` to run a custom `UniTask` sequence.
    -   Sequence:
        1. Scale `play-container` (0 -> 1).
        2. Slide/Scale `center-container`.
        3. Fade in Text.
- [x] Task: Implement Outro Animation
    -   Override `AnimateHide` (or rely on generic fade out).
