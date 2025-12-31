# Technical Debt Report - Core Refactor Assessment
**Date:** 2025-12-31
**Status:** Assessment Complete

## 1. Architecture & Dependency Injection
**Severity:** Critical

The codebase is in a transition state. While VContainer has been introduced, legacy patterns persist, creating a "split brain" architecture where some parts use strict DI and others use Service Location.

*   **Issue:** `GameBootstrap.Instance` exposes the DI Container globally.
    *   **Impact:** Encourages "Service Locator" pattern, making dependencies hidden and testing difficult.
    *   **Violation:** `Assets/Scripts/Core/Bootstrap/GameBootstrap.cs`
*   **Issue:** Manual dependency injection in `Awake()`.
    *   **Impact:** Classes like `PlayerController` manually ask for dependencies instead of having them injected, bypassing the safety of the container.
    *   **Violation:** `Assets/Scripts/Core/Characters/PlayerController.cs`
*   **Issue:** Usage of `FindFirstObjectByType`.
    *   **Impact:** Fragile coupling to scene structure; breaks if objects are missing or duplicated.
    *   **Violation:** `GameplayState.cs`, `GameOverState.cs`

## 2. UI/Logic Coupling
**Severity:** High

The UI layer is doing too much work. View classes are calculating game logic, managing timers, and handling physics (raycasting), violating the Single Responsibility Principle.

*   **Issue:** Logic Leakage (View doing Model work).
    *   **Impact:** UI classes are hard to test and modify without breaking game logic.
    *   **Violation:** `GameplayUIManager.cs` (Raycasting, Timers), `OrderUIItem.cs` (Time calc), `ScoreUI.cs` (Score calc).
*   **Issue:** Heavy Service Coupling in Views.
    *   **Impact:** Screens like `MainMenuScreen` are "God Views" that know about too many subsystems (Auth, Save, Session, etc.).
    *   **Violation:** `MainMenuScreen.cs`, `MatchmakingScreen.cs`

## 3. State Management
**Severity:** Medium

Mutable state is exposed publicly in some core controllers, leading to unpredictable behavior and hard-to-track bugs.

*   **Issue:** Public mutable fields.
    *   **Impact:** Any system can change player stats, making it impossible to track *who* changed it or *why*.
    *   **Violation:** `PlayerController.cs` (`InteractionSpeedModifier`, `CarryingCapacity`)

## 4. Modularity (Gameplay)
**Severity:** Low (Good News!)

The Gameplay module appears to be the healthiest part of the codebase regarding modularity. It does not directly reference UI or legacy singletons, suggesting the "Gameplay" domain is well-isolated.

---

## Recommendations for Refactoring

### Priority 1: Kill the Service Locator
1.  Remove `GameBootstrap.Instance` static property.
2.  Refactor `PlayerController` to use `[Inject]` (Method or Property) or Factory injection.
3.  Replace `FindFirstObjectByType` in States with proper constructor injection (since States are now resolved by VContainer).

### Priority 2: MVVM for UI
1.  Introduce a **ViewModel** layer for complex UIs (`GameplayUIManager`, `MainMenuScreen`).
2.  Move logic (raycasting, time calc) out of Views and into Controllers or Systems.

### Priority 3: Encapsulate State
1.  Convert public fields in `PlayerController` to properties with private setters.
2.  Use a `StatModificationSystem` or similar pattern for changing gameplay values.
