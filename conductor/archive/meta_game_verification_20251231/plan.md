# Plan: Meta-Game Loop Verification & Fixes

## Phase 1: Boot & Auth Verification
Verify the startup sequence.

- [x] Task: Verify Boot Sequence
    -   Check `RootBootstrapper` logs.
    -   Check `SplashScreen` transition (using new `UITransitionHandler`).
- [x] Task: Verify Login
    -   Check `LoginViewModel` interaction with `EOSAuthService`.
    -   Verify `SessionLoadingState` triggers and completes.

## Phase 2: Main Menu Verification
Verify the MVVM refactor of the menu.

- [x] Task: Verify Lobby Tab
    -   Check `LobbyViewModel` map loading.
    -   Check "Play" button command.
- [x] Task: Verify Shop Tab
    -   Check `ShopViewModel` currency display.
    -   Check `BuyItem` command flow.
- [x] Task: Verify Settings Tab
    -   Check `SettingsViewModel` volume bindings.

## Phase 3: Matchmaking Flow Verification
Verify the critical path.

- [x] Task: Verify Matchmaking Entry
    -   Check transition to `MatchmakingState`.
    -   Check `MatchmakingScreen` binding to `MatchmakingViewModel`.
- [x] Task: Verify Match Found
    -   Simulate match found event (if possible via Test/Debug).
    -   Check transition to `GameplayState`.

## Phase 4: Bug Fixes (Reactive)
Allocated space for fixing issues found during verification.

- [x] Task: Fix Discovered Issues
    -   (Dynamic task based on findings).
