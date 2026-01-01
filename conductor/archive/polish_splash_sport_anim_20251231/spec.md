# Specification: Polish - Splash Screen "Rounded Sport" Animation

## Goal
To implement a "Rounded Sport" style animation for the Splash Screen, mimicking the provided HTML/CSS reference. This involves a coordinated sequence of border wiping, text fading, and sliding elements.

## Scope
1.  **Global Animation:**
    -   Scale entire logo container (`master-container`) from 0.9 to 1.0.
2.  **"PLAY" Box (Left):**
    -   **Border Wipe:** Simulate a border drawing effect (Wipe Left-to-Right).
    -   **Text Fade:** "PLAY" text fades in (0 -> 1).
3.  **"CENTER" Box (Right):**
    -   **Slide & Fade:** Slide in from right (TranslateX 30px -> 0) + Fade In.
    -   This element is `center-container`.

## Technical Approach
-   **Border Wipe:** Since UI Toolkit lacks `clip-path`, we will simulate this by animating the `width` of the `play-skewed-border` element from 0% to 100% (or masking it).
-   **Slide:** Use `DOTween` to animate `translate.x`.
-   **Sequence:** Use `UniTask` to choreograph the timings (staggered starts).

## Success Criteria
-   Animation matches the reference feel (Ease Out, specific timings).
-   Implemented in `SplashScreen.cs` using `DOTween` directly (since these are custom effects, not standard screen transitions).
