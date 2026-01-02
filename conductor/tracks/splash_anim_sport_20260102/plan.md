# Plan: Splash Screen Animation (Sport Style)

## Phase 1: UI Structure & Styling
- [x] Task: Update `SplashScreenTemplate.uxml` with the "PLAY CENTER" text and structure.
- [x] Task: Update `SplashScreen.uss` with layout, colors, and font styles.
- [ ] Task: Conductor - User Manual Verification 'Phase 1: UI Structure & Styling' (Protocol in workflow.md)

## Phase 2: Animation Implementation
- [ ] Task: Reference new elements in `SplashScreen.cs`.
- [ ] Task: Implement `AnimateSplashScreen` UniTask method with the 4-step sequence.
    - [ ] Global Scale (1.5s)
    - [ ] Border Wipe (1s)
    - [ ] Box Slide (0.8s)
    - [ ] Text Fade (1s)
- [ ] Task: Trigger animation on `OnInitialize`.
- [ ] Task: Conductor - User Manual Verification 'Phase 2: Animation Implementation' (Protocol in workflow.md)

## Phase 3: Final Polish & Verification
- [ ] Task: Refine easing curves to match the reference "cubic-bezier" feel.
- [ ] Task: Verify timing and overlaps between steps.
- [ ] Task: Conductor - User Manual Verification 'Phase 3: Final Polish & Verification' (Protocol in workflow.md)
