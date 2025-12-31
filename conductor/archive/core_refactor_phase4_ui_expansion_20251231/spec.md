# Specification: Core Refactor - Phase 4 (UI Expansion & Polish)

## Goal
To finalize the UI modernization by verifying UXML/USS compliance with the new MVVM codebase and implementing the remaining screens (Main Menu Tabs, Matchmaking, Settings) using the new architecture.

## Scope
1.  **UXML/USS Audit:**
    -   Verify `SplashScreen`, `LoginView`, `LoadingScreen` binding IDs match their UXML templates.
    -   Update UXML/USS if IDs are missing or mismatched.
    -   Ensure visual style matches the "Skewed Shop" aesthetic defined in `product-guidelines.md`.
2.  **Main Menu Expansion:**
    -   Implement `LobbyView` (replacing `LobbyTabComponent` logic with proper View class).
    -   Implement `ShopView` and `SettingsView` properly.
    -   Ensure full navigation works via `MainMenuViewModel`.
3.  **New Screens:**
    -   Implement any missing screens based on user design input.

## Success Criteria
-   All bindings in C# (`GetElement<T>("id")`) have matching elements in UXML.
-   No runtime errors regarding missing UI elements.
-   Main Menu sub-tabs are fully functional MVVM implementations.
