# Specification: Splash Screen Animation (Sport Style)

## Overview
Implement a high-polish, dynamic splash screen animation for RecipeRage using the "Skewed Shop" aesthetic. The animation will feature a "PLAY CENTER" logo with a border wipe effect, sliding boxes, and scaling transitions, mimicking the provided HTML/CSS reference.

## Functional Requirements
- **Logo Text:** Display "PLAY" in a hollow skewed box (left) and "CENTER" in a solid skewed box (right).
- **Animation Sequence:**
    1.  **Global Scale:** The entire logo wrapper scales from 0.9 to 1.0 over 1.5s using a cubic-bezier-like easing.
    2.  **Border Wipe:** The "PLAY" box border wipes in from left to right over 1s using the `border-progress` property of `SkewedBoxElement`.
    3.  **Box Slide:** The "CENTER" box fades in and slides from right (+30px) to its final position over 0.8s.
    4.  **Text Fade:** The "PLAY" and "CENTER" text labels fade in over 1s.
- **Font:** Use the "Anton-Regular" font for all logo text.
- **Technology:** Use `UniTask` to sequence the animations within the `SplashScreen.cs` controller.

## Visual Requirements
- **Background:** Premium dark radial gradient (simulated with a background image or nested VisualElements).
- **Colors:** 
    - "PLAY" box: White border (2px), no fill.
    - "CENTER" box: Red fill (`#E63946`), white text.
- **Skew:** All boxes must use a -12 degree skew (consistent with existing project UI).
- **Corners:** 5px corner radius for both skewed boxes.

## Acceptance Criteria
- [ ] The animation triggers automatically when the Splash screen is initialized.
- [ ] The "PLAY" border wipe uses the custom `SkewedBoxElement` logic.
- [ ] Text remains legible and correctly centered within the skewed containers.
- [ ] The sequence matches the timing and feel of the provided reference.
- [ ] No errors or warnings in the Unity console during the animation.

## Out of Scope
- Interactive elements (buttons, clicks) during the splash sequence.
- Loading bar implementation (handled by a separate `LoadingScreen`).
