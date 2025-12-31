# Plan: Core Refactor - Phase 2.5 (Main Menu & Loading UI)

## Phase 1: Loading Screen Modernization
- [x] Task: Update Localization Data
    -   Add keys for `loading_status_*`, `loading_tip_title`, `loading_tips_*`.
- [x] Task: Create LoadingViewModel
    -   Expose `StatusText`, `Progress`, `TipTitle`, `TipText`.
    -   Implement tip cycling logic (with `UniTask` delay).
- [x] Task: Refactor LoadingScreen
    -   Inject `LoadingViewModel`.
    -   Bind UI elements.
    -   Use `UITransitionHandler` (Fade/Slide).

## Phase 2: Main Menu Infrastructure
- [x] Task: Create Sub-ViewModels
    -   `LobbyViewModel` (Matchmaking state).
    -   `ShopViewModel` (Currency, Items).
    -   `SettingsViewModel` (Audio/Graphics bindings).
    -   `CharacterViewModel` (Selection logic).
- [x] Task: Create MainMenuViewModel
    -   Manage active tab state (`BindableProperty<MainMenuTab>`).
    -   Expose sub-viewmodels.

## Phase 3: Main Menu Integration
- [x] Task: Refactor MainMenuScreen
    -   Inject `MainMenuViewModel`.
    -   Bind Tab buttons to ViewModel commands.
    -   Bind sub-views to their respective ViewModels.
    -   Remove legacy `*TabComponent` logic classes if possible, or convert them to View-only helpers.

## Phase 4: Cleanup
- [x] Task: Cleanup SplashScreen
    -   Remove localization binding (Logo only).
    -   Verify `SplashScreenViewModel` is minimal.
