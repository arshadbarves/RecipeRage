# Specification: EOS Authentication Flow

## Goal
Replace the placeholder authentication system with a fully functional module powered by Epic Online Services (EOS). This will enable players to sign in, maintain persistent sessions, and provide the necessary identity context for social features (Friends, Chat) and multiplayer networking.

## Core Requirements

### 1. EOS Initialization
- The `Auth` module must correctly initialize the EOS Platform Interface via the `com.playeveryware.eos` plugin.
- It must handle platform-specific tick/update loops if required by the plugin.
- It must ensure the EOS SDK is shutdown correctly on application exit.

### 2. Authentication Service (`IAuthService`)
- Define a clear interface for authentication to decouple implementation from usage.
- **Methods:**
    - `UniTask<bool> LoginAsync(AuthType type)`
    - `UniTask LogoutAsync()`
    - `bool IsLoggedIn()`
    - `string GetCurrentUserId()` (Returns PUID)
- **Auth Types:**
    - `DeviceID`: For simple persistent guest login.
    - *Future: Google, Facebook*

### 3. Integration
- **Bootstrapper:** Update the game's entry point to initialize `AuthService` *before* `FriendsService`.
- **Dependency Injection:** Register `IAuthService` and `EOSAuthService` in the VContainer scope (or ServiceContainer).
- **Friends Dependency:** Ensure the Friends module waits for a valid user ID before attempting to connect.

### 4. User Interface
- Create a `LoginView` using UI Toolkit.
- **Style:** Adhere to the "Skewed Shop" aesthetic (Dark, Red/Yellow accents, Skewed buttons).
- **Functionality:**
    - Display Status (Initializing, Ready, Logging In, Error).
    - Buttons for "Login with Device ID".

## Technical Architecture

### Module Structure
```
Assets/Scripts/Modules/Auth/
├── Core/
│   ├── IAuthService.cs
│   ├── EOSAuthService.cs
│   └── AuthConfig.cs
├── UI/
│   ├── LoginView.cs
│   ├── LoginView.uxml
│   └── LoginView.uss
└── Tests/
    ├── AuthTests.cs
    └── Mocks/
```

### Data Flow
1. Game Start -> Bootstrapper -> `AuthService.Initialize()`
2. UI shows Login Screen.
3. User clicks "Login".
4. `AuthService` calls EOS `ConnectInterface.Login`.
5. On Success:
    - Cache `ProductUserId`.
    - Fire `OnLoginSuccess` event.
    - `FriendsService` listens to event and connects.
    - UI transitions to Main Menu.