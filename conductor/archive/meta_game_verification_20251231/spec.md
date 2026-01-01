# Specification: Meta-Game Loop Verification & Fixes

## Goal
To verify the end-to-end user flow from Application Boot to Gameplay Entry, ensuring that the recent architectural refactors (VContainer, MVVM, Animation, Connectivity) have not introduced regressions.

## Scope
1.  **Boot & Splash:** Verify `GameLifetimeScope` initialization, `SplashScreen` appearance, and transition.
2.  **Authentication:** Verify `LoginView` -> `EOSAuthService` -> `SessionManager` flow.
3.  **Main Menu Navigation:** Verify `MainMenuScreen` tabs (Lobby, Shop, Settings, Character) load and display data.
4.  **Matchmaking Flow:** Verify `LobbyView.Play` -> `MatchmakingState` -> `MatchmakingService` -> `GameplayState`.
5.  **Connectivity Handling:** Verify `ConnectivityService` reports status correctly (simulated).

## Success Criteria
-   Game boots without exceptions.
-   User can log in (Guest).
-   User can navigate all tabs.
-   User can enter Matchmaking and reach `GameplayState` (even if mocked/bot match).
