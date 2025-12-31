# Specification: Migrate Core to VContainer

## Goal
Replace the manual `ServiceContainer` and `GameSession` pattern with VContainer's dependency injection framework to improve maintainability, testability, and decouple service management from the `GameBootstrap` entry point.

## Proposed Architecture

### 1. Root Scope (`GameLifetimeScope`)
- Replaces the foundation and core eager services.
- **Service Registrations:**
    - Foundation: `EventBus`, `AnimationService`, `UIService`, `SaveService`, `NTPTimeService`, `RemoteConfigService`, `ConnectivityService`.
    - Core: `MaintenanceService`, `AuthService`, `StateManager`.
- **Implementation:** `GameLifetimeScope.cs` inheriting from `VContainer.Unity.LifetimeScope`.

### 2. Session Scope (`SessionLifetimeScope`)
- Replaces `GameSession`.
- Created dynamically upon successful login.
- **Service Registrations:**
    - Application: `CurrencyService`, `AudioService`, `InputService`.
    - Game Systems: `GameModeService`, `CharacterService`, `SkinsService`, `NetworkingServices`.
    - Networking: `NetworkGameManager`, `PlayerNetworkManager`, `NetworkObjectPool`.

### 3. Entry Point (`GameBootstrap`)
- Simplified to only initiate the `GameLifetimeScope`.
- Logic for state transitions (`BootstrapState`) will now resolve dependencies via the container.

## Migration Steps
1.  **Define Scopes:** Create `GameLifetimeScope.cs` and `SessionLifetimeScope.cs`.
2.  **Refactor Services:** Update service constructors to use standard DI patterns (already largely done).
3.  **Bootstrap Update:** Replace `ServiceContainer` instantiation with `LifetimeScope` usage.
4.  **Cleanup:** Remove `ServiceContainer.cs` and `GameSession.cs`.

## Critical Considerations
- **Circular Dependencies:** Ensure `UIService` and `UIDocumentProvider` are handled correctly within the VContainer registration flow.
- **Manual Resolution:** Avoid using `Object.FindObjectOfType` or manual singletons where possible; rely on `[Inject]` or constructor injection.
- **Cleanup logic:** Use `IDisposable` implementation in VContainer to mirror current manual disposal logic.
