# Specification: Core Refactor - Phase 2 (UI Architecture & Localization)

## Goal
To modernize the UI architecture by implementing the MVVM pattern, standardizing DOTween animations, and integrating a lightweight, spreadsheet-compatible localization system. The refactor will start with the `SplashScreen` and `LoginScreen` as pilots before expanding to `MainMenu`.

## Scope
1.  **MVVM Infrastructure:**
    -   Create `BaseViewModel` with `INotifyPropertyChanged` or ReactiveProperty support.
    -   Update `BaseUIScreen` to bind to a `ViewModel`.
2.  **Animation System:**
    -   Implement `UITransitionHandler` that uses DOTween.
    -   Support standardized presets: `Fade`, `Slide(Direction)`, `Scale`, `Punch`.
    -   Ensure animations are non-blocking but awaitable (`UniTask`).
3.  **Localization System:**
    -   Create `LocalizationManager` (Singleton in GameScope).
    -   Support CSV parsing (Key, Language Columns) to mimic Google Sheet export.
    -   Create `LocalizedLabel` custom UI control or a binding system to auto-localize text.
4.  **Refactor Screens:**
    -   **Splash Screen:** Convert to MVVM + Localization + Fade Animation.
    -   **Login Screen:** Convert to MVVM + Localization + Slide/Fade Animation.

## Success Criteria
-   `SplashScreen` has NO logic code, only binding code.
-   Animations are defined via configuration/enum, not manual DOTween code in the View.
-   Changing language at runtime updates the UI immediately.
-   Adding a new localized string is as simple as adding a row to a CSV.
