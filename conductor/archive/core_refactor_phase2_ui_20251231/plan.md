# Plan: Core Refactor - Phase 2 (UI Architecture & Localization)

## Phase 1: Foundation (MVVM & Localization)
Build the core systems required for the new UI architecture.

- [x] Task: MVVM & Localization Core
    -   Create `BindableProperty<T>` (lightweight reactive property).
    -   Create `BaseViewModel` abstract class.
    -   Implement `LocalizationManager` (Load CSV, Dictionary lookup).
    -   Create `ILocalizable` interface for UI elements.
    -   Register `LocalizationManager` in `GameLifetimeScope`.

## Phase 2: Animation System
Standardize DOTween usage for UI.

- [x] Task: UITransitionHandler
    -   Create `UITransitionType` enum (Fade, Slide, Scale).
    -   Implement `UITransitionHandler` that takes a `VisualElement` and `UITransitionType`.
    -   Update `BaseUIScreen` to use this handler for Show/Hide logic.

## Phase 3: Pilot Implementation (Splash & Login)
Apply the new architecture to the initial screens.

- [x] Task: Refactor Splash Screen
    -   Create `SplashScreenViewModel`.
    -   Update `SplashScreen` to bind to ViewModel.
    -   Replace manual animations with `UITransitionHandler` (Fade).
    -   Localize "Loading..." text.
- [x] Task: Refactor Login Screen
    -   Create `LoginViewModel` (handling Auth logic).
    -   Update `LoginView` to bind to ViewModel.
    -   Use `UITransitionHandler`.
    -   Localize buttons and status text.

## Phase 4: Integration
Ensure the flow works.

- [x] Task: Verify Flow
    -   Check `BootstrapState` -> `SplashScreen` -> `LoginScreen` transition.
    -   Verify Language Switching (Debug command).
