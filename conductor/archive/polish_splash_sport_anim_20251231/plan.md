# Plan: Polish - Splash Screen "Rounded Sport" Animation

## Phase 1: Setup Elements
- [x] Task: Identify UI Elements
    -   Cache `play-skewed-border` (for wipe).
    -   Cache `play-text`.
    -   Cache `center-container` (red box).
    -   Cache `master-container` (global scale).

## Phase 2: Implement Animation Logic
- [x] Task: Create Animation Sequence
    -   **Global:** Scale `master-container` 0.9 -> 1.0 (1.5s).
    -   **Border:** Animate `play-skewed-border` Width 0% -> 100% (1s).
    -   **Play Text:** Fade In (1s).
    -   **Center Box:** Fade In + TranslateX 30px -> 0 (0.8s, delay 0.2s).
- [x] Task: Cleanup
    -   Ensure `OnDispose` kills tweens.
