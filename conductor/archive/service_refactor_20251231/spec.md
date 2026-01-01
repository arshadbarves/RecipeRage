# Specification: Comprehensive Service & Manager Refactor

## Goal
To systematically review and refactor every service and manager registered in `GameLifetimeScope` to ensure adherence to best practices in Dependency Injection, Single Responsibility, Interface Segregation, Error Handling, and Initialization.

## Scope
The following services (registered in `GameLifetimeScope`) will be audited and refactored:

1.  **Foundation:** `LoggingService`, `EventBus`, `StorageProviderFactory`, `EncryptionService`, `SaveService`, `NTPTimeService`, `RemoteConfigService`, `ConnectivityService`.
2.  **UI System:** `AnimationService` (and animators), `UIService`.
3.  **Core:** `MaintenanceService`, `GameStateManager`, `StateFactory`.
4.  **Session & Auth:** `SessionManager`, `EOSAuthService`.

## Functional Requirements

### 1. Dependency Injection (Strict)
*   **Requirement:** All dependencies must be injected via **constructor**.
*   **Constraint:** No usage of `GameBootstrap.Instance`, `Container.Resolve` (except in Factories), or `Object.Find*` inside service logic.

### 2. Single Responsibility & Interface Segregation
*   **Requirement:** Services should have a single, well-defined purpose.
*   **Requirement:** Interfaces (e.g., `ISaveService`) should be focused. If an interface is too large, it should be split (e.g., `ISaveReader`, `ISaveWriter`) or the implementation refactored.

### 3. Error Handling & Logging
*   **Requirement:** All exceptions must be caught and logged using `GameLogger.LogException`.
*   **Requirement:** Critical failures during initialization should prevent the game from proceeding or show an error state.
*   **Requirement:** Use structured logging (standardized prefixes/categories) for easier debugging.

### 4. Initialization Standardization
*   **Requirement:** Services requiring initialization must implement `IInitializable` (VContainer) or a custom `IAsyncInitializable` if async setup is needed.
*   **Requirement:** Initialization order must be deterministic where dependencies exist.

## Acceptance Criteria
*   All services in `GameLifetimeScope` use constructor injection.
*   No "Service Locator" patterns remain in the Core module.
*   All services define and implement a corresponding interface.
*   Error handling is consistent across all reviewed services.
