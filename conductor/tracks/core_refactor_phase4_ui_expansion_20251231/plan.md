# Plan: Core Refactor - Phase 4 (UI Expansion & Polish)

## Phase 1: Template Verification
Ensure the code we wrote matches the assets.

- [x] Task: Verify Splash & Login Templates
    -   Check `SplashScreenTemplate.uxml` for `splash-logo`, `loading-text` (if added back).
    -   Check `LoginView.uxml` (or equivalent) for `guest-login-button`, `status-text`, `loading-indicator`.
    -   Update files if mismatches found.
- [x] Task: Verify Loading Template
    -   Check `LoadingScreenTemplate.uxml` for `progress-fill`, `status-text`, `percentage`, `tip-text`, `tip-title`, `version-info`.

## Phase 2: Main Menu Views
Convert Components to Views.

- [x] Task: Refactor Lobby
    -   Create `LobbyView` (UXML/CS) or update `LobbyTabComponent` to be purely View.
    -   Ensure `Play` button binds to `LobbyViewModel`.
- [x] Task: Refactor Shop
    -   Create/Update `ShopView` to bind `CoinsText`, `GemsText` to `ShopViewModel`.
- [x] Task: Refactor Settings
    -   Create/Update `SettingsView` to bind Volume/Quality to `SettingsViewModel`.

## Phase 3: Visual Polish
- [x] Task: Apply Styling
    -   Apply "Skewed Shop" style to Main Menu buttons if not present.
    -   Update USS files.
