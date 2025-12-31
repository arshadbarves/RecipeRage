# Specification: Core Refactor - Phase 2.5 (Main Menu & Loading UI)

## Goal
To extend the modern UI architecture (MVVM + Localization + Standard Transitions) to the `LoadingScreen` and `MainMenuScreen`. This includes decoupling the Main Menu's sub-components (Lobby, Shop, etc.) into distinct ViewModels.

## Scope
1.  **Loading Screen Refactor:**
    -   Create `LoadingViewModel`.
    -   Implement localization for "Status" and "Tips".
    -   Bind `Progress` and `Version` text.
2.  **Main Menu Refactor:**
    -   Create `MainMenuViewModel`.
    -   Create ViewModels for Tabs: `LobbyViewModel`, `ShopViewModel`, `SettingsViewModel`, `CharacterViewModel`.
    -   Refactor `MainMenuScreen` to orchestrate these ViewModels instead of managing logic directly.
    -   Replace manual `LobbyTabComponent` logic with binding.
3.  **Cleanup:**
    -   Remove unnecessary localization from `SplashScreen` (Logo only).
    -   Delete legacy UI components that are fully replaced.

## Success Criteria
-   `LoadingScreen` cycles through localized tips.
-   `MainMenuScreen` code is minimal (mostly binding setup).
-   Tabs (Lobby, Shop) function correctly via their ViewModels.
-   Legacy `Component` classes are removed or thinned out.
