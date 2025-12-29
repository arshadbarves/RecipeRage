# Plan: Implement EOS Authentication Flow

## Phase 1: Setup & Initialization [checkpoint: 42875cd]
Establishes the module structure and defines the contracts for authentication.

- [x] Task: Create Auth Module Structure 695a603
    - Create directories: `Assets/Scripts/Modules/Auth/{Core,UI,Tests}`.
    - Create `IAuthService.cs` interface with `LoginAsync`, `LogoutAsync`, `IsLoggedIn`, `GetCurrentUserId`.
    - Create Assembly Definition `RecipeRage.Modules.Auth` and reference `RecipeRage.Core`, `Unity.Netcode.Runtime`, `VContainer`, `UniTask`, `Epic.OnlineServices`.
- [x] Task: Configure VContainer & Bootstrapper 6eeab5c
    - Create `AuthScope` or update `GameLifetimeScope` to register `IAuthService`.
    - Create a mock `MockAuthService` for initial testing.
    - Update `Bootstrapper.cs` to resolve and initialize `IAuthService`.
- [x] Task: Create Basic Auth Tests 8ce9fd0
    - Create `Assets/Scripts/Modules/Auth/Tests/AuthServiceTests.cs`.
    - Write tests to verify VContainer resolution and `MockAuthService` behavior.
- [ ] Task: Conductor - User Manual Verification 'Setup & Initialization' (Protocol in workflow.md)

## Phase 2: Core Authentication Logic [checkpoint: e151118]
Implements the actual EOS connection logic using the PlayEveryWare plugin.

- [x] Task: Implement EOSAuthService Skeleton 3186bbd
    - Create `EOSAuthService.cs` implementing `IAuthService`.
    - Implement `Initialize()` to grab the EOS Platform Interface instance.
    - Add logging using the existing `Logging` module.
- [x] Task: Implement Login Logic (DevAuth & DeviceID) 8c49cb5
    - **TDD Red:** Write failing tests for `LoginAsync` in `AuthServiceTests.cs` (mocking EOS calls if possible, or using integration tests).
    - **Green:** Implement `LoginAsync` handling `AuthType.DevAuth` and `AuthType.DeviceID`.
    - Use `EOSManager.Instance.StartLoginWithLoginTypeAndToken`.
- [x] Task: Implement Logout & State Management b9c0327
    - **TDD Red:** Write failing tests for `LogoutAsync` and state checks.
    - **Green:** Implement `LogoutAsync` and `IsLoggedIn`. Ensure local user data is cleared.
- [ ] Task: Conductor - User Manual Verification 'Core Authentication Logic' (Protocol in workflow.md)

## Phase 3: Integration & UI
Connects the backend logic to the frontend UI and integrates with the Friends system.

- [ ] Task: Create Login UI Layout (UXML/USS)
    - Create `LoginView.uxml` and `LoginView.uss` in `Assets/Scripts/Modules/Auth/UI/`.
    - Implement "Skewed Shop" style: Dark background, skewed buttons (-10deg), Red/Yellow accents.
    - Add buttons: "Connect (Device)", "Connect (Epic)".
- [ ] Task: Implement LoginView Logic
    - Create `LoginView.cs` (UI Document Controller).
    - Inject `IAuthService`.
    - Bind buttons to `LoginAsync` calls.
    - Handle loading states (disable buttons, show spinner).
- [ ] Task: Integrate with Friends Module
    - Update `FriendsService` to wait for `IAuthService.IsLoggedIn` before initializing P2P.
    - Listen for Auth events to trigger Friends connection.
- [ ] Task: Conductor - User Manual Verification 'Integration & UI' (Protocol in workflow.md)
