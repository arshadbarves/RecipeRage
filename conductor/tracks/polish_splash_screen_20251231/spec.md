# Specification: Polish - Splash Screen Animation

## Goal
To implement a high-quality, sequenced animation for the Splash Screen using the `AnimationService` (UniTask), replacing the generic fade. This adds professional polish to the game startup.

## Scope
1.  **Animation Sequence:**
    -   Initial state: Elements hidden/scaled down.
    -   **Step 1:** `play-skewed-border` (Left Box) scales in/fades in.
    -   **Step 2:** `center-skewed-box` (Right Box) slides in from right or scales in.
    -   **Step 3:** Text labels ("PLAY", "CENTER", "STUDIO") fade in.
    -   **Step 4:** Hold for duration.
    -   **Step 5:** Fade out entire screen.

2.  **Implementation:**
    -   Update `SplashScreen.cs` to use `AnimationService` directly for specific elements.
    -   Remove the generic `TransitionType = Fade` logic for the *content* (keep it for the screen container or override `AnimateShow`).
    -   Clean up commented-out ViewModel code.

## Success Criteria
-   Splash screen plays a choreographed animation sequence.
-   Code is clean (no commented-out blocks).
-   Uses `AnimationService` UniTask methods.
