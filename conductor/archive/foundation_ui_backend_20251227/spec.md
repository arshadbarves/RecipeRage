# Track Spec: Foundation: UI System and Backend Integration

## Objective
Establish the technical foundation for RecipeRage by integrating essential backend services (EOS and Firebase) and implementing the core UI architecture and design system.

## Scope
### Backend Services
- **Epic Online Services (EOS):**
    - SDK Integration.
    - Authentication (Device ID / External Account).
    - Friends System (Lists, Requests).
    - Presence (Online status).
- **Firebase:**
    - SDK Integration.
    - Remote Config for dynamic game settings.
    - Analytics for player behavior tracking.

### UI Foundation
- **Design System:** Implementation of global USS styles, themes, and common UXML templates based on existing designs.
- **UI Service:** A centralized service for managing screen transitions, popups, and UI state.
- **Core Screens:**
    - Login/Bootstrap Screen.
    - Main Menu Screen.
    - Social/Friends Overlay.

## Constraints & Requirements
- **Unity Version:** 6000.3.0f1
- **Architecture:** Modular and extensible, adhering to the project's established patterns.
- **TDD:** All core logic must be covered by unit tests before implementation.
- **Coverage:** Minimum 80% code coverage for new modules.
- **UI Toolkit:** Use UXML and USS exclusively for UI development.
