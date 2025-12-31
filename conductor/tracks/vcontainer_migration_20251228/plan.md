# Plan: Migrate Core to VContainer

## Phase 1: Foundation Setup [checkpoint: 21e83cd]
Establish the root LifetimeScope and register foundational services.

- [x] Task: Create GameLifetimeScope [2f4245d]
    - Create `Assets/Scripts/Core/Bootstrap/GameLifetimeScope.cs`.
    - Register: `EventBus`, `SaveService`, `NTPTimeService`, `RemoteConfigService`, `AnimationService`, `UIService`, `ConnectivityService`.
    - Inject `UIDocumentProvider` from the scene into `UIService`.
- [x] Task: Register Core Services [2f4245d]
    - Register: `MaintenanceService`, `AuthService` (EOSAuthService), `StateManager`.
    - Ensure `AuthService` is correctly configured with its constructor dependencies.
- [x] Task: Update GameBootstrap [2f4245d]
    - Replace `ServiceContainer` instantiation with `LifetimeScope`.
    - Update `BootstrapState` to resolve dependencies from the container.
- [x] Task: Conductor - User Manual Verification 'Foundation Setup' (Protocol in workflow.md) [86a4994]

## Phase 2: Session Management [checkpoint: 6655d03]
Migrate user-specific services to a dynamic session scope.

- [x] Task: Create SessionLifetimeScope [73798a0]
    - Create `Assets/Scripts/Core/Bootstrap/SessionLifetimeScope.cs`.
    - Register: `CurrencyService`, `AudioService`, `InputService`, `GameModeService`, `CharacterService`, `SkinsService`, `NetworkingServices`, `NetworkGameManager`, `PlayerNetworkManager`, `NetworkObjectPool`.
- [x] Task: Update Session Creation Logic [73798a0]
    - Update `AuthService` or a dedicated `SessionManager` to create the `SessionLifetimeScope` upon login.
    - Ensure proper disposal of the scope on logout.
- [x] Task: Conductor - User Manual Verification 'Session Management' (Protocol in workflow.md) [c7b6929]

## Phase 3: Cleanup & Refinement
- [x] Task: Remove Legacy Container Files [fa17591]
    - Delete `ServiceContainer.cs` and `GameSession.cs`.
    - Fix any remaining direct references to `GameBootstrap.Services`.
- [ ] Task: Refactor UI Injection
    - Update screens (e.g., `MainMenuScreen`) to use VContainer property injection or manual resolution from the scope if necessary.
- [ ] Task: Conductor - User Manual Verification 'Cleanup & Refinement' (Protocol in workflow.md)
