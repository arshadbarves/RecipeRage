# Plan: Comprehensive Service & Manager Refactor

## Phase 1: Foundation Services Refactor
Audit and cleanup the low-level services first.

- [x] Task: Refactor Logging & Events
    -   Audit `LoggingService`: Ensure thread safety and standard formatting.
    -   Audit `EventBus`: Verify exception handling in subscribers doesn't crash the bus.
- [x] Task: Refactor Data Services
    -   Audit `SaveService`: Ensure it uses `EncryptionService` and `StorageProviderFactory` via DI.
    -   Audit `NTPTimeService` & `RemoteConfigService`: Check for proper `IInitializable` usage.
- [~] Task: Conductor - User Manual Verification 'Foundation Services' (Protocol in workflow.md)

## Phase 2: Core & State Services Refactor
Address the game logic drivers.

- [ ] Task: Refactor GameState System
    -   Audit `GameStateManager`: Ensure state transitions are safe and logged.
    -   Refactor `StateFactory` to strictly use container resolution for states.
- [ ] Task: Refactor Maintenance & Connectivity
    -   Audit `MaintenanceService`: Ensure it fails gracefully if network is down.
    -   Audit `ConnectivityService`: Verify it properly notifies other services of network changes.
- [ ] Task: Conductor - User Manual Verification 'Core Services' (Protocol in workflow.md)

## Phase 3: UI & Interaction Services Refactor
Ensure UI services are decoupled.

- [ ] Task: Refactor AnimationService
    -   Audit `AnimationService`: Ensure it returns `UniTask` for all animations (completed in previous track, but verify).
- [ ] Task: Refactor UIService
    -   Audit `UIService`: Ensure strict MVVM support and no direct gameplay coupling.
- [ ] Task: Conductor - User Manual Verification 'UI Services' (Protocol in workflow.md)

## Phase 4: Session & Auth Refactor
The critical path for multiplayer.

- [ ] Task: Refactor EOSAuthService
    -   Ensure `EOSAuthService` handles login failures gracefully and logs them.
    -   Verify strict dependency injection for any internal wrappers.
- [ ] Task: Refactor SessionManager
    -   Audit `SessionManager`: Ensure `SessionLifetimeScope` creation is robust and handles errors.
- [ ] Task: Conductor - User Manual Verification 'Session & Auth' (Protocol in workflow.md)
